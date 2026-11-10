using System.Globalization;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.BankFeed;
using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.Transactions;
using Shouldly;

namespace BudgetAnalyser.Wpf.XUnit3.Transactions;

public class BankFeedImportSummaryTest
{
    private static readonly ChequeAccount Cheque = new(AccountTypeRepositoryConstants.Cheque);
    private static readonly ImportDateRange Range = new(new DateOnly(2026, 11, 13), new DateOnly(2026, 11, 19));
    private static readonly VisaAccount Visa = new(AccountTypeRepositoryConstants.Visa);

    [Fact]
    public void Describe_ShouldListCountsPerAccountAndDuplicates()
    {
        var result = new BankFeedImportResult(Range, [new BankFeedAccountImportResult(Cheque, 12, 12, 0), new BankFeedAccountImportResult(Visa, 5, 5, 0)], 2);

        var text = WithCulture(() => BankFeedImportSummary.Describe(result));

        text.ShouldBe("Imported 17 transactions (13 Nov – 19 Nov): CHEQUE 12, VISA 5. 2 look like duplicates and have been flagged for review.");
    }

    [Fact]
    public void Describe_ShouldSayNothingNew_GivenEverythingAlreadyImported()
    {
        var result = new BankFeedImportResult(Range, [new BankFeedAccountImportResult(Cheque, 3, 0, 3)], 0);

        BankFeedImportSummary.Describe(result).ShouldBe("No new transactions in that date range. 3 had already been imported and were skipped.");
    }

    [Fact]
    public void Describe_ShouldSayNothingNew_GivenEmptyDownload()
    {
        var result = new BankFeedImportResult(Range, [new BankFeedAccountImportResult(Cheque, 0, 0, 0)], 0);

        BankFeedImportSummary.Describe(result).ShouldBe("No new transactions in that date range.");
    }

    [Fact]
    public void Describe_ShouldUseSingular_GivenOneTransaction()
    {
        var result = new BankFeedImportResult(Range, [new BankFeedAccountImportResult(Cheque, 1, 1, 0), new BankFeedAccountImportResult(Visa, 0, 0, 0)], 0);

        var text = WithCulture(() => BankFeedImportSummary.Describe(result));

        text.ShouldBe("Imported 1 transaction (13 Nov – 19 Nov): CHEQUE 1.");
    }

    private static string WithCulture(Func<string> action)
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-NZ");
            return action();
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}
