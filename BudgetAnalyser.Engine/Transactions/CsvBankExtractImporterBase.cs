using System.Globalization;
using BudgetAnalyser.Engine.BankAccount;

namespace BudgetAnalyser.Engine.Transactions;

/// <summary>
///     Shared behaviour for the single-header-line CSV bank extract importers: read the file, skip the header
///     line, parse each remaining line into a <see cref="Transaction" />, and taste-test a file by checking its
///     header and first data line. Importers whose file layout doesn't fit this shape (variable metadata, a
///     movable header row) override <see cref="LoadAsync" /> and <see cref="TasteTestAsync" /> directly.
/// </summary>
internal abstract class CsvBankExtractImporterBase : IBankExtractImporter
{
    protected CsvBankExtractImporterBase(BankImportUtilities importUtilities, ILogger logger, IReaderWriterSelector readerWriterSelector)
    {
        ImportUtilities = importUtilities ?? throw new ArgumentNullException(nameof(importUtilities));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ReaderWriterSelector = readerWriterSelector ?? throw new ArgumentNullException(nameof(readerWriterSelector));
        ImportUtilities.ConfigureLocale(new CultureInfo("en-NZ"));
        // All current importers are NZ specific at this stage.
    }

    /// <summary>The exact column header line this importer expects, excluding any trailing "\r".</summary>
    protected abstract string ExpectedHeaderLine { get; }

    protected BankImportUtilities ImportUtilities { get; }
    protected ILogger Logger { get; }
    protected IReaderWriterSelector ReaderWriterSelector { get; }

    /// <summary>
    ///     Load the given file into a <see cref="TransactionsListModel" />.
    /// </summary>
    /// <param name="fileName">The file to load.</param>
    /// <param name="account">
    ///     The account to classify these transactions. This is useful when merging one extract to another. For example, merging a cheque account export with visa account export, each can be
    ///     classified using an account.
    /// </param>
    public virtual async Task<TransactionsListModel> LoadAsync(string fileName, Account account)
    {
        try
        {
            ImportUtilities.AbortIfFileDoesntExist(fileName);
        }
        catch (FileNotFoundException ex)
        {
            throw new KeyNotFoundException(ex.Message, ex);
        }

        var transactions = new List<Transaction>();
        var firstTime = true;
        foreach (var line in await ReadLinesAsync(fileName))
        {
            if (firstTime)
            {
                // File contains column headers
                firstTime = false;
                continue;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            transactions.Add(ParseLine(line.Split(','), account));
        }

        return new TransactionsListModel(Logger) { StorageKey = fileName, LastImport = DateTime.Now }.LoadTransactions(transactions);
    }

    /// <summary>
    ///     Test the given file to see if this importer implementation can read and import it.
    ///     This will open and read some of the contents of the file.
    /// </summary>
    public virtual async Task<bool> TasteTestAsync(string fileName)
    {
        ImportUtilities.AbortIfFileDoesntExist(fileName);

        var lines = await ReadFirstTwoLinesAsync(fileName);
        if (lines is null || lines.Length != 2 || lines[0].IsNothing() || lines[1].IsNothing())
        {
            return false;
        }

        try
        {
            if (!VerifyColumnHeaderLine(lines[0]))
            {
                return false;
            }

            if (!VerifyFirstDataLine(lines[1].Split(',')))
            {
                return false;
            }
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    /// <summary>Shared lookup/cache pattern used by the importers that key transaction types off a single column.</summary>
    protected static TransactionType FetchTransactionType(BankImportUtilities importUtilities, Dictionary<string, TransactionType> cache, string[] array, int typeIndex, decimal amount)
    {
        var stringType = importUtilities.FetchString(array, typeIndex);
        if (stringType.IsNothing())
        {
            return NamedTransaction.Empty;
        }

        if (cache.TryGetValue(stringType, out var type))
        {
            return type;
        }

        var transactionType = new NamedTransaction(stringType, amount < 0);
        cache.Add(stringType, transactionType);
        return transactionType;
    }

    /// <summary>Parses one already-split data line into a <see cref="Transaction" />.</summary>
    protected abstract Transaction ParseLine(string[] split, Account account);

    protected async Task<string[]?> ReadFirstTwoLinesAsync(string fileName)
    {
        var chunk = await ReadTextChunkAsync(fileName);
        return chunk.IsNothing() ? null : chunk.SplitLines(2);
    }

    /// <summary>
    ///     Reads the lines from the file asynchronously.
    /// </summary>
    protected virtual async Task<IEnumerable<string>> ReadLinesAsync(string fileName)
    {
        var reader = ReaderWriterSelector.SelectReaderWriter(false);
        var allText = await reader.LoadFromDiskAsync(fileName);
        return allText.SplitLines();
    }

    /// <summary>
    ///     Reads a chunk of text asynchronously.
    /// </summary>
    protected virtual async Task<string> ReadTextChunkAsync(string filePath)
    {
        var reader = ReaderWriterSelector.SelectReaderWriter(false);
        return await reader.LoadFirstLinesFromDiskAsync(filePath, 2);
    }

    protected virtual bool VerifyColumnHeaderLine(string line)
    {
        var compareTo = line.EndsWith("\r", StringComparison.OrdinalIgnoreCase) ? line.Remove(line.Length - 1, 1) : line;
        return string.CompareOrdinal(compareTo, ExpectedHeaderLine) == 0;
    }

    /// <summary>Verifies the first data line (already split) looks like this bank's format.</summary>
    protected abstract bool VerifyFirstDataLine(string[] split);
}
