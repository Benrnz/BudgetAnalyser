using System.Globalization;
using BudgetAnalyser.Engine.BankAccount;

namespace BudgetAnalyser.Engine.Statement;

/// <summary>
///     An importer for ANZ Visa bank statement export.
/// </summary>
[AutoRegisterWithIoC(SingleInstance = true)]
internal class AnzVisaStatementImporterV1 : IBankStatementImporter
{
    private const int Reference1Index = 0;
    private const int TransactionTypeIndex = 1;
    private const int AmountIndex = 2;
    private const int DescriptionIndex = 3;
    private const int DateIndex = 4;
    private const string DebitTransactionType = "D";
    private const string CreditTransactionType = "C";

    private static readonly Dictionary<string, NamedTransaction> TransactionTypes = new()
    {
        { CreditTransactionType, new NamedTransaction("Credit Card Credit") }, { DebitTransactionType, new NamedTransaction("Credit Card Debit", true) }
    };

    private readonly BankImportUtilities importUtilities;
    private readonly ILogger logger;
    private readonly IReaderWriterSelector readerWriterSelector;

    public AnzVisaStatementImporterV1(BankImportUtilities importUtilities, ILogger logger, IReaderWriterSelector readerWriterSelector)
    {
        this.importUtilities = importUtilities ?? throw new ArgumentNullException(nameof(importUtilities));
        this.importUtilities.ConfigureLocale(new CultureInfo("en-NZ"));
        // ANZ importers are NZ specific at this stage.
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.readerWriterSelector = readerWriterSelector ?? throw new ArgumentNullException(nameof(readerWriterSelector));
    }

    /// <summary>
    ///     Load the given file into a <see cref="TransactionSetModel" />.
    /// </summary>
    /// <param name="fileName">The file to load.</param>
    /// <param name="account">
    ///     The account to classify these transactions. This is useful when merging one statement to another. For example,
    ///     merging a cheque account
    ///     export with visa account export, each can be classified using an account.
    /// </param>
    public async Task<TransactionSetModel> LoadAsync(string fileName, Account account)
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
            var transactionType = FetchTransactionType(split);
            var transaction = new Transaction
            {
                Account = account,
                Reference1 = this.importUtilities.FetchString(split, Reference1Index),
                TransactionType = transactionType,
                Description = this.importUtilities.FetchString(split, DescriptionIndex),
                Date = this.importUtilities.FetchDate(split, DateIndex),
                Amount = FetchAmount(split, transactionType)
            };
            transactions.Add(transaction);
        }

        return new TransactionSetModel(this.logger) { StorageKey = fileName, LastImport = DateTime.Now }.LoadTransactions(transactions);
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

    private decimal FetchAmount(string[] array, NamedTransaction transaction)
    {
        try
        {
            if (array[AmountIndex].IsNothing())
            {
                return 0;
            }

            var amount = this.importUtilities.FetchDecimal(array, AmountIndex);
            if (transaction.IsDebit)
            {
                amount *= -1;
            }

            return amount;
        }
        catch (InvalidDataException ex)
        {
            this.logger.LogError(ex, l => l.Format("Unable to convert provided string to a decimal. Probable format change in bank file."));
            throw;
        }
    }

    private NamedTransaction FetchTransactionType(string[] array)
    {
        var stringType = this.importUtilities.FetchString(array, TransactionTypeIndex);
        if (string.IsNullOrWhiteSpace(stringType))
        {
            return NamedTransaction.Empty;
        }

        if (TransactionTypes.TryGetValue(stringType, out var cachedTransactionType))
        {
            return cachedTransactionType;
        }

        var fullTypeText = stringType;
        var transactionType = new NamedTransaction(fullTypeText, true);
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
        return string.CompareOrdinal(compareTo, "Card,Type,Amount,Details,TransactionDate,ProcessedDate,ForeignCurrencyAmount,ConversionCharge") == 0;
    }

    private bool VerifyFirstDataLine(string line)
    {
        var split = line.Split(',');
        var card = this.importUtilities.FetchString(split, Reference1Index);
        if (card.IsSomething())
        {
            if (!char.IsDigit(card.ToCharArray()[0]))
            {
                return false;
            }
        }

        var amount = this.importUtilities.FetchDecimal(split, AmountIndex);
        if (amount == 0)
        {
            return false;
        }

        var date = this.importUtilities.FetchDate(split, DateIndex);
        return date != DateOnly.MinValue;
    }
}
