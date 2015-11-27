using System;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Ledger
{
    [TestClass]
    public class SavedUpForLedgerReconciliationBehaviourTest
    {
        private const decimal OpeningBalance = 125M;
        private DateTime reconciliationDate;
        private LedgerEntry subject;

        [TestMethod]
        public void Test1()
        {
            
        }

        [TestInitialize]
        public void TestInitialise()
        {
            this.reconciliationDate = new DateTime(2013, 9, 20);

            this.subject = new LedgerEntry(true)
            {
                LedgerBucket = LedgerBookTestData.PowerLedger,
                Balance = OpeningBalance
            };
        }
    }
}