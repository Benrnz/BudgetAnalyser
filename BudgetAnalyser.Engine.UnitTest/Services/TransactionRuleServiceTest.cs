using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rees.UnitTestUtilities;

namespace BudgetAnalyser.Engine.UnitTest.Services
{
    [TestClass]
    public class TransactionRuleServiceTest
    {
        private IBudgetBucketRepository mockBucketRepo;
        private Mock<IMatchmaker> mockMatchMaker;
        private Mock<IMatchingRuleFactory> mockRuleFactory;
        private Mock<IMatchingRuleRepository> mockRuleRepo;
        private TransactionRuleService subject;

        [TestMethod]
        public void CreateNewSingleUseRule_ShouldAddRuleToRulesCollection()
        {
            ArrangeForCreateNewRule();

            SingleUseMatchingRule result = this.subject.CreateNewSingleUseRule("Foo", "Bar", new[] { "Spock", "Kirk" }, "NCC-1701", 1701, true);

            Assert.IsTrue(this.subject.MatchingRules.Any(r => r == result));
        }

        [TestMethod]
        public void CreateNewSingleUseRule_ShouldCallFactoryToCreateTheRule()
        {
            ArrangeForCreateNewRule();

            SingleUseMatchingRule result = this.subject.CreateNewSingleUseRule("Foo", "Bar", new[] { "Spock", "Kirk" }, "NCC-1701", 1701, true);

            Assert.IsNotNull(result);
            this.mockRuleFactory.Verify();
        }

        [TestMethod]
        public void Match_ShouldRemoveSingleUseRulesThatWereUsed()
        {
            IEnumerable<Transaction> testTransactions = StatementModelTestData.TestData1().Transactions;
            var testMatchingRules = new List<MatchingRule>
            {
                new SingleUseMatchingRule(this.mockBucketRepo)
                {
                    Amount = -95.15M,
                    And = true,
                    BucketCode = StatementModelTestData.PhoneBucket.Code,
                    Reference1 = "skjghjkh",
                    MatchCount = 1 // Artificially set to simulate a match
                },
                new MatchingRule(this.mockBucketRepo)
                {
                    Amount = -11.11M,
                    BucketCode = StatementModelTestData.CarMtcBucket.Code
                }
            };

            this.mockMatchMaker.Setup(m => m.Match(testTransactions, testMatchingRules)).Returns(true);
            PrivateAccessor.InvokeMethod(this.subject, "InitialiseTheRulesCollections", testMatchingRules);
            PrivateAccessor.SetField<TransactionRuleService>(this.subject, "rulesStorageKey", "lksjgjklshgjkls");

            bool success = this.subject.Match(testTransactions);

            Assert.IsTrue(success);
            Assert.IsFalse(this.subject.MatchingRules.Any(r => r is SingleUseMatchingRule));
        }

        [TestMethod]
        public void CreateNewRule_ShouldNotCreateDuplicates()
        {
            this.mockRuleFactory.Setup(m => m.CreateNewRule(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<decimal?>(), It.IsAny<bool>()))
                .Returns(new MatchingRule(this.mockBucketRepo) { And = true, BucketCode = TestDataConstants.CarMtcBucketCode, Description = "Test Description" });

            var newRule = this.subject.CreateNewRule(" ", " ", new string[] { }, null, null, true);

            Assert.AreEqual(1, this.subject.MatchingRules.Count());
        }

        [TestInitialize]
        public void TestInitialise()
        {
            this.mockRuleRepo = new Mock<IMatchingRuleRepository>();
            this.mockMatchMaker = new Mock<IMatchmaker>();
            this.mockRuleFactory = new Mock<IMatchingRuleFactory>();
            this.mockBucketRepo = new BucketBucketRepoAlwaysFind();

            this.subject = new TransactionRuleService(
                this.mockRuleRepo.Object, 
                new FakeLogger(), 
                this.mockMatchMaker.Object, 
                this.mockRuleFactory.Object, 
                new FakeEnvironmentFolders(),
                new FakeMonitorableDependencies());
            
            PrivateAccessor.SetField(this.subject, "rulesStorageKey", "Any storage key value");
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