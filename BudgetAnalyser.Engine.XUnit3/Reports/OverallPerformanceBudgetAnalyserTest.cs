using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.Engine.XUnit.Helpers;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Reports;

public class OverallPerformanceBudgetAnalyserTest
{
    private readonly OverallPerformanceBudgetAnalyser analyser;
    private readonly IBudgetBucketRepository bucketRepository = new BudgetBucketRepoAlwaysFind();
    private readonly TransactionsListModel transactionsTestData = TransactionsListModelTestData.TestData6();
    private BudgetCollection budgetsTestData = BudgetModelTestData.CreateCollectionWith2And5();
    private readonly ILogger logger;

    // Setup
    public OverallPerformanceBudgetAnalyserTest(ITestOutputHelper outputHelper)
    {
        this.logger = new XUnitLogger(outputHelper);
        this.analyser = new OverallPerformanceBudgetAnalyser(this.bucketRepository, this.logger);
        this.bucketRepository.GetByCode(TransactionsListModelTestData.IncomeBucket.Code);
        this.bucketRepository.GetByCode(TransactionsListModelTestData.HairBucket.Code);
        this.bucketRepository.GetByCode(TransactionsListModelTestData.PowerBucket.Code);
        this.bucketRepository.GetByCode(TransactionsListModelTestData.CarMtcBucket.Code);
        this.bucketRepository.GetByCode(TransactionsListModelTestData.PhoneBucket.Code);
    }

    [Fact]
    public void Analyse_ShouldErrorWhenMultipleBudgetsWithVariousPayCycles()
    {
        this.budgetsTestData[0].BudgetCycle = BudgetCycle.Monthly;
        this.budgetsTestData[1].BudgetCycle = BudgetCycle.Fortnightly;
        var result = this.analyser.Analyse(this.transactionsTestData, this.budgetsTestData, new DateOnly(2013, 1, 1), new DateOnly(2014, 03, 1));

        result.Error.ShouldBeTrue();
        result.HasValidationMessage.ShouldBeTrue();
    }

    [Fact]
    public void Analyse_ShouldFunctionWhenMultipleBudgetsWithSamePayCycles()
    {
        var result = this.analyser.Analyse(this.transactionsTestData, this.budgetsTestData, new DateOnly(2013, 1, 1), new DateOnly(2014, 03, 1));

        result.Error.ShouldBeFalse();
        result.HasValidationMessage.ShouldBeTrue();
    }

    [Fact]
    public void Analyse_ShouldRecogniseSingleBudget()
    {
        var result = this.analyser.Analyse(this.transactionsTestData, this.budgetsTestData, new DateOnly(2013, 1, 1), new DateOnly(2014, 01, 1));
        result.UsesMultipleBudgets.ShouldBeFalse();
    }

    [Fact]
    public void Analyse_ShouldRecogniseSingleBudget_Fortnight()
    {
        this.budgetsTestData[0].BudgetCycle = BudgetCycle.Fortnightly;
        this.budgetsTestData[1].BudgetCycle = BudgetCycle.Fortnightly;
        var result = this.analyser.Analyse(this.transactionsTestData, this.budgetsTestData, new DateOnly(2013, 1, 1), new DateOnly(2014, 01, 1));

        result.UsesMultipleBudgets.ShouldBeFalse();
    }

    [Fact]
    public void Analyse_ShouldReturnCorrectAverageSpend()
    {
        var result = this.analyser.Analyse(this.transactionsTestData, this.budgetsTestData, new DateOnly(2013, 1, 1), new DateOnly(2014, 01, 1));
        result.AverageSpend.ShouldBe(-1000);
    }

    [Fact]
    public void Analyse_ShouldReturnCorrectAverageSpend_Fortnight()
    {
        this.budgetsTestData = new BudgetCollection { BudgetModelTestData.CreateTestData5() };
        this.budgetsTestData.Single().BudgetCycle = BudgetCycle.Fortnightly;
        var result = this.analyser.Analyse(this.transactionsTestData, this.budgetsTestData, new DateOnly(2013, 1, 1), new DateOnly(2014, 01, 1));
        result.AverageSpend.ShouldBe(-461.54M, 0.01M);
    }

    [Fact]
    public void Analyse_ShouldReturnCorrectAverageSurplus()
    {
        var result = this.analyser.Analyse(this.transactionsTestData, this.budgetsTestData, new DateOnly(2013, 1, 1), new DateOnly(2014, 01, 1));
        result.AverageSurplus.ShouldBe(-500);
    }

