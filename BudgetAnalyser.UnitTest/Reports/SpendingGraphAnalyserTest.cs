using BudgetAnalyser.Engine.Budget;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Reports
{
    [TestClass]
    public class SpendingGraphAnalyserTest
    {
        public SpendingGraphAnalyserTest()
        {
            SurplusTestBucket = new SurplusBucket();
        }

        private BudgetBucket SurplusTestBucket { get; set; }
    }
}