using BudgetAnalyser.Engine.BankAccount;

namespace BudgetAnalyser.Engine.Transactions;

/// <summary>
///     An importer for ANZ Visa bank extracts.
/// </summary>
[AutoRegisterWithIoC(SingleInstance = true)]
internal class AnzVisaExtractImporterV1 : CsvBankExtractImporterBase
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

    public AnzVisaExtractImporterV1(BankImportUtilities importUtilities, ILogger logger, IReaderWriterSelector readerWriterSelector)
        : base(importUtilities, logger, readerWriterSelector)
    {
    }

    protected override string ExpectedHeaderLine => "Card,Type,Amount,Details,TransactionDate,ProcessedDate,ForeignCurrencyAmount,ConversionCharge";

    protected override Transaction ParseLine(string[] split, Account account)
    {
        var transactionType = FetchTransactionType(split);
        return new Transaction
        {
            Account = account,
            Reference1 = ImportUtilities.FetchString(split, Reference1Index),
            TransactionType = transactionType,
            Description = ImportUtilities.FetchString(split, DescriptionIndex),
            Date = ImportUtilities.FetchDate(split, DateIndex),
            Amount = FetchAmount(split, transactionType)
        };
    }

    protected override bool VerifyFirstDataLine(string[] split)
    {
        var card = ImportUtilities.FetchString(split, Reference1Index);
        if (card.IsSomething())
        {
            if (!char.IsDigit(card.ToCharArray()[0]))
            {
                return false;
            }
        }

        var amount = ImportUtilities.FetchDecimal(split, AmountIndex);
        if (amount == 0)
        {
            return false;
        }

        var date = ImportUtilities.FetchDate(split, DateIndex);
        return date != DateOnly.MinValue;
    }

    private decimal FetchAmount(string[] array, NamedTransaction transaction)
    {
        try
        {
            if (array[AmountIndex].IsNothing())
            {
                return 0;
            }

            var amount = ImportUtilities.FetchDecimal(array, AmountIndex);
            if (transaction.IsDebit)
            {
                amount *= -1;
            }

            return amount;
        }
        catch (InvalidDataException ex)
        {
            Logger.LogError(ex, l => l.Format("Unable to convert provided string to a decimal. Probable format change in bank file."));
            throw;
        }
    }

    private NamedTransaction FetchTransactionType(string[] array)
    {
        var stringType = ImportUtilities.FetchString(array, TransactionTypeIndex);
        if (string.IsNullOrWhiteSpace(stringType))
        {
            return NamedTransaction.Empty;
        }

        if (TransactionTypes.TryGetValue(stringType, out var cachedTransactionType))
        {
            return cachedTransactionType;
        }

        var transactionType = new NamedTransaction(stringType, true);
        TransactionTypes.Add(stringType, transactionType);
        return transactionType;
    }
}
