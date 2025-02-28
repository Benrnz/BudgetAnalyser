using System.Globalization;
using System.Text;
using System.Text.Json;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Ledger;

/// <summary>
///     A repository for Ledger Books that persists to local disk in Xaml format.
/// </summary>
/// <seealso cref="ILedgerBookRepository" />
[AutoRegisterWithIoC(SingleInstance = true)]
internal class JsonOnDiskLedgerBookRepository(
    IDtoMapper<LedgerBookDto, LedgerBook> mapper,
    BankImportUtilities importUtilities,
    IReaderWriterSelector readerWriterSelector,
    ILogger logger)
    : ILedgerBookRepository
{
    private readonly BankImportUtilities importUtilities = importUtilities ?? throw new ArgumentNullException(nameof(importUtilities));
    private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IDtoMapper<LedgerBookDto, LedgerBook> mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IReaderWriterSelector readerWriterSelector = readerWriterSelector ?? throw new ArgumentNullException(nameof(readerWriterSelector));

    public async Task<LedgerBook> CreateNewAndSaveAsync(string storageKey)
    {
        if (storageKey.IsNothing())
        {
            throw new ArgumentNullException(nameof(storageKey));
        }

        var book = new LedgerBook { Name = Path.GetFileNameWithoutExtension(storageKey).Replace('.', ' '), StorageKey = storageKey, Modified = DateTime.Now };

        await SaveAsync(book, storageKey, false);
        return await LoadAsync(storageKey, false);
    }

    public async Task<LedgerBook> LoadAsync(string storageKey, bool isEncrypted)
    {
        if (storageKey.IsNothing())
        {
            throw new ArgumentNullException(nameof(storageKey));
        }

        LedgerBookDto dataEntity;
        try
        {
            this.importUtilities.AbortIfFileDoesntExist(storageKey);
            dataEntity = await LoadJsonFromDiskAsync(storageKey, isEncrypted);
        }
        catch (FileNotFoundException ex)
        {
            this.logger.LogError(ex, _ => $"File not found: {storageKey}");
            throw new KeyNotFoundException("Data file can not be found: " + storageKey, ex);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, _ => $"General Error loading and deserialising, likely invalid file format: {storageKey}");
            throw new DataFormatException("Deserialisation Ledger Book file failed, an exception was thrown by the Json deserialiser, the file format is invalid.", ex);
        }

        if (dataEntity is null)
        {
            this.logger.LogError(_ => $"General Error loading and deserialising, likely invalid file format: {storageKey}");
            throw new DataFormatException(string.Format(CultureInfo.CurrentCulture, "The specified file {0} is not of type Data-Ledger-Book", storageKey));
        }

        dataEntity.StorageKey = storageKey;
        var book = this.mapper.ToModel(dataEntity);

        var messages = new StringBuilder();
        if (!book.Validate(messages))
        {
            this.logger.LogError(_ => $"The ledger book failed validation after deserialisation: {storageKey}. Validation messages: {messages}");
            throw new DataFormatException(messages.ToString());
        }

        if (Math.Abs(dataEntity.Checksum - -1) < 0.0001)
        {
            // bypass checksum check - this is to allow intentional manual changes to the file.  This checksum is only trying to prevent
            // bugs in code from breaking the consistency of the file.
        }
        else
        {
            var calculatedChecksum = CalculateChecksum(book);
            if (Math.Abs(calculatedChecksum - dataEntity.Checksum) > 0.0001)
            {
                this.logger.LogError(_ => $"The ledger book failed checksum validation: {storageKey}. Expected: {dataEntity.Checksum}, Actual: {calculatedChecksum}");
                throw new DataFormatException("The Ledger Book has been tampered with, checksum should be " + calculatedChecksum);
            }
        }

        return book;
    }

    public async Task SaveAsync(LedgerBook book, string storageKey, bool isEncrypted)
    {
        if (book is null)
        {
            throw new ArgumentNullException(nameof(book));
        }

        if (storageKey.IsNothing())
        {
            throw new ArgumentNullException(nameof(storageKey));
        }

        var dataEntity = MapToDto(book);
        book.StorageKey = storageKey;
        dataEntity.StorageKey = storageKey;
        dataEntity.Checksum = CalculateChecksum(book);
        this.logger.LogInfo(_ => $"Saving Ledger Book to disk: {storageKey}. Checksum is: {dataEntity.Checksum}");

        await SaveDtoToDiskAsync(dataEntity, isEncrypted);
    }

    protected virtual async Task<LedgerBookDto> LoadJsonFromDiskAsync(string fileName, bool isEncrypted)
    {
        var reader = this.readerWriterSelector.SelectReaderWriter(isEncrypted);
        await using var stream = reader.CreateReadableStream(fileName);
        var ledgerBookDto = await JsonSerializer.DeserializeAsync<LedgerBookDto>(stream);

        return ledgerBookDto ?? throw new CorruptedLedgerBookException("Unable to deserialise ledger book data into correct type.");
    }

    protected virtual LedgerBookDto MapToDto(LedgerBook book)
    {
        return this.mapper.ToDto(book);
    }

    protected virtual async Task SerialiseAndWriteToStream(Stream stream, LedgerBookDto dataEntity)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        await JsonSerializer.SerializeAsync(stream, dataEntity, options);
    }

    private double CalculateChecksum(LedgerBook dataEntity)
    {
        // ReSharper disable once EnumerableSumInExplicitUncheckedContext - Used to calculate a checksum and revolving (overflowing) integers are ok here.
        return dataEntity.Reconciliations.Sum(
            l =>
                (double)l.LedgerBalance
                + l.BankBalanceAdjustments.Sum(b => (double)b.Amount)
                + l.Entries.Sum(e => (double)e.Balance));
    }

    private async Task SaveDtoToDiskAsync(LedgerBookDto dataEntity, bool isEncrypted)
    {
        if (dataEntity is null)
        {
            throw new ArgumentNullException(nameof(dataEntity));
        }

        var writer = this.readerWriterSelector.SelectReaderWriter(isEncrypted);
        await using var stream = writer.CreateWritableStream(dataEntity.StorageKey);
        await SerialiseAndWriteToStream(stream, dataEntity);
    }
}
