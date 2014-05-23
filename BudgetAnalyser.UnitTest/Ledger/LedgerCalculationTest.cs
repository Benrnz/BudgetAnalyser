using System;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.UnitTest.Helper;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Ledger
{
    [TestClass]
    public class LedgerCalculationTest
    {
        [TestMethod]
        public void UsingTestData1WithAugust15_LocateApplicableLedgerBalance_ShouldReturn64()
        {
            var ledgerBook = LedgerBookTestData.TestData1();
            var filter = new GlobalFilterCriteria { BeginDate = new DateTime(2013, 07, 15), EndDate = new DateTime(2013, 08, 15) };

            var result = LedgerCalculation.LocateApplicableLedgerBalance(ledgerBook, filter, StatementModelTestData.PhoneBucket.Code);
            ledgerBook.Output();
            Assert.AreEqual(64.71M, result);
        }

        [TestMethod]
        public void UsingTestData1_LocateApplicableLedgerBalance_ShouldReturn64()
        {
            var ledgerBook = LedgerBookTestData.TestData1();
            var filter = new GlobalFilterCriteria { BeginDate = new DateTime(2013, 04, 15), EndDate = new DateTime(2013, 05, 15) };

            var result = LedgerCalculation.LocateApplicableLedgerBalance(ledgerBook, filter, StatementModelTestData.PhoneBucket.Code);
            Assert.AreEqual(0M, result);
        }
    }
}