    [Fact]
    public void Analyse_ShouldReturnCorrectAverageSurplus_Fortnight()
    {
        this.budgetsTestData = new BudgetCollection { BudgetModelTestData.CreateTestData5() };
        this.budgetsTestData.Single().BudgetCycle = BudgetCycle.Fortnightly;
        var result = this.analyser.Analyse(this.transactionsTestData, this.budgetsTestData, new DateOnly(2013, 1, 1), new DateOnly(2014, 01, 1));
        result.AverageSurplus.ShouldBe(-230.77M, 0.01M);
    }

    [Fact]
    public void Analyse_ShouldReturnCorrectNumberOfFortnights()
    {
        this.budgetsTestData = new BudgetCollection { BudgetModelTestData.CreateTestData5() };
        this.budgetsTestData.Single().BudgetCycle = BudgetCycle.Fortnightly;
        var result = this.analyser.Analyse(this.transactionsTestData, this.budgetsTestData, new DateOnly(2013, 1, 1), new DateOnly(2014, 01, 1));
        result.DurationInPeriods.ShouldBe(26);
    }

    [Fact]
    public void Analyse_ShouldReturnCorrectNumberOfMonths()
    {
        var result = this.analyser.Analyse(this.transactionsTestData, this.budgetsTestData, new DateOnly(2013, 1, 1), new DateOnly(2014, 01, 1));
        result.DurationInPeriods.ShouldBe(12);
    }

    [Fact]
    public void Analyse_ShouldReturnCorrectOverallPerformanceRating()
    {
        var result = this.analyser.Analyse(this.transactionsTestData, this.budgetsTestData, new DateOnly(2013, 1, 1), new DateOnly(2014, 01, 1));
        result.OverallPerformance.ShouldBe(-2760);
    }

    [Fact]
    public void Analyse_ShouldReturnCorrectOverallPerformanceRating_Fortnight()
    {
        this.budgetsTestData = new BudgetCollection { BudgetModelTestData.CreateTestData5() };
        this.budgetsTestData.Single().BudgetCycle = BudgetCycle.Fortnightly;
        var result = this.analyser.Analyse(this.transactionsTestData, this.budgetsTestData, new DateOnly(2013, 1, 1), new DateOnly(2014, 01, 1));
        result.OverallPerformance.ShouldBe(8020M, 0.01M);
    }

    [Fact]
    public void Analyse_ShouldReturnResultCorrectAvgForCarMtc()
    {
        var result = this.analyser.Analyse(this.transactionsTestData, this.budgetsTestData, new DateOnly(2013, 1, 1), new DateOnly(2014, 01, 1));
        result.Analyses.Single(b => b.Bucket.Code == TransactionsListModelTestData.CarMtcBucket.Code).AverageSpend.ShouldBe(200);
    }

    [Fact]
    public void Analyse_ShouldReturnResultCorrectAvgForCarMtc_Fortnight()
    {
        this.budgetsTestData = new BudgetCollection { BudgetModelTestData.CreateTestData5() };
        this.budgetsTestData.Single().BudgetCycle = BudgetCycle.Fortnightly;
        var result = this.analyser.Analyse(this.transactionsTestData, this.budgetsTestData, new DateOnly(2013, 1, 1), new DateOnly(2014, 01, 1));
        var avgCarMtc = result.Analyses.Single(b => b.Bucket.Code == TransactionsListModelTestData.CarMtcBucket.Code).AverageSpend;
        avgCarMtc.ShouldBe(92.31M, 0.01M);
    }

    [Fact]
    public void Analyse_ShouldReturnResultCorrectAvgForHair()
    {
        var result = this.analyser.Analyse(this.transactionsTestData, this.budgetsTestData, new DateOnly(2013, 1, 1), new DateOnly(2014, 01, 1));
        result.Analyses.Single(b => b.Bucket.Code == TransactionsListModelTestData.HairBucket.Code).AverageSpend.ShouldBe(300);
    }

    [Fact]
    public void Analyse_ShouldReturnResultCorrectAvgForHair_Fortnight()
    {
        this.budgetsTestData = new BudgetCollection { BudgetModelTestData.CreateTestData5() };
        this.budgetsTestData.Single().BudgetCycle = BudgetCycle.Fortnightly;
        var result = this.analyser.Analyse(this.transactionsTestData, this.budgetsTestData, new DateOnly(2013, 1, 1), new DateOnly(2014, 01, 1));
        var avgHair = result.Analyses.Single(b => b.Bucket.Code == TransactionsListModelTestData.HairBucket.Code).AverageSpend;
        avgHair.ShouldBe(138.46M, 0.01M);
    }

