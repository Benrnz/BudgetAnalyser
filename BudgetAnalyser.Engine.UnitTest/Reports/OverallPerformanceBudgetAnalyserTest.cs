using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.UnitTest.Helper;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;

namespace BudgetAnalyser.Engine.UnitTest.Reports;

[TestClass]
public class OverallPerformanceBudgetAnalyserTest
{
    private readonly IBudgetBucketRepository bucketRepository = new BucketBucketRepoAlwaysFind();
    private readonly BudgetCollection budgetsTestData = BudgetModelTestData.CreateCollectionWith2And5();
    private readonly StatementModel statementTestData = StatementModelTestData.TestData6();
    private OverallPerformanceBudgetAnalyser? analyser;
    private GlobalFilterCriteria dateCriteria = new() { BeginDate = new DateTime(2013, 1, 1), EndDate = new DateTime(2014, 1, 1) };

    [TestMethod]
    public void Analyse_ShouldErrorWhenMultipleBudgetsWithVariousPayCycles()
    {
        // Arrange
        this.dateCriteria = new GlobalFilterCriteria { BeginDate = new DateTime(2013, 1, 1), EndDate = new DateTime(2014, 03, 1) };
        this.budgetsTestData[0].BudgetCycle = BudgetCycle.Monthly;
        this.budgetsTestData[1].BudgetCycle = BudgetCycle.Fortnightly;

        //Act
        var result = this.analyser!.Analyse(this.statementTestData, this.budgetsTestData, this.dateCriteria);

        Console.WriteLine(result.ValidationMessage);

        Assert.IsTrue(result.Error);
        Assert.IsTrue(result.HasValidationMessage);
    }

    [TestMethod]
    public void Analyse_ShouldFunctionWhenMultipleBudgetsWithSamePayCycles()
    {
        this.dateCriteria = new GlobalFilterCriteria { BeginDate = new DateTime(2013, 1, 1), EndDate = new DateTime(2014, 03, 1) };
        var result = this.analyser!.Analyse(this.statementTestData, this.budgetsTestData, this.dateCriteria);

        Console.WriteLine(result.ValidationMessage);

        Assert.IsFalse(result.Error);
        Assert.IsTrue(result.HasValidationMessage); // This will still have a warning.
    }

    [TestMethod]
    public void Analyse_ShouldRecogniseSingleBudget()
    {
        var result = this.analyser!.Analyse(this.statementTestData, this.budgetsTestData, this.dateCriteria);

        Assert.IsFalse(result.UsesMultipleBudgets);
    }

    [TestMethod]
    public void Analyse_ShouldReturnCorrectAverageSpend()
    {
        var result = this.analyser!.Analyse(this.statementTestData, this.budgetsTestData, this.dateCriteria);

        Assert.AreEqual(-1000, result.AverageSpend);
    }

    [TestMethod]
    public void Analyse_ShouldReturnCorrectAverageSurplus()
    {
        var result = this.analyser!.Analyse(this.statementTestData, this.budgetsTestData, this.dateCriteria);

        Assert.AreEqual(-500, result.AverageSurplus);
    }

    [TestMethod]
    public void Analyse_ShouldReturnCorrectNumberOfMonths()
    {
        var result = this.analyser!.Analyse(this.statementTestData, this.budgetsTestData, this.dateCriteria);

        Assert.AreEqual(12, result.DurationInPeriods);
    }

    [TestMethod]
    public void Analyse_ShouldReturnCorrectOverallPerformanceRating()
    {
        var result = this.analyser!.Analyse(this.statementTestData, this.budgetsTestData, this.dateCriteria);

        Assert.AreEqual(-2760, result.OverallPerformance);
    }

    [TestMethod]
    public void Analyse_ShouldReturnResultCorrectAvgForCarMtc()
    {
        var result = this.analyser!.Analyse(this.statementTestData, this.budgetsTestData, this.dateCriteria);

        Assert.AreEqual(200, result.Analyses.Single(b => b.Bucket.Code == StatementModelTestData.CarMtcBucket.Code).AverageSpend);
    }

    [TestMethod]
    public void Analyse_ShouldReturnResultCorrectAvgForHair()
    {
        var result = this.analyser!.Analyse(this.statementTestData, this.budgetsTestData, this.dateCriteria);

        Assert.AreEqual(300, result.Analyses.Single(b => b.Bucket.Code == StatementModelTestData.HairBucket.Code).AverageSpend);
    }

    [TestMethod]
    public void Analyse_ShouldReturnResultCorrectAvgForPhone()
    {
        var result = this.analyser!.Analyse(this.statementTestData, this.budgetsTestData, this.dateCriteria);

        Assert.AreEqual(100, result.Analyses.Single(b => b.Bucket.Code == StatementModelTestData.PhoneBucket.Code).AverageSpend);
    }

    [TestMethod]
    public void Analyse_ShouldReturnResultCorrectAvgForPower()
    {
        var result = this.analyser!.Analyse(this.statementTestData, this.budgetsTestData, this.dateCriteria);

        Assert.AreEqual(400, result.Analyses.Single(b => b.Bucket.Code == StatementModelTestData.PowerBucket.Code).AverageSpend);
    }

    [TestMethod]
    public void Analyse_ShouldReturnResultCorrectAvgForSurplus()
    {
        var result = this.analyser!.Analyse(this.statementTestData, this.budgetsTestData, this.dateCriteria);

        Assert.AreEqual(500, result.Analyses.Single(b => b.Bucket.Code == StatementModelTestData.SurplusBucket.Code).AverageSpend);
    }

    [TestMethod]
    public void Analyse_ShouldReturnResultWithSingleBudget()
    {
        var result = this.analyser!.Analyse(this.statementTestData, this.budgetsTestData, this.dateCriteria);

        Assert.AreEqual(6, result.Analyses.Count());
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Analyse_ShouldThrowArgumentNullException_WhenBudgetsIsNull()
    {
        this.analyser!.Analyse(this.statementTestData, null, this.dateCriteria);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Analyse_ShouldThrowArgumentNullException_WhenCriteriaIsNull()
    {
        this.analyser!.Analyse(this.statementTestData, this.budgetsTestData, null);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Analyse_ShouldThrowArgumentNullException_WhenStatementModelIsNull()
    {
        this.analyser!.Analyse(null, this.budgetsTestData, this.dateCriteria);
    }

    [TestMethod]
    public void OutputTestData()
    {
        this.statementTestData.Output(new DateTime(2013, 1, 1));
    }

    [TestInitialize]
    public void TestInitialize()
    {
        this.analyser = new OverallPerformanceBudgetAnalyser(this.bucketRepository);

        // Initialise the bucket repository with the test data
        this.bucketRepository.GetByCode(StatementModelTestData.IncomeBucket.Code);
        this.bucketRepository.GetByCode(StatementModelTestData.HairBucket.Code);
        this.bucketRepository.GetByCode(StatementModelTestData.PowerBucket.Code);
        this.bucketRepository.GetByCode(StatementModelTestData.CarMtcBucket.Code);
        this.bucketRepository.GetByCode(StatementModelTestData.PhoneBucket.Code);
    }
}
