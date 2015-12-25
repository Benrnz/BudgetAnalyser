using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.UnitTest.Helper;
using BudgetAnalyser.Engine.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Ledger
{
    [TestClass]
    public class SavedUpForLedgerReconciliationBehaviourTest
    {
        private const decimal OpeningBalance = 100M;
        private LedgerEntry subject;
        private readonly DateTime reconciliationDate = new DateTime(2013, 9, 20);

        [TestMethod]
        public void ShouldAddCompensatingTransaction_GivenClosingBalanceLessThanBudgetAmount()
        {
            Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
            var testInput = new List<LedgerTransaction>
            {
                new BudgetCreditLedgerTransaction { Amount = 150, Date = new DateTime(2013, 9, 11), Narrative = "Budget Amount" },
                new CreditLedgerTransaction { Amount = -200, Date = new DateTime(2013, 9, 11) }
            };
            this.subject.SetTransactionsForReconciliation(testInput, this.reconciliationDate);
            this.subject.Output();

            Assert.AreEqual(3, this.subject.Transactions.Count());
        }

        [TestMethod]
        public void ShouldNotAddCompensatingTransaction_GivenNoBudgetAmountAndClosingBalanceGreaterThanZero()
        {
            Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
            var testInput = new List<LedgerTransaction>
            {
                new CreditLedgerTransaction { Amount = -20, Date = new DateTime(2013, 9, 11) }
            };
            this.subject.SetTransactionsForReconciliation(testInput, this.reconciliationDate);
            this.subject.Output();

            Assert.AreEqual(1, this.subject.Transactions.Count());
        }

        [TestMethod]
        public void ShouldNotAddCompensatingTransaction_GivenClosingBalanceGreaterThanBudgetAmount()
        {
            Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
            var testInput = new List<LedgerTransaction>
            {
                new BudgetCreditLedgerTransaction { Amount = 150, Date = new DateTime(2013, 9, 11), Narrative = "Budget Amount" },
                new CreditLedgerTransaction { Amount = -20, Date = new DateTime(2013, 9, 11) }
            };
            this.subject.SetTransactionsForReconciliation(testInput, this.reconciliationDate);
            this.subject.Output();

            Assert.AreEqual(2, this.subject.Transactions.Count());
        }

        [TestMethod]
        public void ShouldAddCompensatingTransaction_GivenNoBudgetAmountAndClosingBalanceLessThanZero()
        {
            Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
            var testInput = new List<LedgerTransaction>
            {
                new CreditLedgerTransaction { Amount = -200, Date = new DateTime(2013, 9, 11) }
            };
            this.subject.SetTransactionsForReconciliation(testInput, this.reconciliationDate);
            this.subject.Output();

            Assert.AreEqual(2, this.subject.Transactions.Count());
        }

        [TestMethod]
        public void ShouldSupplementOverdrawnBalance_GivenClosingBalanceLessThanBudgetAmount()
        {
            Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
            var testInput = new List<LedgerTransaction>
            {
                new BudgetCreditLedgerTransaction { Amount = 150, Date = new DateTime(2013, 9, 11), Narrative = "Budget Amount" },
                new CreditLedgerTransaction { Amount = -200, Date = new DateTime(2013, 9, 11) }
            };
            this.subject.SetTransactionsForReconciliation(testInput, this.reconciliationDate);
            this.subject.Output();

            Assert.AreEqual(150, this.subject.Balance);
        }

        [TestMethod]
        public void ShouldNotSupplementPositiveBalance_GivenClosingBalanceGreaterThanBudgetAmount()
        {
            Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
            var testInput = new List<LedgerTransaction>
            {
                new BudgetCreditLedgerTransaction { Amount = 150, Date = new DateTime(2013, 9, 11), Narrative = "Budget Amount" },
                new CreditLedgerTransaction { Amount = -20, Date = new DateTime(2013, 9, 11) }
            };
            this.subject.SetTransactionsForReconciliation(testInput, this.reconciliationDate);
            this.subject.Output();

            Assert.AreEqual(230M, this.subject.Balance);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            this.subject = new LedgerEntry(true)
            {
                LedgerBucket = LedgerBookTestData.CarInsLedger,
                Balance = OpeningBalance
            };
        }
    }
}