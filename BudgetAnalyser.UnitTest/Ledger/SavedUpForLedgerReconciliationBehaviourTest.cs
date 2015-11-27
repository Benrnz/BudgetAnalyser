using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.UnitTest.Helper;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Ledger
{
    [TestClass]
    public class SavedUpForLedgerReconciliationBehaviourTest
    {
        private const decimal OpeningBalance = 100M;
        private LedgerEntry subject;

        [TestMethod]
        [Description("")]
        public void ShouldAddCompensatingTransaction_GivenClosingBalanceLessThanBudgetAmount()
        {
            //Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
            //var testInput = new List<LedgerTransaction>
            //{
            //    new CreditLedgerTransaction { Amount = 1, Date = new DateTime(2013, 9, 11) },
            //    new CreditLedgerTransaction { Amount = -1, Date = new DateTime(2013, 9, 11) }
            //};
            //this.subject.SetTransactionsForReconciliation(testInput, this.reconciliationDate);
            //this.subject.Output();

            Assert.AreEqual(3, this.subject.Transactions.Count());
        }

        [TestMethod]
        [Description("")]
        public void ShouldNotAddCompensatingTransaction_GivenNoBudgetAmountAndClosingBalanceGreaterThanZero()
        {
            //Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
            //var testInput = new List<LedgerTransaction>
            //{
            //    new CreditLedgerTransaction { Amount = 1, Date = new DateTime(2013, 9, 11) },
            //    new CreditLedgerTransaction { Amount = -1, Date = new DateTime(2013, 9, 11) }
            //};
            //this.subject.SetTransactionsForReconciliation(testInput, this.reconciliationDate);
            //this.subject.Output();

            Assert.AreEqual(3, this.subject.Transactions.Count());
        }

        [TestMethod]
        [Description("")]
        public void ShouldNotAddCompensatingTransaction_GivenClosingBalanceGreaterThanBudgetAmount()
        {
            //Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
            //var testInput = new List<LedgerTransaction>
            //{
            //    new CreditLedgerTransaction { Amount = 1, Date = new DateTime(2013, 9, 11) },
            //    new CreditLedgerTransaction { Amount = -1, Date = new DateTime(2013, 9, 11) }
            //};
            //this.subject.SetTransactionsForReconciliation(testInput, this.reconciliationDate);
            //this.subject.Output();

            Assert.AreEqual(3, this.subject.Transactions.Count());
        }

        [TestMethod]
        [Description("")]
        public void ShouldAddCompensatingTransaction_GivenNoBudgetAmountAndClosingBalanceLessThanZero()
        {
            //Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
            //var testInput = new List<LedgerTransaction>
            //{
            //    new CreditLedgerTransaction { Amount = 1, Date = new DateTime(2013, 9, 11) },
            //    new CreditLedgerTransaction { Amount = -1, Date = new DateTime(2013, 9, 11) }
            //};
            //this.subject.SetTransactionsForReconciliation(testInput, this.reconciliationDate);
            //this.subject.Output();

            Assert.AreEqual(3, this.subject.Transactions.Count());
        }

        [TestMethod]
        [Description("")]
        public void ShouldSupplementOverdrawnBalance_GivenClosingBalanceLessThanBudgetAmount()
        {
            //Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
            //var testInput = new List<LedgerTransaction>
            //{
            //    new CreditLedgerTransaction { Amount = 1, Date = new DateTime(2013, 9, 11) },
            //    new CreditLedgerTransaction { Amount = -1, Date = new DateTime(2013, 9, 11) }
            //};
            //this.subject.SetTransactionsForReconciliation(testInput, this.reconciliationDate);
            //this.subject.Output();

            Assert.AreEqual(3, this.subject.Balance);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            this.subject = new LedgerEntry(true)
            {
                LedgerBucket = LedgerBookTestData.PowerLedger,
                Balance = OpeningBalance
            };
        }
    }
}