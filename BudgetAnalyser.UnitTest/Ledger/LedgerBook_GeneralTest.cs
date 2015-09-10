using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.UnitTest.Helper;
using BudgetAnalyser.UnitTest.TestData;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Ledger
{
    [TestClass]
    public class LedgerBook_GeneralTest
    {
        private static readonly IEnumerable<BankBalance> NextReconcileBankBalance = new[] { new BankBalance(StatementModelTestData.ChequeAccount, 1850.5M) };
        private static readonly DateTime NextReconcileDate = new DateTime(2013, 09, 15);
        private LedgerBook subject;
        private BudgetModel testDataBudget;
        private StatementModel testDataStatement;
        private ToDoCollection testDataToDoList;

        [TestInitialize]
        public void TestInitialise()
        {
            this.testDataStatement = StatementModelTestData.TestData1();
            this.testDataToDoList = new ToDoCollection();
            this.testDataBudget = BudgetModelTestData.CreateTestData1();
            this.subject = LedgerBookTestData.TestData1();
        }

        [TestMethod]
        public void UnlockMostRecentLineShouldNotThrowIfBookIsEmpty()
        {
            this.subject = new LedgerBook(new ReconciliationBuilder(new FakeLogger()))
            {
                Name = "Foo",
                Modified = new DateTime(2011, 12, 4),
                StorageKey = @"C:\TestLedgerBook.xml"
            };
            LedgerEntryLine result = this.subject.UnlockMostRecentLine();

            Assert.IsNull(result);
        }

        [TestMethod]
        public void UnlockMostRecentLineShouldReturnMostRecentLine()
        {
            LedgerEntryLine result = this.subject.UnlockMostRecentLine();
            LedgerEntryLine expectedLine = this.subject.Reconciliations.OrderByDescending(e => e.Date).First();

            Assert.AreSame(expectedLine, result);
        }

        [TestMethod]
        public void UnlockMostRecentLineShouldUnlockMostRecentLine()
        {
            LedgerEntryLine result = this.subject.UnlockMostRecentLine();

            Assert.IsTrue(result.IsNew);
        }

        [TestMethod]
        public void UsingTestData1_AddAdjustment_Output()
        {
            ReconciliationResult result = Act(this.subject, this.testDataBudget);
            result.Reconciliation.BalanceAdjustment(101M, "foo dar far");

            this.subject.Output();
        }

        [TestMethod]
        public void UsingTestData1_AddAdjustment_ShouldAddToAdjustmentCollection()
        {
            ReconciliationResult result = Act(this.subject, this.testDataBudget);
            result.Reconciliation.BalanceAdjustment(101M, "foo dar far");

            Assert.AreEqual(1, result.Reconciliation.BankBalanceAdjustments.Count());
        }

        [TestMethod]
        public void UsingTestData1_AddAdjustment_ShouldAffectLedgerBalance()
        {
            ReconciliationResult result = Act(this.subject, this.testDataBudget);
            result.Reconciliation.BalanceAdjustment(-101M, "foo dar far");

            Assert.AreEqual(1749.50M, result.Reconciliation.LedgerBalance);
        }

        [TestMethod]
        public void UsingTestData1_AddTransactionShouldEffectEntryBalance()
        {
            ReconciliationResult entryLine = Act(this.subject, this.testDataBudget);
            var newTransaction = new CreditLedgerTransaction { Amount = -100 };
            LedgerEntry entry = entryLine.Reconciliation.Entries.First();
            entry.AddTransaction(newTransaction);

            this.subject.Output();
            Assert.AreEqual(20, entry.Balance);
        }

        [TestMethod]
        public void UsingTestData1_AddTransactionShouldEffectEntryNetAmount()
        {
            ReconciliationResult entryLine = Act(this.subject, this.testDataBudget);
            var newTransaction = new CreditLedgerTransaction { Amount = -100 };
            LedgerEntry entry = entryLine.Reconciliation.Entries.First();
            entry.AddTransaction(newTransaction);

            Assert.AreEqual(-100, entry.NetAmount);
        }

        [TestMethod]
        public void UsingTestData1_RemoveTransactionShouldEffectEntryBalance()
        {
            ReconciliationResult entryLine = Act(this.subject, this.testDataBudget);
            LedgerEntry entry = entryLine.Reconciliation.Entries.First();
            entry.RemoveTransaction(entry.Transactions.First(t => t is CreditLedgerTransaction).Id);

            Assert.AreEqual(175M, entry.Balance);
        }

        [TestMethod]
        public void UsingTestData1_RemoveTransactionShouldEffectEntryNetAmount()
        {
            ReconciliationResult entryLine = Act(this.subject, this.testDataBudget);
            LedgerEntry entry = entryLine.Reconciliation.Entries.First();
            entry.RemoveTransaction(entry.Transactions.First(t => t is CreditLedgerTransaction).Id);

            Assert.AreEqual(55M, entry.NetAmount);
        }

        [TestMethod]
        public void UsingTestData1_RemoveTransactionShouldGiveSurplus1558()
        {
            ReconciliationResult entryLine = Act(this.subject, this.testDataBudget);
            LedgerEntry entry = entryLine.Reconciliation.Entries.First();
            entry.RemoveTransaction(entry.Transactions.First(t => t is CreditLedgerTransaction).Id);

            this.subject.Output();
            Assert.AreEqual(1558.47M, entryLine.Reconciliation.CalculatedSurplus);
        }

        [TestMethod]
        public void UsingTestData1_UpdateRemarks_ShouldSetRemarks()
        {
            ReconciliationResult result = Act(this.subject, this.testDataBudget);
            result.Reconciliation.UpdateRemarks("Foo bar");

            Assert.AreEqual("Foo bar", result.Reconciliation.Remarks);
        }

        [TestMethod]
        public void UsingTestData2_Output()
        {
            LedgerBook book = LedgerBookTestData.TestData2();
            book.Output();
        }

        private ReconciliationResult Act(LedgerBook book, BudgetModel budget)
        {
            return book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget, this.testDataStatement);
        }
    }
}