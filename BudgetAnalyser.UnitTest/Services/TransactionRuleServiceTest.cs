using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rees.TestUtilities;

namespace BudgetAnalyser.UnitTest.Services
{
    [TestClass]
    public class TransactionRuleServiceTest
    {
        private Mock<IMatchmaker> mockMatchMaker;
        private Mock<IMatchingRuleFactory> mockRuleFactory;
        private Mock<IMatchingRuleRepository> mockRuleRepo;
        private IBudgetBucketRepository mockBucketRepo;
        private TransactionRuleService subject;

        [TestInitialize]
        public void TestInitialise()
        {
            this.mockRuleRepo = new Mock<IMatchingRuleRepository>();
            this.mockMatchMaker = new Mock<IMatchmaker>();
            this.mockRuleFactory = new Mock<IMatchingRuleFactory>();
            this.mockBucketRepo = new BucketBucketRepoAlwaysFind();

            this.subject = new TransactionRuleService(this.mockRuleRepo.Object, new FakeLogger(), this.mockMatchMaker.Object, this.mockRuleFactory.Object);
        }

        [TestMethod]
        public void Match_ShouldRemoveSingleUseRulesThatWereUsed()
        {
            var testTransactions = TestData.StatementModelTestData.TestData1().Transactions;
            var testMatchingRules = new List<MatchingRule>()
            {
                new SingleUseMatchingRule(this.mockBucketRepo)
                {
                    Amount = -95.15M,
                    And = true,
                    BucketCode = TestData.StatementModelTestData.PhoneBucket.Code,
                    Reference1 = "skjghjkh",
                    MatchCount = 1 // Artificially set to simulate a match
                },
                new MatchingRule(this.mockBucketRepo)
                {
                    Amount = -11.11M,
                    BucketCode = TestData.StatementModelTestData.CarMtcBucket.Code,
                },
            };

            this.mockMatchMaker.Setup(m => m.Match(testTransactions, testMatchingRules)).Returns(true);
            PrivateAccessor.InvokeMethod(this.subject, "InitialiseTheRulesCollections", testMatchingRules);
            PrivateAccessor.SetField<TransactionRuleService>(this.subject, "rulesStorageKey", "lksjgjklshgjkls");

            var success = this.subject.Match(testTransactions);
            
            Assert.IsTrue(success);
            Assert.IsFalse(this.subject.MatchingRules.Any(r => r is SingleUseMatchingRule));
        }

        [TestMethod]
        public void CreateNewSingleUseRule_ShouldCallFactoryToCreateTheRule()
        {
            ArrangeForCreateNewRule();

            var result = this.subject.CreateNewSingleUseRule("Foo", "Bar", new[] { "Spock", "Kirk" }, "NCC-1701", 1701, true);

            Assert.IsNotNull(result);
            this.mockRuleFactory.Verify();
        }

        [TestMethod]
        public void CreateNewSingleUseRule_ShouldAddRuleToRulesCollection()
        {
            ArrangeForCreateNewRule();

            var result = this.subject.CreateNewSingleUseRule("Foo", "Bar", new[] { "Spock", "Kirk" }, "NCC-1701", 1701, true);

            Assert.IsTrue(this.subject.MatchingRules.Any(r => r == result));
        }

        private void ArrangeForCreateNewRule()
        {
            this.mockRuleFactory
                .Setup(m => m.CreateNewSingleUseRule(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<decimal?>(), It.IsAny<bool>()))
                .Returns(new SingleUseMatchingRule(this.mockBucketRepo) { BucketCode = "Foo" });

            // This is to bypass validating that Initialise has happened when adding a new rule
            PrivateAccessor.SetField<TransactionRuleService>(this.subject, "rulesStorageKey", "Anything");
        }
    }
}