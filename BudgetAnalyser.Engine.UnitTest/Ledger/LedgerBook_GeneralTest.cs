using System;
using System.Linq;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Reconciliation;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.UnitTest.Helper;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Ledger
{
    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class LedgerBook_GeneralTest
    {
        private static readonly BankBalance NextReconcileBankBalance = new BankBalance(StatementModelTestData.ChequeAccount, 1850.5M);
        private static readonly DateTime NextReconcileDate = new DateTime(2013, 09, 15);
        private LedgerBook subject;
        private BudgetModel testDataBudget;
        private StatementModel testDataStatement;

        [TestInitialize]
        public void TestInitialise()
        {
            this.testDataStatement = StatementModelTestData.TestData1();
            this.testDataBudget = BudgetModelTestData.CreateTestData1();
            this.subject = LedgerBookTestData.TestData1();
        }

        [TestMethod]
        public void UnlockMostRecentLineShouldNotThrowIfBookIsEmpty()
        {
            this.subject = new LedgerBook()
            {
                Name = "Foo",
                Modified = new DateTime(2011, 12, 4),
                StorageKey = @"C:\TestLedgerBook.xml"
            };
            var result = this.subject.UnlockMostRecentLine();

            Assert.IsNull(result);
        }

        [TestMethod]
        public void UnlockMostRecentLineShouldReturnMostRecentLine()
        {
            var result = this.subject.UnlockMostRecentLine();
            var expectedLine = this.subject.Reconciliations.OrderByDescending(e => e.Date).First();

            Assert.AreSame(expectedLine, result);
        }

        [TestMethod]
        public void UnlockMostRecentLineShouldUnlockMostRecentLine()
        {
            var result = this.subject.UnlockMostRecentLine();

            Assert.IsTrue(result.IsNew);
        }

        [TestMethod]
        public void UsingTestData1_AddAdjustment_Output()
        {
            var result = Act(this.subject, this.testDataBudget);
            result.Reconciliation.BalanceAdjustment(101M, "foo dar far", new ChequeAccount("Chq"));

            this.subject.Output();
        }

        [TestMethod]
        public void UsingTestData1_AddAdjustment_ShouldAddToAdjustmentCollection()
        {
            var result = Act(this.subject, this.testDataBudget);
            result.Reconciliation.BalanceAdjustment(101M, "foo dar far", new ChequeAccount("Chq"));

            Assert.AreEqual(1, result.Reconciliation.BankBalanceAdjustments.Count());
        }

        [TestMethod]
        public void UsingTestData1_AddAdjustment_ShouldAffectLedgerBalance()
        {
            var result = Act(this.subject, this.testDataBudget);
            result.Reconciliation.BalanceAdjustment(-101M, "foo dar far", new ChequeAccount("Chq"));

            Assert.AreEqual(1749.50M, result.Reconciliation.LedgerBalance);
        }

        [TestMethod]
        public void UsingTestData1_AddTransactionShouldEffectEntryBalance()
        {
            var entryLine = Act(this.subject, this.testDataBudget);
            var newTransaction = new CreditLedgerTransaction { Amount = -100 };
            var entry = entryLine.Reconciliation.Entries.First();
            entry.AddTransactionForPersistenceOnly(newTransaction);

            this.subject.Output();
            Assert.AreEqual(20, entry.Balance);
        }

        [TestMethod]
        public void UsingTestData1_AddTransactionShouldEffectEntryNetAmount()
        {
            var entryLine = Act(this.subject, this.testDataBudget);
            var newTransaction = new CreditLedgerTransaction { Amount = -100 };
            var entry = entryLine.Reconciliation.Entries.First();
            entry.AddTransactionForPersistenceOnly(newTransaction);

            Assert.AreEqual(-100, entry.NetAmount);
        }

        [TestMethod]
        public void UsingTestData1_RemoveTransactionShouldNOTEffectEntryBalance()
        {
            var entryLine = Act(this.subject, this.testDataBudget);
            var entry = entryLine.Reconciliation.Entries.First();
            entry.RemoveTransaction(entry.Transactions.First(t => t is CreditLedgerTransaction).Id);

            // The balance cannot be simply set inside the Ledger Line, it must be recalc'ed at the ledger book level.
            Assert.AreEqual(120M, entry.Balance);
        }

        [TestMethod]
        public void UsingTestData1_RemoveTransactionShouldEffectEntryNetAmount()
        {
            var entryLine = Act(this.subject, this.testDataBudget);
            var entry = entryLine.Reconciliation.Entries.First();
            entry.RemoveTransaction(entry.Transactions.First(t => t is CreditLedgerTransaction).Id);

            Assert.AreEqual(55M, entry.NetAmount);
        }

        [TestMethod]
        public void UsingTestData1_RemoveTransactionShouldGiveSurplus1555()
        {
            var entryLine = Act(this.subject, this.testDataBudget);
            var entry = entryLine.Reconciliation.Entries.First();
            entry.RemoveTransaction(entry.Transactions.First(t => t is CreditLedgerTransaction).Id);

            // The balance of a ledger cannot simply be calculated inside the ledger line. It must be recalc'ed at the ledger book level.
            Assert.AreEqual(1555.50M, entryLine.Reconciliation.CalculatedSurplus);
        }

        [TestMethod]
        public void UsingTestData1_UpdateRemarks_ShouldSetRemarks()
        {
            var result = Act(this.subject, this.testDataBudget);
            result.Reconciliation.UpdateRemarks("Foo bar");

            Assert.AreEqual("Foo bar", result.Reconciliation.Remarks);
        }

        [TestMethod]
        public void UsingTestData2_Output()
        {
            var book = LedgerBookTestData.TestData2();
            book.Output();
        }

        private void Act(LedgerBook book, ReconciliationResult recon)
        {
            book.Reconcile(recon);
        }
    }
}