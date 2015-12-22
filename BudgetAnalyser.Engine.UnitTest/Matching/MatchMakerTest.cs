using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.UnitTest.TestData;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Matching
{
    [TestClass]
    public class MatchmakerTest
    {
        private IEnumerable<MatchingRule> AllRules { get; set; }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowIfLoggerIsNull()
        {
            new Matchmaker(null);
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

        [TestInitialize]
        public void TestInitialise()
        {
            MatchingRulesTestDataGenerated.BucketRepo = new BucketBucketRepoAlwaysFind();
            MatchingRulesTestDataGenerated.BucketRepo.Initialise(null);
            AllRules = MatchingRulesTestDataGenerated.TestData1();
        }

        private static Matchmaker Arrange()
        {
            return new Matchmaker(new FakeLogger());
        }
    }
}