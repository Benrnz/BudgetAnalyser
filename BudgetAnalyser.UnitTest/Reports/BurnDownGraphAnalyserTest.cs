using System;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.UnitTest.TestData;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Reports
{
    [TestClass]
    public class BurnDownGraphAnalyserTest
    {
        private BudgetModel Budget { get; set; }
        private LedgerBook LedgerBook { get; set; }
        private StatementModel StatementModel { get; set; }
        private BurnDownGraphAnalyser Subject { get; set; }

        [TestMethod]
        public void AnalyseShouldReturn30DaysOfActualSpendingElements()
        {
            Assert.AreEqual(30, Subject.ActualSpending.Count());
        }

        [TestMethod]
        public void AnalyseShouldReturn30DaysOfBudgetElements()
        {
            Assert.AreEqual(30, Subject.BudgetLine.Count());
        }

        [TestMethod]
        public void AnalyseShouldReturn30DaysOfZeroLineElements()
        {
            Assert.AreEqual(30, Subject.ZeroLine.Count());
        }

        [TestMethod]
        public void AnalyseShouldReturnAFirstBudgetElementEqualToTheMonthlyBudgetAmountGivenNullLedger()
        {
            LedgerBook = null;
            Act();
            Assert.AreEqual(1435M, Subject.BudgetLine.First().Value);
        }

        [TestMethod]
        public void AnalyseShouldReturnAFirstBudgetElementEqualToTheLedgerAmountAvailableGivenValidLedger()
        {
            Assert.AreEqual(2490M, Subject.BudgetLine.First().Value);
        }

        [TestMethod]
        public void AnalyseShouldReturnALastBudgetElementOfOneThirtythOfTheFirst()
        {
            decimal expected = decimal.Round(Subject.BudgetLine.First().Value / 30, 2);
            Assert.AreEqual(expected, decimal.Round(Subject.BudgetLine.Last().Value, 2));
        }

        [TestMethod]
        public void AnalyseShouldReturnZeroLineElementsTotalingToZero()
        {
            Assert.AreEqual(0, Subject.ZeroLine.Sum(z => z.Value));
        }

        [TestInitialize]
        public void TestInitialise()
        {
            Subject = new BurnDownGraphAnalyser(new LedgerCalculation(), new FakeLogger());
            Budget = BudgetModelTestData.CreateTestData2();
            LedgerBook = LedgerBookTestData.TestData2();
            StatementModel = StatementModelTestData.TestData3();
            
            Act();
        }

        private void Act()
        {
            Subject.Analyse(
                StatementModel,
                Budget,
                new BudgetBucket[] { StatementModelTestData.PhoneBucket, StatementModelTestData.PowerBucket, new SurplusBucket() },
                new DateTime(2013, 06, 15),
                LedgerBook);
        }
    }
}