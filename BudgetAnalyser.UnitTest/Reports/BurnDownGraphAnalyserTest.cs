using System;
using System.Linq;
using BudgetAnalyser.Engine;
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

        private SeriesData BudgetLine
        {
            get { return Subject.GraphLines.Series.Single(s => s.SeriesName == BurnDownGraphAnalyser.BudgetSeriesName); }
        }

        private SeriesData ZeroLine
        {
            get { return Subject.GraphLines.Series.Single(s => s.SeriesName == BurnDownGraphAnalyser.ZeroSeriesName); }
        }

        private SeriesData BalanceLine
        {
            get { return Subject.GraphLines.Series.Single(s => s.SeriesName == BurnDownGraphAnalyser.BalanceSeriesName); }
        }

        [TestMethod]
        public void AnalyseShouldReturn31DaysOfBalanceLineElements()
        {
            Assert.AreEqual(31, BalanceLine.Plots.Count());
        }

        [TestMethod]
        public void AnalyseShouldReturn31DaysOfBudgetElements()
        {
            Assert.AreEqual(31, BudgetLine.Plots.Count());
        }

        [TestMethod]
        public void AnalyseShouldReturn31DaysOfZeroLineElements()
        {
            Assert.AreEqual(31, ZeroLine.Plots.Count());
        }

        [TestMethod]
        public void AnalyseShouldReturnAFirstBudgetElementEqualToTheLedgerAmountAvailableGivenValidLedger()
        {
            Assert.AreEqual(3635M, BudgetLine.Plots.First().Amount);
        }

        [TestMethod]
        public void AnalyseShouldReturnAFirstBudgetElementEqualToTheMonthlyBudgetAmountGivenNullLedger()
        {
            LedgerBook = null;
            Act();
            Assert.AreEqual(1435M, BudgetLine.Plots.First().Amount);
        }

        [TestMethod]
        public void AnalyseShouldReturnALastBudgetElementOfOneThirtythOfTheFirst()
        {
            decimal expected = decimal.Round(BudgetLine.Plots.First().Amount / 31, 2);
            Assert.AreEqual(expected, decimal.Round(BudgetLine.Plots.Last().Amount, 2));
        }

        [TestMethod]
        public void AnalyseShouldReturnBalanceLineAxesMinimumOf0()
        {
            Assert.AreEqual(0, Subject.GraphLines.GraphMinimumValue);
        }

        [TestMethod]
        public void AnalyseShouldReturnBalanceLineLineElementsTotalingTo104745()
        {
            Assert.AreEqual(104745.78M, BalanceLine.Plots.Sum(z => z.Amount));
        }

        [TestMethod]
        public void AnalyseShouldReturnZeroLineElementsTotalingToZero()
        {
            Assert.AreEqual(0, ZeroLine.Plots.Sum(z => z.Amount));
        }

        [TestMethod]
        public void AnalyseShouldReturnReportTransactionsAmountsTotaling3376()
        {
            Assert.AreEqual(3376.34M, Subject.ReportTransactions.Sum(t => t.Amount));
        }

        [TestMethod]
        public void AnalyseShouldReturnALastReportTransactionsElementWithBalanceEqualTo3376()
        {
            foreach (var transaction in Subject.ReportTransactions)
            {
                Console.WriteLine("{0} {1} {2:N} {3:N}", transaction.Date, transaction.Narrative.Truncate(30).PadRight(30), transaction.Amount.ToString().Truncate(10).PadRight(10), transaction.Balance);
            }
      
            Assert.AreEqual(3376.34M, Subject.ReportTransactions.Last().Balance);
        }

        [TestMethod]
        public void AnalyseShouldReturn5ReportTransactions()
        {
            Assert.AreEqual(5, Subject.ReportTransactions.Count());
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
                new DateTime(2013, 07, 15),
                LedgerBook);
        }
    }
}