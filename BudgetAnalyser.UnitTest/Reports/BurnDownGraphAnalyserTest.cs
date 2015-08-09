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
private  SeriesData BalanceLine => Result.GraphLines.Series.Single(s => s.SeriesName == BurnDownChartAnalyserResult.BalanceSeriesName);

        private BudgetModel Budget { get; set; }

private  SeriesData BudgetLine => Result.GraphLines.Series.Single(s => s.SeriesName == BurnDownChartAnalyserResult.BudgetSeriesName);

        private LedgerBook LedgerBook { get; set; }
        private BurnDownChartAnalyserResult Result { get; set; }
        private StatementModel StatementModel { get; set; }
        private BurnDownChartAnalyser Subject { get; set; }

private  SeriesData ZeroLine => Result.GraphLines.Series.Single(s => s.SeriesName == BurnDownChartAnalyserResult.ZeroSeriesName);

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
        public void AnalyseShouldReturn5ReportTransactions()
        {
            Assert.AreEqual(5, Result.ReportTransactions.Count());
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
        public void AnalyseShouldReturnALastReportTransactionsElementWithBalanceEqualTo3376()
        {
            foreach (ReportTransactionWithRunningBalance transaction in Result.ReportTransactions)
            {
                Console.WriteLine(
                    "{0} {1} {2:N} {3:N}",
                    transaction.Date,
                    transaction.Narrative.Truncate(30).PadRight(30),
                    transaction.Amount.ToString().Truncate(10).PadRight(10),
                    transaction.Balance);
            }

            Assert.AreEqual(3376.34M, Result.ReportTransactions.Last().Balance);
        }

        [TestMethod]
        public void AnalyseShouldReturnBalanceLineAxesMinimumOf0()
        {
            Assert.AreEqual(0, Result.GraphLines.GraphMinimumValue);
        }

        [TestMethod]
        public void AnalyseShouldReturnBalanceLineLineElementsTotalingTo104745()
        {
            Assert.AreEqual(104745.78M, BalanceLine.Plots.Sum(z => z.Amount));
        }

        [TestMethod]
        public void AnalyseShouldReturnReportTransactionsAmountsTotaling3376()
        {
            Assert.AreEqual(3376.34M, Result.ReportTransactions.Sum(t => t.Amount));
        }

        [TestMethod]
        public void AnalyseShouldReturnZeroLineElementsTotalingToZero()
        {
            Assert.AreEqual(0, ZeroLine.Plots.Sum(z => z.Amount));
        }

        [TestInitialize]
        public void TestInitialise()
        {
            Subject = new BurnDownChartAnalyser(new LedgerCalculation(), new FakeLogger());
            Budget = BudgetModelTestData.CreateTestData2();
            LedgerBook = LedgerBookTestData.TestData2();
            StatementModel = StatementModelTestData.TestData3();

            Act();
        }

        private void Act()
        {
            Result = Subject.Analyse(
                StatementModel,
                Budget,
                new BudgetBucket[] { StatementModelTestData.PhoneBucket, StatementModelTestData.PowerBucket, new SurplusBucket() },
                LedgerBook,
                new DateTime(2013, 07, 15));
        }
    }
}