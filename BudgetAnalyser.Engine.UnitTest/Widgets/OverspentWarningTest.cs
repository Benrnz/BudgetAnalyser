using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using BudgetAnalyser.Engine.Widgets;
using Moq;
using Range = Moq.Range;

namespace BudgetAnalyser.Engine.UnitTest.Widgets;

[TestClass]
public class OverspentWarningTest
{
    private IBudgetCurrencyContext BudgetCurrencyContext { get; set; }
    private GlobalFilterCriteria Filter => new() { BeginDate = new DateOnly(2013, 08, 15), EndDate = new DateOnly(2013, 09, 14) };

    /// <summary>
    ///     Used in response to <see cref="LedgerCalculation.CalculateCurrentPeriodLedgerBalances" />
    /// </summary>
    private IDictionary<BudgetBucket, decimal> LedgerBalancesFake { get; set; }

    private LedgerBook LedgerBook { get; set; }
    private LedgerCalculation LedgerCalculator { get; set; }
    private StatementModel Statement { get; set; }
    private OverspentWarning Subject { get; set; }

    [TestInitialize]
    public void TestInitialise()
    {
        Statement = StatementModelTestData.TestData2A();

        // Mocking out the Calculator means we don't need the LedgerBook
        LedgerBook = new LedgerBookTestHarness { StorageKey = "Test Ledger Book.xaml" };
        SetLedgerBalancesFakeDataSomeOverspentBuckets();

        Subject = new OverspentWarning(new FakeLogger())
        {
            Tolerance = 10,
        };
        Act();
    }

    [TestMethod]
    public void UpdateShouldAdd2ElementsToOverSpentSummaryGivenOverSpentTestData()
    {
        Assert.AreEqual(2, Subject.OverSpentSummary.Count());
    }

    [TestMethod]
    public void UpdateShouldAddHairElementToOverSpentSummaryGivenOverSpentTestData()
    {
        Assert.IsTrue(Subject.OverSpentSummary.Any(s => s.Key == StatementModelTestData.HairBucket));
    }

    [TestMethod]
    public void UpdateShouldAddPhoneElementToOverSpentSummaryGivenOverSpentTestData()
    {
        Assert.IsTrue(Subject.OverSpentSummary.Any(s => s.Key == StatementModelTestData.PhoneBucket));
    }

    [TestMethod]
    public void UpdateShouldSetEnabledToFalseGivenStatementIsNull()
    {
        Statement = null;
        Act();
        Assert.IsFalse(Subject.Enabled);
    }

    [TestMethod]
    public void UpdateShouldSetEnabledToTrueGivenAllDependenciesAreValid()
    {
        Assert.IsTrue(Subject.Enabled);
    }

    [TestMethod]
    public void UpdateShouldSetLargeNumberTo0GivenUnderSpentTestData()
    {
        SetLedgerBalancesFakeDataNoOverSpentBuckets();
        Act();
        Assert.AreEqual("0", Subject.LargeNumber);
    }

    [TestMethod]
    public void UpdateShouldSetLargeNumberTo2GivenOverSpentTestData()
    {
        Assert.AreEqual("2", Subject.LargeNumber);
    }

    [TestMethod]
    public void UpdateShouldSetWarningStyleGivenOverSpentTestData()
    {
        Assert.AreEqual("WidgetWarningStyle", Subject.ColourStyleName);
    }

    [TestMethod]
    public void UpdateShouldSetWarningStyleGivenUnderSpentTestData()
    {
        SetLedgerBalancesFakeDataNoOverSpentBuckets();
        Act();
        Assert.AreEqual("WidgetStandardStyle", Subject.ColourStyleName);
    }

    private void Act()
    {
        var expenseListFake = new List<Expense>
        {
            new() { Bucket = StatementModelTestData.CarMtcBucket, Amount = 90 }, // Overspent by $1
            new() { Bucket = StatementModelTestData.HairBucket, Amount = 55 },
            new() { Bucket = StatementModelTestData.PhoneBucket, Amount = 65 }, // Overpsent 3.29
            new() { Bucket = StatementModelTestData.PowerBucket, Amount = 100 },
            new() { Bucket = StatementModelTestData.RegoBucket, Amount = 20 }
        };
        var modelMock = new Mock<BudgetModel>();
        modelMock.Setup(m => m.Expenses).Returns(expenseListFake);
        var budgetCurrencyContextMock = new Mock<IBudgetCurrencyContext>();
        budgetCurrencyContextMock.Setup(m => m.Model).Returns(modelMock.Object);
        BudgetCurrencyContext = budgetCurrencyContextMock.Object;

        // Mocking out the Calculator means we dont need the LedgerBook
        var ledgerCalculatorMock = new Mock<LedgerCalculation>();
        ledgerCalculatorMock.Setup(m => m.CalculateCurrentPeriodLedgerBalances(
            It.IsAny<LedgerEntryLine>(),
            It.IsAny<GlobalFilterCriteria>(),
            It.IsAny<StatementModel>())).Returns(LedgerBalancesFake);

        // Create a stubbed LedgerEntryLine to satisfy LocateApplicableLedgerLine.  This stub is passed into the mock calculator above.
        var ledgerReconLine = new LedgerEntryLine();
        ledgerCalculatorMock.Setup(m => m.LocateApplicableLedgerLine(
                It.IsAny<LedgerBook>(),
                It.IsInRange(DateOnly.MinValue, DateOnly.MaxValue, Range.Inclusive),
                It.IsInRange(DateOnly.MinValue, DateOnly.MaxValue, Range.Inclusive)))
            .Returns(ledgerReconLine);
        LedgerCalculator = ledgerCalculatorMock.Object;


        Subject.Update(Statement, BudgetCurrencyContext, Filter, LedgerBook, LedgerCalculator);
    }

    private void SetLedgerBalancesFakeDataNoOverSpentBuckets()
    {
        LedgerBalancesFake = new Dictionary<BudgetBucket, decimal>
        {
            { StatementModelTestData.CarMtcBucket, 10 },
            { StatementModelTestData.HairBucket, -10.00M },
            { StatementModelTestData.PowerBucket, 0 },
            { StatementModelTestData.RegoBucket, -3 },
            { StatementModelTestData.PhoneBucket, 10 }
        };
    }

    private void SetLedgerBalancesFakeDataSomeOverspentBuckets()
    {
        // Phone is intentionally missing from here, so testing fall back to budget can occur.
        // These are balances remaining after looking up the ledger balance and subtracting all spending transactions.
        // LedgerBalancesFake is used in response to mock the response from <see cref="LedgerCalculation.CalculateCurrentPeriodLedgerBalances"/>.

        LedgerBalancesFake = new Dictionary<BudgetBucket, decimal>
        {
            { StatementModelTestData.CarMtcBucket, 10 }, { StatementModelTestData.HairBucket, -10.01M }, { StatementModelTestData.PowerBucket, 0 }, { StatementModelTestData.RegoBucket, -3 }
        };
    }
}
