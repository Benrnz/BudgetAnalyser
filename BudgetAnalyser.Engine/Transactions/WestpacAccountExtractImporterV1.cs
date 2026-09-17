using BudgetAnalyser.Engine.BankAccount;

namespace BudgetAnalyser.Engine.Transactions;

/// <summary>
///     An Importer for Westpac Cheque and Savings Accounts bank extracts.
/// </summary>
[AutoRegisterWithIoC(SingleInstance = true)]
internal class WestpacAccountExtractImporterV1 : CsvBankExtractImporterBase
{
    private const int TransactionTypeIndex = 3;
    private const int DescriptionIndex = 2;
    private const int Reference1Index = 4;
    private const int Reference2Index = 5;
    private const int Reference3Index = 6;
    private const int AmountIndex = 1;
    private const int DateIndex = 0;
    private const int ExpectedColumnCount = 7;

    private static readonly Dictionary<string, TransactionType> TransactionTypes = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="WestpacAccountExtractImporterV1" /> class.
    /// </summary>
    /// <exception cref="System.ArgumentNullException">
    /// </exception>
    public WestpacAccountExtractImporterV1(BankImportUtilities importUtilities, ILogger logger, IReaderWriterSelector readerWriterSelector)
        : base(importUtilities, logger, readerWriterSelector)
    {
    }

    protected override string ExpectedHeaderLine => "Date,Amount,Other Party,Description,Reference,Particulars,Analysis Code";

    protected override Transaction ParseLine(string[] split, Account account)
    {
        var transaction = new Transaction
        {
            Account = account,
            Description = ImportUtilities.FetchString(split, DescriptionIndex),
            Reference1 = ImportUtilities.FetchString(split, Reference1Index),
            Reference2 = ImportUtilities.FetchString(split, Reference2Index),
            Reference3 = ImportUtilities.FetchString(split, Reference3Index),
            Amount = ImportUtilities.FetchDecimal(split, AmountIndex),
            Date = ImportUtilities.FetchDate(split, DateIndex)
        };
        transaction.TransactionType = FetchTransactionType(ImportUtilities, TransactionTypes, split, TransactionTypeIndex, transaction.Amount);
        return transaction;
    }

    protected override bool VerifyFirstDataLine(string[] split)
    {
        var type = ImportUtilities.FetchString(split, TransactionTypeIndex);
        if (string.IsNullOrWhiteSpace(type))
        {
            return false;
        }

        if (char.IsDigit(type.ToCharArray()[0]))
        {
            return false;
        }

        var amount = ImportUtilities.FetchDecimal(split, AmountIndex);
        if (amount == 0)
        {
            return false;
        }

        var date = ImportUtilities.FetchDate(split, DateIndex);
        if (date == DateOnly.MinValue)
        {
            return false;
        }

        return split.Length == ExpectedColumnCount;
    }
}
