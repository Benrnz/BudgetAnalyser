using System.Globalization;
using BudgetAnalyser.Engine.BankAccount;

namespace BudgetAnalyser.Engine.Statement;

/// <summary>
///     An Importer for ANZ Cheque and Savings Accounts bank statement exports.
/// </summary>
[AutoRegisterWithIoC(SingleInstance = true)]
internal class AnzAccountStatementImporterV1 : IBankStatementImporter
{
    private const int TransactionTypeIndex = 0;
    private const int DescriptionIndex = 1;
    private const int Reference1Index = 2;
    private const int Reference2Index = 3;
    private const int Reference3Index = 4;
    private const int AmountIndex = 5;
    private const int DateIndex = 6;

    private static readonly Dictionary<string, TransactionType> TransactionTypes = new();

    private readonly BankImportUtilities importUtilities;
    private readonly ILogger logger;
    private readonly IReaderWriterSelector readerWriterSelector;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AnzAccountStatementImporterV1" /> class.
    /// </summary>
    /// <exception cref="System.ArgumentNullException">
    /// </exception>
    public AnzAccountStatementImporterV1(BankImportUtilities importUtilities, ILogger logger, IReaderWriterSelector readerWriterSelector)
    {
        this.importUtilities = importUtilities ?? throw new ArgumentNullException(nameof(importUtilities));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.readerWriterSelector = readerWriterSelector ?? throw new ArgumentNullException(nameof(readerWriterSelector));
        this.importUtilities.ConfigureLocale(new CultureInfo("en-NZ"));
        // ANZ importers are NZ specific at this stage.
    }

    /// <summary>
    ///     Load the given file into a <see cref="StatementModel" />.
    /// </summary>
    /// <param name="fileName">The file to load.</param>
    /// <param name="account">
    ///     The account to classify these transactions. This is useful when merging one statement to another. For example,
    ///     merging a cheque account export with visa account export, each can be classified using an account.
    /// </param>
    public async Task<StatementModel> LoadAsync(string fileName, Account account)
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

            var split = line.Split(',');
            var transaction = new Transaction
            {
                Account = account,
                Description = this.importUtilities.FetchString(split, DescriptionIndex),
                Reference1 = this.importUtilities.FetchString(split, Reference1Index),
                Reference2 = this.importUtilities.FetchString(split, Reference2Index),
                Reference3 = this.importUtilities.FetchString(split, Reference3Index),
                Amount = this.importUtilities.FetchDecimal(split, AmountIndex),
                Date = this.importUtilities.FetchDate(split, DateIndex)
            };
            transaction.TransactionType = FetchTransactionType(split, transaction.Amount);
            transactions.Add(transaction);
        }

        var statement = new StatementModel(this.logger) { StorageKey = fileName, LastImport = DateTime.Now }.LoadTransactions(transactions);

        return statement;
    }

    /// <summary>
    ///     Test the given file to see if this importer implementation can read and import it.
    ///     This will open and read some of the contents of the file.
    /// </summary>
    public async Task<bool> TasteTestAsync(string fileName)
    {
        this.importUtilities.AbortIfFileDoesntExist(fileName);

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

            if (!VerifyFirstDataLine(lines[1]))
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
        return await reader.LoadFirstLinesFromDiskAsync(filePath, 2);
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

    private async Task<string[]?> ReadFirstTwoLinesAsync(string fileName)
    {
        var chunk = await ReadTextChunkAsync(fileName);
        return chunk.IsNothing() ? null : chunk.SplitLines(2);
    }

    private static bool VerifyColumnHeaderLine(string line)
    {
        var compareTo = line.EndsWith("\r", StringComparison.OrdinalIgnoreCase) ? line.Remove(line.Length - 1, 1) : line;
        return string.CompareOrdinal(compareTo, "Type,Details,Particulars,Code,Reference,Amount,Date,ForeignCurrencyAmount,ConversionCharge") == 0;
    }

    private bool VerifyFirstDataLine(string line)
    {
        var split = line.Split(',');
        var type = this.importUtilities.FetchString(split, 0);
        if (string.IsNullOrWhiteSpace(type))
        {
            return false;
        }

        if (char.IsDigit(type.ToCharArray()[0]))
        {
            return false;
        }

        var amount = this.importUtilities.FetchDecimal(split, AmountIndex);
        if (amount == 0)
        {
            return false;
        }

        var date = this.importUtilities.FetchDate(split, DateIndex);
        if (date == DateTime.MinValue)
        {
            return false;
        }

        return split.Length == 9;
    }
}
