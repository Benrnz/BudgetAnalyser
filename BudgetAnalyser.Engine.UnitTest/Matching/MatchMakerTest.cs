using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BudgetAnalyser.Engine.UnitTest.Matching
{
    [TestClass]
    public class MatchmakerTest
    {
        private Mock<IBudgetBucketRepository> mockBudgetBucketRepo;

        private IEnumerable<MatchingRule> AllRules { get; set; }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowIfLoggerIsNull()
        {
            new Matchmaker(null, this.mockBudgetBucketRepo.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowIfBucketRepoIsNull()
        {
            new Matchmaker(new FakeLogger(), null);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfNoMatchesAreMade()
        {
            Matchmaker subject = Arrange();
            bool result = subject.Match(StatementModelTestData.TestData2().AllTransactions, AllRules);
            Assert.IsFalse(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MatchShouldThrowIfGivenNullRulesList()
        {
            Matchmaker subject = Arrange();
            subject.Match(StatementModelTestData.TestData2().AllTransactions, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MatchShouldThrowIfGivenNullTransactionList()
        {
            Matchmaker subject = Arrange();
            subject.Match(null, AllRules);
        }

        [TestMethod]
        public void MatchShouldMatchIfReferenceIsBucketCode()
        {
            this.mockBudgetBucketRepo.Setup(m => m.IsValidCode("Foo")).Returns(true);
            this.mockBudgetBucketRepo.Setup(m => m.GetByCode("Foo")).Returns(new SpentMonthlyExpenseBucket("FOO", "Foo"));
            var transactions = StatementModelTestData.TestData2().AllTransactions;
            transactions.First().Reference1 = "Foo";
            var subject = Arrange();

            var result = subject.Match(transactions, new List<MatchingRule>());
            Assert.IsTrue(result);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            MatchingRulesTestDataGenerated.BucketRepo = new BucketBucketRepoAlwaysFind();
            MatchingRulesTestDataGenerated.BucketRepo.Initialise(null);
            AllRules = MatchingRulesTestDataGenerated.TestData1();

            this.mockBudgetBucketRepo = new Mock<IBudgetBucketRepository>();
        }

        private Matchmaker Arrange()
        {
            return new Matchmaker(new FakeLogger(), this.mockBudgetBucketRepo.Object);
        }
    }
}