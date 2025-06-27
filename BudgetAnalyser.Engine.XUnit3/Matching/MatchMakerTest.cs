using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using NSubstitute;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Matching;

public class MatchMakerTest
{
    private readonly IEnumerable<MatchingRule> allRules;
    private readonly ILogger logger;
    private readonly IBudgetBucketRepository mockBudgetBucketRepo;
    private readonly MatchingRulesTestDataGenerated testData = new();
    private readonly IList<Transaction> testDataTransactions;

    public MatchMakerTest(ITestOutputHelper testOutputHelper)
    {
        this.testData.BucketRepo = new BudgetBucketRepoAlwaysFind().Initialise(Array.Empty<BudgetBucketDto>());
        this.allRules = this.testData.TestData1();
        this.testDataTransactions = StatementModelTestData.TestData2().WithNullBudgetBuckets().AllTransactions.ToList();
        this.mockBudgetBucketRepo = Substitute.For<IBudgetBucketRepository>();
        this.logger = new XUnitLogger(testOutputHelper);
    }

    [Fact]
    public void CtorShouldThrowIfBucketRepoIsNull()
    {
        Should.Throw<ArgumentNullException>(() => new Matchmaker(new FakeLogger(), null!));
    }

    [Fact]
    public void CtorShouldThrowIfLoggerIsNull()
    {
        Should.Throw<ArgumentNullException>(() => new Matchmaker(null!, this.mockBudgetBucketRepo));
    }

    [Fact]
    public void MatchShouldMatchIfReference1IsBucketCode()
    {
        this.mockBudgetBucketRepo.IsValidCode("FOO").Returns(true);
        this.mockBudgetBucketRepo.GetByCode("FOO").Returns(new SpentPerPeriodExpenseBucket("FOO", "Foo"));
        var transactions = this.testDataTransactions;
        transactions.First().Reference1 = "Foo";
        var subject = Arrange();

        var result = subject.Match(transactions, new List<MatchingRule>());
        result.ShouldBeTrue();
    }

    [Fact]
    public void MatchShouldMatchIfReference2IsBucketCode()
    {
        this.mockBudgetBucketRepo.IsValidCode("FOO").Returns(true);
        this.mockBudgetBucketRepo.GetByCode("FOO").Returns(new SpentPerPeriodExpenseBucket("FOO", "Foo"));
        var transactions = this.testDataTransactions;
        transactions.First().Reference2 = "Foo";
        var subject = Arrange();

        var result = subject.Match(transactions, new List<MatchingRule>());
        result.ShouldBeTrue();
    }

    [Fact]
    public void MatchShouldMatchIfReference3IsBucketCode()
    {
        this.mockBudgetBucketRepo.IsValidCode("FOO").Returns(true);
        this.mockBudgetBucketRepo.GetByCode("FOO").Returns(new SpentPerPeriodExpenseBucket("FOO", "Foo"));
        var transactions = this.testDataTransactions;
        transactions.First().Reference3 = "Foo";
        var subject = Arrange();

        var result = subject.Match(transactions, new List<MatchingRule>());
        result.ShouldBeTrue();
    }

    [Theory]
    [InlineData("FOOBAR", "FOOBAR", true)]
    [InlineData("FOOBAR", "FOOBA", true)]
    [InlineData("FOOBAR", "", false)]
    [InlineData("FOOBAR", null, false)]
    [InlineData("FOOBAR", "   ", false)]
    public void MatchShouldMatchIfReferenceIsPartialBucketCode(string bucketeCode, string? reference, bool expectedResult)
    {
        var bucket = new SpentPerPeriodExpenseBucket(bucketeCode, "Foo");
        this.mockBudgetBucketRepo.IsValidCode(bucketeCode).Returns(true);
        this.mockBudgetBucketRepo.GetByCode(bucketeCode).Returns(bucket);
        this.mockBudgetBucketRepo.Buckets.Returns([bucket]);
        var transactions = this.testDataTransactions;
        transactions.First().Reference1 = reference; // At least 4 characters of the bucket code.
        var subject = Arrange();

        var result = subject.Match(transactions, new List<MatchingRule>());
        result.ShouldBe(expectedResult);
    }

