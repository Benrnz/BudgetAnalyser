using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.UnitTest.Helper;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Ledger
{
    [TestClass]
    public class LedgerCalculationTest
    {
        private LedgerCalculation Subject { get; set; }
        private LedgerBook TestData { get; set; }

        [TestMethod]
        public void CalculateOverSpentLedgersShouldReturnEmptyGivenNoLedgerLineForGivenDate()
        {
            IEnumerable<ReportTransaction> result = Subject.CalculateOverspentLedgers(StatementModelTestData.TestData1(), TestData, new DateTime(2014, 07, 01));
            Assert.IsTrue(result.Count() == 0);
        }

        [TestMethod]
        public void CalculateOverSpentLedgersShouldReturnEmptyGivenNoLedgersWereOverdrawn()
        {
            TestData.Output(true);
            IEnumerable<ReportTransaction> result = Subject.CalculateOverspentLedgers(StatementModelTestData.TestData3(), TestData, new DateTime(2013, 08, 15));
            foreach (ReportTransaction txn in result)
            {
                Console.WriteLine("{0} {1} {2}", txn.Date, txn.Narrative, txn.Amount);
            }

            Assert.IsTrue(result.Count() == 0);
        }

        [TestMethod]
        public void CalculateOverSpentLedgersShouldReturnOverdrawnTransactionsGivenStatementTransactionsSpendMoreThanLedgerBalance()
        {
            TestData.Output(true);
            IEnumerable<ReportTransaction> result = Subject.CalculateOverspentLedgers(StatementModelTestData.TestData2(), TestData, new DateTime(2013, 08, 15));
            foreach (ReportTransaction txn in result)
            {
                Console.WriteLine("{0} {1} {2}", txn.Date, txn.Narrative, txn.Amount);
            }

            Assert.AreEqual(-40.41M, result.Sum(t => t.Amount));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CalculateOverSpentLedgersShouldThrowGivenNullLedger()
        {
            Subject.CalculateOverspentLedgers(StatementModelTestData.TestData1(), null, new DateTime(2014, 07, 01));
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CalculateOverSpentLedgersShouldThrowGivenNullStatement()
        {
            Subject.CalculateOverspentLedgers(null, TestData, new DateTime(2014, 07, 01));
            Assert.Fail();
        }

        [TestInitialize]
        public void TestInitialise()
        {
            Subject = new LedgerCalculation();
            TestData = LedgerBookTestData.TestData1();
        }

        [TestMethod]
        public void UsingTestData1_LocateApplicableLedgerBalance_ShouldReturn64()
        {
            var filter = new GlobalFilterCriteria { BeginDate = new DateTime(2013, 04, 15), EndDate = new DateTime(2013, 05, 15) };
            decimal result = Subject.LocateApplicableLedgerBalance(TestData, filter, StatementModelTestData.PhoneBucket.Code);
            Assert.AreEqual(0M, result);
        }

        [TestMethod]
        public void UsingTestData1WithAugust15_LocateApplicableLedgerBalance_ShouldReturn64()
        {
            var filter = new GlobalFilterCriteria { BeginDate = new DateTime(2013, 07, 15), EndDate = new DateTime(2013, 08, 15) };

            decimal result = Subject.LocateApplicableLedgerBalance(TestData, filter, StatementModelTestData.PhoneBucket.Code);
            TestData.Output();
            Assert.AreEqual(64.71M, result);
        }
    }
}