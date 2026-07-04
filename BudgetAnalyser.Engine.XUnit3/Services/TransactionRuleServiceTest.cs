using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using NSubstitute;
using Rees.UnitTestUtilities;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Services;

public class TransactionRuleServiceTest
{
    private readonly IBudgetBucketRepository mockBucketRepo;
    private readonly IMatchmaker mockMatchMaker;
    private readonly IMatchingRuleFactory mockRuleFactory;
    private readonly IMatchingRuleRepository mockRuleRepo;
    private readonly TransactionRuleService subject;

    public TransactionRuleServiceTest()
    {
        this.mockRuleRepo = Substitute.For<IMatchingRuleRepository>();
        this.mockMatchMaker = Substitute.For<IMatchmaker>();
        this.mockRuleFactory = Substitute.For<IMatchingRuleFactory>();
        this.mockBucketRepo = new BudgetBucketRepoAlwaysFind();
        var fakeEnvironmentFolders = Substitute.For<IEnvironmentFolders>();

        this.subject = new TransactionRuleService(
            this.mockRuleRepo,
            new FakeLogger(),
            this.mockMatchMaker,
            this.mockRuleFactory,
            fakeEnvironmentFolders,
            new FakeMonitorableDependencies(),
            this.mockBucketRepo);

        PrivateAccessor.SetField(this.subject, "rulesStorageKey", "Any storage key value");
    }

    [Fact]
    public void CreateNewRule_ShouldNotCreateDuplicates()
    {
        this.mockRuleFactory.CreateNewRule(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<string>(), Arg.Any<decimal?>(), Arg.Any<bool>())
            .Returns(new MatchingRule(this.mockBucketRepo) { And = true, BucketCode = TestDataConstants.CarMtcBucketCode, Description = "Test Description" });

        this.subject.CreateNewRule(" ", " ", Array.Empty<string>(), null, null, true);

        this.subject.MatchingRules.Count().ShouldBe(1);
    }

    [Fact]
    public void CreateNewSingleUseRule_ShouldAddRuleToRulesCollection()
    {
        ArrangeForCreateNewRule();

        var result = this.subject.CreateNewSingleUseRule("Foo", "Bar", ["Spock", "Kirk"], "NCC-1701", 1701, true);

        this.subject.MatchingRules.Any(r => r == result).ShouldBeTrue();
    }

    [Fact]
    public void CreateNewSingleUseRule_ShouldCallFactoryToCreateTheRule()
    {
        ArrangeForCreateNewRule();

        var result = this.subject.CreateNewSingleUseRule("Foo", "Bar", ["Spock", "Kirk"], "NCC-1701", 1701, true);

        result.ShouldNotBeNull();
        this.mockRuleFactory.Received(1).CreateNewSingleUseRule(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<string>(), Arg.Any<decimal?>(), Arg.Any<bool>());
    }

    [Fact]
    public void Match_ShouldRemoveSingleUseRulesThatWereUsed()
    {
        var testTransactions = TransactionsListModelTestData.TestData1().Transactions;
        var testMatchingRules = new List<MatchingRule>
        {
            new SingleUseMatchingRule(this.mockBucketRepo)
            {
                Amount = -95.15M,
                And = true,
                BucketCode = TransactionsListModelTestData.PhoneBucket.Code,
                Reference1 = "skjghjkh",
                MatchCount = 1
            },
            new(this.mockBucketRepo)
            {
                Amount = -11.11M,
                BucketCode = TransactionsListModelTestData.CarMtcBucket.Code
            }
        };

        this.mockMatchMaker.Match(Arg.Any<IEnumerable<Transaction>>(), Arg.Any<IEnumerable<MatchingRule>>()).Returns(true);
        this.mockBucketRepo.GetOrCreateNew(TestDataConstants.PowerBucketCode, () => new SpentPerPeriodExpenseBucket(TestDataConstants.PowerBucketCode, "Foo"));
        this.mockBucketRepo.GetOrCreateNew(TestDataConstants.PhoneBucketCode, () => new SpentPerPeriodExpenseBucket(TestDataConstants.PhoneBucketCode, "Foo"));
        PrivateAccessor.InvokeMethod(this.subject, "InitialiseTheRulesCollections", testMatchingRules);
        PrivateAccessor.SetField(this.subject, "rulesStorageKey", "lksjgjklshgjkls");

        var success = this.subject.Match(testTransactions);

        success.ShouldBeTrue();
        this.subject.MatchingRules.Any(r => r is SingleUseMatchingRule).ShouldBeFalse();
    }

    private void ArrangeForCreateNewRule()
    {
        this.mockRuleFactory
            .CreateNewSingleUseRule(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<string>(), Arg.Any<decimal?>(), Arg.Any<bool>())
            .Returns(new SingleUseMatchingRule(this.mockBucketRepo) { BucketCode = "Foo" });

        PrivateAccessor.SetField(this.subject, "rulesStorageKey", "Anything");
    }
}