    [Theory]
    [InlineData("FOOBARF", "Foobarft", true)]
    [InlineData("FOOBARF", "FoObArft", true)]
    [InlineData("FOOBAR", "FOOBARf123456789", false)]
    public void MatchShouldMatchIfReferenceIsSupersetBucketCode(string bucketCode, string reference, bool expectedResult)
    {
        var bucket = new SpentPerPeriodExpenseBucket(bucketCode, "Foo");
        this.mockBudgetBucketRepo.IsValidCode(bucketCode).Returns(true);
        this.mockBudgetBucketRepo.GetByCode(bucketCode).Returns(bucket);
        this.mockBudgetBucketRepo.Buckets.Returns([bucket]);
        var transactions = this.testDataTransactions;
        transactions.First().Reference1 = reference; // At least 7 characters of the bucket code.
        var subject = Arrange();

        var result = subject.Match(transactions, new List<MatchingRule>());
        result.ShouldBe(expectedResult);
    }

    [Fact]
    public void MatchShouldPreferMatchFromReferenceOverRules()
    {
        this.mockBudgetBucketRepo.IsValidCode("FOO").Returns(true);
        var bucket = new SpentPerPeriodExpenseBucket("FOO", "Foo");
        this.mockBudgetBucketRepo.GetByCode("FOO").Returns(bucket);
        this.mockBudgetBucketRepo.Buckets.Returns([bucket]);
        var transactions = this.testDataTransactions;
        var firstTxn = transactions.First();
        firstTxn.Reference1 = "Foo";
        var rules = this.allRules.ToList();
        var firstRule = rules.First();
        firstRule.BucketCode = "FOO";
        firstRule.MatchCount = 0;
        firstRule.Reference1 = "Foo";
        firstRule.And = false;
        var subject = Arrange();

        var result = subject.Match(transactions, rules);
        result.ShouldBeTrue();
        firstRule.MatchCount.ShouldBe(0);
    }

    [Fact]
    public void MatchShouldReturnFalseIfNoMatchesAreMade()
    {
        var subject = Arrange();
        var result = subject.Match(this.testDataTransactions, this.allRules);
        result.ShouldBeFalse();
    }

    [Fact]
    public void MatchShouldReturnTrueIfRuleMatchesAmount()
    {
        var firstRule = this.allRules.First();
        firstRule.Amount = -95.15M;
        firstRule.MatchCount = 0;
        firstRule.And = false;

        var subject = Arrange();

        var result = subject.Match(this.testDataTransactions, this.allRules);
        result.ShouldBeTrue();
        firstRule.MatchCount.ShouldBe(1);
    }

    [Fact]
    public void MatchShouldReturnTrueIfRuleMatchesAmountAndDescription()
    {
        var firstRule = this.allRules.First();
        firstRule.Amount = -95.15M;
        firstRule.Description = "Engery Online Electricity";
        firstRule.Reference1 = null;
        firstRule.Reference2 = null;
        firstRule.Reference3 = null;
        firstRule.TransactionType = null;
        firstRule.MatchCount = 0;
        firstRule.And = true;

        var subject = Arrange();

        var result = subject.Match(this.testDataTransactions, this.allRules);
        result.ShouldBeTrue();
        firstRule.MatchCount.ShouldBe(1);
    }

    [Fact]
    public void MatchShouldReturnTrueIfRuleMatchesDescription()
    {
        var firstRule = this.allRules.First();
        firstRule.Description = "Engery Online Electricity";
        firstRule.MatchCount = 0;
        firstRule.And = false;

        var subject = Arrange();

        var result = subject.Match(this.testDataTransactions, this.allRules);
        result.ShouldBeTrue();
        firstRule.MatchCount.ShouldBe(2);
    }

    [Fact]
    public void MatchShouldReturnTrueIfRuleMatchesReference1()
    {
        var firstRule = this.allRules.First();
        firstRule.Reference1 = "12334458989";
        firstRule.MatchCount = 0;
        firstRule.And = false;

        var subject = Arrange();

        var result = subject.Match(this.testDataTransactions, this.allRules);
        result.ShouldBeTrue();
        firstRule.MatchCount.ShouldBe(2);
    }

    [Fact]
    public void MatchShouldThrowIfGivenNullRulesList()
    {
        var subject = Arrange();
        Should.Throw<ArgumentNullException>(() => subject.Match(this.testDataTransactions, null!));
    }

    [Fact]
    public void MatchShouldThrowIfGivenNullTransactionList()
    {
        var subject = Arrange();
        Should.Throw<ArgumentNullException>(() => subject.Match(null!, this.allRules));
    }

    private Matchmaker Arrange()
    {
        return new Matchmaker(this.logger, this.mockBudgetBucketRepo);
    }
}
