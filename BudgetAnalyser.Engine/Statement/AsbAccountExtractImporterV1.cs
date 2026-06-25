using System.Globalization;
using BudgetAnalyser.Engine.BankAccount;

namespace BudgetAnalyser.Engine.Statement;

/// <summary>
///     An Importer for ASB Everyday Accounts CSV files.
/// </summary>
[AutoRegisterWithIoC(SingleInstance = true)]
internal class AsbAccountExtractImporterV1 : IBankExtractImporter
{
    private const int DateIndex = 0;
    private const int UniqueIdIndex = 1;
    private const int TransactionTypeIndex = 2;
    private const int ChequeNumberIndex = 3;
    private const int PayeeIndex = 4;
    private const int MemoIndex = 5;
    private const int AmountIndex = 6;
    private const int MetadataLineCount = 7;

    private static readonly Dictionary<string, TransactionType> TransactionTypes = new();

    private readonly BankImportUtilities importUtilities;
    private readonly ILogger logger;
    private readonly IReaderWriterSelector readerWriterSelector;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AsbAccountExtractImporterV1" /> class.
    /// </summary>
    /// <exception cref="System.ArgumentNullException">
    /// </exception>
    public AsbAccountExtractImporterV1(BankImportUtilities importUtilities, ILogger logger, IReaderWriterSelector readerWriterSelector)
    {
        this.importUtilities = importUtilities ?? throw new ArgumentNullException(nameof(importUtilities));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.readerWriterSelector = readerWriterSelector ?? throw new ArgumentNullException(nameof(readerWriterSelector));
        this.importUtilities.ConfigureLocale(new CultureInfo("en-NZ"));
        // ASB importers are NZ specific.
    }

    /// <summary>
    ///     Load the given file into a <see cref="TransactionsListModel" />.
    /// </summary>
    /// <param name="fileName">The file to load.</param>
    /// <param name="account">
    ///     The account to classify these transactions. This is useful when merging one statement to another. For example,
    ///     merging a cheque account export with visa account export, each can be classified using an account.
    /// </param>
    public async Task<TransactionsListModel> LoadAsync(string fileName, Account account)
    {
        try
        {
            this.importUtilities.AbortIfFileDoesntExist(fileName);
        }
        catch (FileNotFoundException ex)
        {
            throw new KeyNotFoundException(ex.Message, ex);
        }

        var transactions = new List<Transaction>();
        var lineNumber = 0;
        foreach (var line in await ReadLinesAsync(fileName))
        {
            lineNumber++;

            // Skip metadata lines (lines 1-7)
            if (lineNumber <= MetadataLineCount)
            {
                continue;
            }

            // Skip header line (line 8)
            if (lineNumber == MetadataLineCount + 1)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var split = line.Split(',');
            var transaction = new Transaction
            {
                Account = account,
                Description = this.importUtilities.FetchString(split, MemoIndex),
                Reference1 = this.importUtilities.FetchString(split, PayeeIndex),
                Reference2 = this.importUtilities.FetchString(split, ChequeNumberIndex),
                Reference3 = this.importUtilities.FetchString(split, UniqueIdIndex),
                Amount = this.importUtilities.FetchDecimal(split, AmountIndex),
                Date = this.importUtilities.FetchDate(split, DateIndex)
            };
            transaction.TransactionType = FetchTransactionType(split, transaction.Amount);
            transactions.Add(transaction);
        }

        var statement = new TransactionsListModel(this.logger) { StorageKey = fileName, LastImport = DateTime.Now }.LoadTransactions(transactions);

        return statement;
    }

    /// <summary>
    ///     Test the given file to see if this importer implementation can read and import it.
    ///     This will open and read some of the contents of the file.
    /// </summary>
    public async Task<bool> TasteTestAsync(string fileName)
    {
        this.importUtilities.AbortIfFileDoesntExist(fileName);

        var lines = await ReadFirstLinesAsync(fileName);
        if (lines is null || lines.Length < 8 || lines[7].IsNothing())
        {
            return false;
        }

        try
        {
            if (!VerifyMetadataLine(lines[1]))
            {
                return false;
            }

            if (!VerifyColumnHeaderLine(lines[6]))
            {
                return false;
            }

            if (!VerifyFirstDataLine(lines[7]))
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

    /// <summary>
    ///     Reads the lines from the file asynchronously.
    /// </summary>
    protected virtual async Task<IEnumerable<string>> ReadLinesAsync(string fileName)
    {
        var reader = this.readerWriterSelector.SelectReaderWriter(false);
        var allText = await reader.LoadFromDiskAsync(fileName);
        return allText.SplitLines();
    }

    /// <summary>
    ///     Reads a chunk of text asynchronously.
    /// </summary>
    protected virtual async Task<string> ReadTextChunkAsync(string filePath)
    {
        var reader = this.readerWriterSelector.SelectReaderWriter(false);
        return await reader.LoadFirstLinesFromDiskAsync(filePath, 9);
    }

    private TransactionType FetchTransactionType(string[] array, decimal amount)
    {
        var stringType = this.importUtilities.FetchString(array, TransactionTypeIndex);
        if (stringType.IsNothing())
        {
            return NamedTransaction.Empty;
        }

        if (TransactionTypes.TryGetValue(stringType, out var cachedType))
        {
            return cachedType;
        }

        var transactionType = new NamedTransaction(stringType, amount < 0);
        TransactionTypes.Add(stringType, transactionType);
        return transactionType;
    }

    private async Task<string[]?> ReadFirstLinesAsync(string fileName)
    {
        var chunk = await ReadTextChunkAsync(fileName);
        return chunk.IsNothing() ? null : chunk.SplitLines(9);
    }

    private bool VerifyMetadataLine(string line)
    {
        var compareTo = line.EndsWith("\r", StringComparison.OrdinalIgnoreCase) ? line.Remove(line.Length - 1, 1) : line;
        return compareTo.Contains("Bank") && compareTo.Contains("Account");
    }

    private static bool VerifyColumnHeaderLine(string line)
    {
        var compareTo = line.EndsWith("\r", StringComparison.OrdinalIgnoreCase) ? line.Remove(line.Length - 1, 1) : line;
        return string.CompareOrdinal(compareTo, "Date,Unique Id,Tran Type,Cheque Number,Payee,Memo,Amount") == 0;
    }

    private bool VerifyFirstDataLine(string line)
    {
        var split = line.Split(',');
        if (split.Length < 7)
        {
            return false;
        }

        try
        {
            var date = this.importUtilities.FetchDate(split, DateIndex);
            if (date == DateOnly.MinValue)
            {
                return false;
            }

            var amount = this.importUtilities.FetchDecimal(split, AmountIndex);
            if (amount == 0)
            {
                return false;
            }

            var payee = this.importUtilities.FetchString(split, PayeeIndex);
            if (string.IsNullOrWhiteSpace(payee))
            {
                return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}
