using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Matching.Data;
using BudgetAnalyser.Engine.UnitTest.TestHarness;

namespace BudgetAnalyser.Engine.UnitTest.Matching;

[TestClass]
public class DataMatchingRuleToMatchingRuleMapperTest
{
    private MatchingRuleDto TestData { get; set; }

    [TestMethod]
    public void AmountShouldBeMapped()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(TestData.Amount, result.Amount);
    }

    [TestMethod]
    public void BucketCodeShouldBeMapped()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(TestData.BucketCode, result.BucketCode);
    }

    [TestMethod]
    public void CreatedDatesShouldBeMapped()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(TestData.Created?.ToLocalTime(), result.Created);
    }

    [TestMethod]
    public void DescriptionShouldBeMapped()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(TestData.Description, result.Description);
    }

    [TestMethod]
    public void LastMatchShouldBeMapped()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(TestData.LastMatch?.ToLocalTime(), result.LastMatch);
    }

    [TestMethod]
    public void MatchCountShouldBeMapped()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(TestData.MatchCount, result.MatchCount);
    }

    [TestMethod]
    [Description("A test designed to break when new propperties are added to the MatchingRuleDto. This is a trigger to update the mappers.")]
    public void NumberOfDataMatchingRulePropertiesShouldBe12()
    {
        var dataProperties = typeof(MatchingRule).CountProperties();
        Assert.AreEqual(13, dataProperties);
    }

    [TestMethod]
    public void Reference1ShouldBeMapped()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(TestData.Reference1, result.Reference1);
    }

    [TestMethod]
    public void Reference2ShouldBeMapped()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(TestData.Reference2, result.Reference2);
    }

    [TestMethod]
    public void Reference3ShouldBeMapped()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(TestData.Reference3, result.Reference3);
    }

    [TestMethod]
    public void RuleIdShouldBeMapped()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(TestData.RuleId, result.RuleId);
    }

    [TestInitialize]
    public void TestInitialise()
    {
        TestData = new MatchingRuleDto
        {
            Created = new DateTime(2014, 1, 2),
            Amount = 123.45M,
            BucketCode = "CARMTC",
            Description = "Testing Description",
            LastMatch = new DateTime(2014, 06, 22),
            MatchCount = 2,
            Reference1 = "Testing Reference1",
            Reference2 = "Testing Reference2",
            Reference3 = "Testing Reference3",
            TransactionType = "Testing TransactionType"
        };
    }

    [TestMethod]
    public void TransactionTypeShouldBeMapped()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(TestData.TransactionType, result.TransactionType);
    }

    private MatchingRule ArrangeAndAct()
    {
        var subject = new MapperMatchingRuleToDto2(new BucketBucketRepoAlwaysFind());
        return subject.ToModel(TestData);
    }
}
