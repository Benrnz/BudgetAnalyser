using System;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Matching.Data;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Matching
{
    [TestClass]
    public class MatchingRuleToDataMatchingRuleMapperTest
    {
        private MatchingRule TestData { get; set; }

        [TestMethod]
        public void AmountShouldBeMapped()
        {
            MatchingRuleDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.Amount, result.Amount);
        }

        [TestMethod]
        public void BucketCodeShouldBeMapped()
        {
            MatchingRuleDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.BucketCode, result.BucketCode);
        }

        [TestMethod]
        public void CreatedDatesShouldBeMapped()
        {
            MatchingRuleDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.Created, result.Created);
        }

        [TestMethod]
        public void DescriptionShouldBeMapped()
        {
            MatchingRuleDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.Description, result.Description);
        }

        [TestMethod]
        public void LastMatchShouldBeMapped()
        {
            MatchingRuleDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.LastMatch, result.LastMatch);
        }

        [TestMethod]
        public void MatchCountShouldBeMapped()
        {
            MatchingRuleDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.MatchCount, result.MatchCount);
        }

        [TestMethod]
        [Description("A test designed to break when new propperties are added to the MatchingRuleDto. This is a trigger to update the mappers.")]
        public void NumberOfDataMatchingRulePropertiesShouldBe12()
        {
            int dataProperties = typeof(MatchingRuleDto).CountProperties();
            Assert.AreEqual(12, dataProperties);
        }

        [TestMethod]
        public void Reference1ShouldBeMapped()
        {
            MatchingRuleDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.Reference1, result.Reference1);
        }

        [TestMethod]
        public void Reference2ShouldBeMapped()
        {
            MatchingRuleDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.Reference2, result.Reference2);
        }

        [TestMethod]
        public void Reference3ShouldBeMapped()
        {
            MatchingRuleDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.Reference3, result.Reference3);
        }

        [TestMethod]
        public void RuleIdShouldBeMapped()
        {
            MatchingRuleDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.RuleId, result.RuleId);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            TestData = new MatchingRule(new BucketBucketRepoAlwaysFind())
            {
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
            MatchingRuleDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.TransactionType, result.TransactionType);
        }

        private MatchingRuleDto ArrangeAndAct()
        {
            var subject = new Mapper_MatchingRuleDto_MatchingRule(new BucketBucketRepoAlwaysFind());
            return subject.ToDto(TestData);
        }
    }
}