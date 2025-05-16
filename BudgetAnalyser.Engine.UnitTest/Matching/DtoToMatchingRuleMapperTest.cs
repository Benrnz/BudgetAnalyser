using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Matching.Data;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;

namespace BudgetAnalyser.Engine.UnitTest.Matching;

[TestClass]
public class DtoToMatchingRuleMapperTest
{
    private static readonly Guid Id = new("901EC4BB-B8B5-43CD-A8C9-15121048CBA4");
    private MatchingRule Result { get; set; }

    private MatchingRuleDto TestData => new(
        123.45M,
        BucketCode: TestDataConstants.PhoneBucketCode,
        Created: new DateTime(2014, 07, 04),
        Description: "The quick brown fox",
        LastMatch: new DateTime(2014, 07, 29),
        MatchCount: 2,
        Reference1: "jumped",
        Reference2: "over",
        Reference3: "the lazy",
        TransactionType: "dog.",
        And: true
    )
    { RuleId = Id };

    [TestMethod]
    public void ShouldMapAmount()
    {
        Assert.AreEqual(TestData.Amount, Result.Amount);
    }

    [TestMethod]
    public void ShouldMapBucket()
    {
        Assert.AreEqual(TestData.BucketCode, Result.Bucket.Code);
    }

    [TestMethod]
    public void ShouldMapBucketCode()
    {
        Assert.AreEqual(TestData.BucketCode, Result.BucketCode);
    }

    [TestMethod]
    public void ShouldMapCreated()
    {
        Assert.AreEqual(TestData.Created?.ToLocalTime(), Result.Created);
    }

    [TestMethod]
    public void ShouldMapDescription()
    {
        Assert.AreEqual(TestData.Description, Result.Description);
    }

    [TestMethod]
    public void ShouldMapId()
    {
        Assert.AreEqual(Id, Result.RuleId);
    }

    [TestMethod]
    public void ShouldMapLastMatch()
    {
        Assert.AreEqual(TestData.LastMatch?.ToLocalTime(), Result.LastMatch);
    }

    [TestMethod]
    public void ShouldMapMatchCount()
    {
        Assert.AreEqual(TestData.MatchCount, Result.MatchCount);
    }

    [TestMethod]
    public void ShouldMapReference1()
    {
        Assert.AreEqual(TestData.Reference1, Result.Reference1);
    }

    [TestMethod]
    public void ShouldMapReference2()
    {
        Assert.AreEqual(TestData.Reference2, Result.Reference2);
    }

    [TestMethod]
    public void ShouldMapReference3()
    {
        Assert.AreEqual(TestData.Reference3, Result.Reference3);
    }

    [TestMethod]
    public void ShouldMapTransactionType()
    {
        Assert.AreEqual(TestData.TransactionType, Result.TransactionType);
    }

    [TestInitialize]
    public void TestInitialise()
    {
        var subject = new MapperMatchingRuleToDto2(new BucketBucketRepoAlwaysFind());
        Result = subject.ToModel(TestData);
    }
}