    [Fact]
    public void Analyse_ShouldReturnResultCorrectAvgForPhone()
    {
        var result = this.analyser.Analyse(this.transactionsTestData, this.budgetsTestData, new DateOnly(2013, 1, 1), new DateOnly(2014, 01, 1));
        result.Analyses.Single(b => b.Bucket.Code == TransactionsListModelTestData.PhoneBucket.Code).AverageSpend.ShouldBe(100);
    }

    [Fact]
    public void Analyse_ShouldReturnResultCorrectAvgForPhone_Fortnight()
    {
        this.budgetsTestData = new BudgetCollection { BudgetModelTestData.CreateTestData5() };
        this.budgetsTestData.Single().BudgetCycle = BudgetCycle.Fortnightly;
        var result = this.analyser.Analyse(this.transactionsTestData, this.budgetsTestData, new DateOnly(2013, 1, 1), new DateOnly(2014, 01, 1));
        var avgPhone = result.Analyses.Single(b => b.Bucket.Code == TransactionsListModelTestData.PhoneBucket.Code).AverageSpend;
        avgPhone.ShouldBe(46.15M, 0.01M);
    }

    [Fact]
    public void Analyse_ShouldReturnResultCorrectAvgForPower()
    {
        var result = this.analyser.Analyse(this.transactionsTestData, this.budgetsTestData, new DateOnly(2013, 1, 1), new DateOnly(2014, 01, 1));
        result.Analyses.Single(b => b.Bucket.Code == TransactionsListModelTestData.PowerBucket.Code).AverageSpend.ShouldBe(400);
    }

    [Fact]
    public void Analyse_ShouldReturnResultCorrectAvgForPower_Fortnight()
    {
        this.budgetsTestData = new BudgetCollection { BudgetModelTestData.CreateTestData5() };
        this.budgetsTestData.Single().BudgetCycle = BudgetCycle.Fortnightly;
        var result = this.analyser.Analyse(this.transactionsTestData, this.budgetsTestData, new DateOnly(2013, 1, 1), new DateOnly(2014, 01, 1));
        var avgPower = result.Analyses.Single(b => b.Bucket.Code == TransactionsListModelTestData.PowerBucket.Code).AverageSpend;
        avgPower.ShouldBe(184.62M, 0.01M);
    }

    [Fact]
    public void Analyse_ShouldReturnResultCorrectAvgForSurplus()
    {
        var result = this.analyser.Analyse(this.transactionsTestData, this.budgetsTestData, new DateOnly(2013, 1, 1), new DateOnly(2014, 01, 1));
        result.Analyses.Single(b => b.Bucket.Code == TransactionsListModelTestData.SurplusBucket.Code).AverageSpend.ShouldBe(500);
    }

    [Fact]
    public void Analyse_ShouldReturnResultCorrectAvgForSurplus_Fortnight()
    {
        this.budgetsTestData = new BudgetCollection { BudgetModelTestData.CreateTestData5() };
        this.budgetsTestData.Single().BudgetCycle = BudgetCycle.Fortnightly;
        var result = this.analyser.Analyse(this.transactionsTestData, this.budgetsTestData, new DateOnly(2013, 1, 1), new DateOnly(2014, 01, 1));
        var avgSurplus = result.Analyses.Single(b => b.Bucket.Code == TransactionsListModelTestData.SurplusBucket.Code).AverageSpend;
        avgSurplus.ShouldBe(230.77M, 0.01M);
    }

    [Fact]
    public void Analyse_ShouldReturnResultWithSingleBudget()
    {
        var result = this.analyser.Analyse(this.transactionsTestData, this.budgetsTestData, new DateOnly(2013, 1, 1), new DateOnly(2014, 01, 1));
        result.Analyses.Count().ShouldBe(6);
    }

    [Fact]
    public void Analyse_ShouldThrowArgumentNullException_WhenBudgetsIsNull()
    {
        Should.Throw<ArgumentNullException>(() => this.analyser.Analyse(this.transactionsTestData, null!, new DateOnly(2013, 1, 1), new DateOnly(2014, 01, 1)));
    }

    [Fact]
    public void Analyse_ShouldThrowArgumentNullException_WhenTransactionsModelIsNull()
    {
        Should.Throw<NullReferenceException>(() => this.analyser.Analyse(null!, this.budgetsTestData, new DateOnly(2013, 1, 1), new DateOnly(2014, 01, 1)));
    }

    [Fact]
    public void OutputTestData()
    {
        this.transactionsTestData.Output(new DateOnly(2013, 1, 1));
    }
}
