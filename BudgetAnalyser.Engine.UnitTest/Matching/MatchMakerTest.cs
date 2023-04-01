using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BudgetAnalyser.Engine.UnitTest.Matching
{
    [TestClass]
    public class MatchmakerTest
    {
        private IEnumerable<MatchingRule> allRules;
        private Mock<IBudgetBucketRepository> mockBudgetBucketRepo;
        private IList<Transaction> testDataTransactions;

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowIfBucketRepoIsNull()
        {
            new Matchmaker(new FakeLogger(), null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowIfLoggerIsNull()
        {
            new Matchmaker(null, this.mockBudgetBucketRepo.Object);
        }

        [TestMethod]
        public void MatchShouldMatchIfReferenceIsBucketCode()
        {
            this.mockBudgetBucketRepo.Setup(m => m.IsValidCode("Foo")).Returns(true);
            this.mockBudgetBucketRepo.Setup(m => m.GetByCode("Foo")).Returns(new SpentPerPeriodExpenseBucket("FOO", "Foo"));
            IList<Transaction> transactions = this.testDataTransactions;
            transactions.First().Reference1 = "Foo";
            var subject = Arrange();

            var result = subject.Match(transactions, new List<MatchingRule>());
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void MatchShouldPreferMatchFromReferenceOverRules()
        {
            this.mockBudgetBucketRepo.Setup(m => m.IsValidCode("Foo")).Returns(true);
            this.mockBudgetBucketRepo.Setup(m => m.GetByCode("Foo")).Returns(new SpentPerPeriodExpenseBucket("FOO", "Foo"));
            IList<Transaction> transactions = this.testDataTransactions;
            var firstTxn = transactions.First();
            firstTxn.Reference1 = "Foo";
            List<MatchingRule> rules = this.allRules.ToList();
            var firstRule = rules.First();
            firstRule.BucketCode = "FOO";
            firstRule.MatchCount = 0;
            firstRule.Reference1 = "Foo";
            firstRule.And = false;
            var subject = Arrange();

            var result = subject.Match(transactions, rules);
            Assert.IsTrue(result);
            Assert.AreEqual(0, firstRule.MatchCount);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfNoMatchesAreMade()
        {
            var subject = Arrange();
            var result = subject.Match(this.testDataTransactions, this.allRules);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnTrueIfRuleMatchesDescription()
        {
            // Customise first rule in list to match something in transaction test data.
            var firstRule = this.allRules.First();
            firstRule.Description = "Engery Online Electricity";
            firstRule.MatchCount = 0;
            firstRule.And = false; // Using OR

            var subject = Arrange();
            
            var result = subject.Match(this.testDataTransactions, this.allRules);
            Assert.IsTrue(result);
            Assert.AreEqual(2, firstRule.MatchCount);
        }

        [TestMethod]
        public void MatchShouldReturnTrueIfRuleMatchesReference1()
        {
            // Customise first rule in list to match something in transaction test data.
            var firstRule = this.allRules.First();
            firstRule.Reference1 = "12334458989";
            firstRule.MatchCount = 0;
            firstRule.And = false; // Using OR

            var subject = Arrange();

            var result = subject.Match(this.testDataTransactions, this.allRules);
            Assert.IsTrue(result);
            Assert.AreEqual(2, firstRule.MatchCount);
        }

        [TestMethod]
        public void MatchShouldReturnTrueIfRuleMatchesAmount()
        {
            // Customise first rule in list to match something in transaction test data.
            var firstRule = this.allRules.First();
            firstRule.Amount = -95.15M;
            firstRule.MatchCount = 0;
            firstRule.And = false; // Using OR

            var subject = Arrange();

            var result = subject.Match(this.testDataTransactions, this.allRules);
            Assert.IsTrue(result);
            Assert.AreEqual(1, firstRule.MatchCount);
        }

        [TestMethod]
        public void MatchShouldReturnTrueIfRuleMatchesAmountAndDescription()
        {
            // Customise first rule in list to match something in transaction test data.
            var firstRule = this.allRules.First();
            firstRule.Amount = -95.15M;
            firstRule.Description = "Engery Online Electricity";
            firstRule.Reference1 = null;
            firstRule.Reference2 = null;
            firstRule.Reference3 = null;
            firstRule.TransactionType = null;
            firstRule.MatchCount = 0;
            firstRule.And = true; // Using AND

            var subject = Arrange();

            var result = subject.Match(this.testDataTransactions, this.allRules);
            Assert.IsTrue(result);
            Assert.AreEqual(1, firstRule.MatchCount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MatchShouldThrowIfGivenNullRulesList()
        {
            var subject = Arrange();
            subject.Match(this.testDataTransactions, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MatchShouldThrowIfGivenNullTransactionList()
        {
            var subject = Arrange();
            subject.Match(null, this.allRules);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            MatchingRulesTestDataGenerated.BucketRepo = new BucketBucketRepoAlwaysFind();
            MatchingRulesTestDataGenerated.BucketRepo.Initialise(null);
            this.allRules = MatchingRulesTestDataGenerated.TestData1();
            this.testDataTransactions = StatementModelTestData.TestData2().WithNullBudgetBuckets().AllTransactions.ToList();
            this.mockBudgetBucketRepo = new Mock<IBudgetBucketRepository>();
        }

        private Matchmaker Arrange()
        {
            return new Matchmaker(new FakeLogger(), this.mockBudgetBucketRepo.Object);
        }
    }
}