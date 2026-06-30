using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using NSubstitute;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Widgets;

public class OverspentWarningTest
{
    private readonly GlobalFilterCriteria filter = new() { BeginDate = new DateOnly(2013, 08, 15), EndDate = new DateOnly(2013, 09, 14) };
    private readonly LedgerBook ledgerBook;
    private readonly OverspentWarning subject;
    private IBudgetCurrencyContext budgetCurrencyContext = null!;
    private IDictionary<BudgetBucket, decimal> ledgerBalancesFake = null!;
    private LedgerCalculation ledgerCalculator = null!;
    private TransactionsListModel? transactionsList;

    public OverspentWarningTest()
    {
        this.transactionsList = TransactionsListModelTestData.TestData2A();
        this.ledgerBook = new LedgerBookTestHarness { StorageKey = "Test Ledger Book.xaml" };
        SetLedgerBalancesFakeDataSomeOverspentBuckets();
        this.subject = new OverspentWarning(new FakeLogger()) { Tolerance = 10 };
        Act();
    }

    [Fact]
    public void UpdateShouldAdd2ElementsToOverSpentSummaryGivenOverSpentTestData()
    {
        this.subject.OverSpentSummary.Count().ShouldBe(2);
    }

    [Fact]
    public void UpdateShouldAddHairElementToOverSpentSummaryGivenOverSpentTestData()
    {
        this.subject.OverSpentSummary.Any(s => s.Key == TransactionsListModelTestData.HairBucket).ShouldBeTrue();
    }

    [Fact]
    public void UpdateShouldAddPhoneElementToOverSpentSummaryGivenOverSpentTestData()
    {
        this.subject.OverSpentSummary.Any(s => s.Key == TransactionsListModelTestData.PhoneBucket).ShouldBeTrue();
    }

    [Fact]
    public void UpdateShouldSetEnabledToFalseGivenTransactionsListIsNull()
    {
        this.transactionsList = null;
        Act();
        this.subject.Enabled.ShouldBeFalse();
    }

    [Fact]
    public void UpdateShouldSetEnabledToTrueGivenAllDependenciesAreValid()
    {
        this.subject.Enabled.ShouldBeTrue();
    }

    [Fact]
    public void UpdateShouldSetLargeNumberTo0GivenUnderSpentTestData()
    {
        SetLedgerBalancesFakeDataNoOverSpentBuckets();
        Act();
        this.subject.LargeNumber.ShouldBe("0");
    }

    [Fact]
    public void UpdateShouldSetLargeNumberTo2GivenOverSpentTestData()
    {
        this.subject.LargeNumber.ShouldBe("2");
    }

    [Fact]
    public void UpdateShouldSetWarningStyleGivenOverSpentTestData()
    {
        this.subject.ColourStyleName.ShouldBe("WidgetWarningStyle");
    }

    [Fact]
    public void UpdateShouldSetWarningStyleGivenUnderSpentTestData()
    {
        SetLedgerBalancesFakeDataNoOverSpentBuckets();
        Act();
        this.subject.ColourStyleName.ShouldBe("WidgetStandardStyle");
    }

    private void Act()
    {
        var expenseListFake = new List<Expense>
        {
            new() { Bucket = TransactionsListModelTestData.CarMtcBucket, Amount = 90 },
            new() { Bucket = TransactionsListModelTestData.HairBucket, Amount = 55 },
            new() { Bucket = TransactionsListModelTestData.PhoneBucket, Amount = 65 },
            new() { Bucket = TransactionsListModelTestData.PowerBucket, Amount = 100 },
            new() { Bucket = TransactionsListModelTestData.RegoBucket, Amount = 20 }
        };

        var modelMock = Substitute.For<BudgetModel>();
        modelMock.Expenses.Returns(expenseListFake);
        this.budgetCurrencyContext = Substitute.For<IBudgetCurrencyContext>();
        this.budgetCurrencyContext.Model.Returns(modelMock);

        var ledgerCalculatorMock = Substitute.For<LedgerCalculation>();
        ledgerCalculatorMock.CalculateCurrentPeriodLedgerBalances(
                Arg.Any<LedgerEntryLine>(),
                Arg.Any<GlobalFilterCriteria>(),
                Arg.Any<TransactionsListModel>())
            .Returns(this.ledgerBalancesFake);

        var ledgerReconLine = new LedgerEntryLine();
        ledgerCalculatorMock.LocateApplicableLedgerLine(
                Arg.Any<LedgerBook>(),
                Arg.Any<DateOnly?>(),
                Arg.Any<DateOnly?>())
            .Returns(ledgerReconLine);
        this.ledgerCalculator = ledgerCalculatorMock;

        this.subject.Update(this.transactionsList!, this.budgetCurrencyContext, this.filter, this.ledgerBook, this.ledgerCalculator);
    }

    private void SetLedgerBalancesFakeDataNoOverSpentBuckets()
    {
        this.ledgerBalancesFake = new Dictionary<BudgetBucket, decimal>
        {
            { TransactionsListModelTestData.CarMtcBucket, 10 },
            { TransactionsListModelTestData.HairBucket, -10.00M },
            { TransactionsListModelTestData.PowerBucket, 0 },
            { TransactionsListModelTestData.RegoBucket, -3 },
            { TransactionsListModelTestData.PhoneBucket, 10 }
        };
    }

    private void SetLedgerBalancesFakeDataSomeOverspentBuckets()
    {
        this.ledgerBalancesFake = new Dictionary<BudgetBucket, decimal>
        {
            { TransactionsListModelTestData.CarMtcBucket, 10 },
            { TransactionsListModelTestData.HairBucket, -10.01M },
            { TransactionsListModelTestData.PowerBucket, 0 },
            { TransactionsListModelTestData.RegoBucket, -3 }
        };
    }
}
