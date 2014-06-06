using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.UnitTest.TestData;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Matching
{
    [TestClass]
    public class MatchmakerTest
    {
        [TestMethod]
        public void Test1()
        {
            var subject = Arrange();
            var selectedRules = MatchingRulesTestData.TestData1();

            var result = subject.Match(StatementModelTestData.TestData2().AllTransactions, selectedRules);

            Assert.IsTrue(result);
        }

        private static Matchmaker Arrange()
        {
            return new Matchmaker(new FakeLogger());
        }
    }
}
