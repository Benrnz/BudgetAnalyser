using System;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.UnitTest.Helper;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest
{
    // ReSharper disable InconsistentNaming
    [TestClass]
    public class LedgerBook_ReconcileTest
    {
        private static readonly DateTime NextReconcileDate = new DateTime(2013, 09, 15);
        private const decimal NextReconcileBankBalance = 1850.5M;

        [TestMethod]
        public void UsingTestData1_AddTransactionShouldEffectEntryBalance()
        {
            var book = TestData.LedgerBookTestData.TestData1();
            var budget = TestData.BudgetModelTestData.CreateTestData1();
            var statement = TestData.StatementModelTestData.TestData1();
            var entryLine = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget, statement);
            var newTransaction = new DebitLedgerTransaction { Debit = 100 };
            var entry = entryLine.Entries.First();
            entry.AddTransaction(newTransaction);

            book.Output();
            Assert.AreEqual(20, entry.Balance);
        }

        [TestMethod]
        public void UsingTestData1_AddTransactionShouldEffectEntryNetAmount()
        {
            var book = TestData.LedgerBookTestData.TestData1();
            var budget = TestData.BudgetModelTestData.CreateTestData1();
            var statement = TestData.StatementModelTestData.TestData1();
            var entryLine = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget, statement);
            var newTransaction = new DebitLedgerTransaction { Debit = 100 };
            var entry = entryLine.Entries.First();
            entry.AddTransaction(newTransaction);

            Assert.AreEqual(-100, entry.NetAmount);
        }

        [TestMethod]
        public void UsingTestData1_RemoveTransactionShouldEffectEntryNetAmount()
        {
            var book = TestData.LedgerBookTestData.TestData1();
            var budget = TestData.BudgetModelTestData.CreateTestData1();
            var statement = TestData.StatementModelTestData.TestData1();
            var entryLine = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget, statement);
            var entry = entryLine.Entries.First();
            entry.RemoveTransaction(entry.Transactions.First(t => t is DebitLedgerTransaction).Id);

            Assert.AreEqual(55M, entry.NetAmount);
        }

        [TestMethod]
        public void UsingTestData1_RemoveTransactionShouldEffectEntryBalance()
        {
            var book = TestData.LedgerBookTestData.TestData1();
            var budget = TestData.BudgetModelTestData.CreateTestData1();
            var statement = TestData.StatementModelTestData.TestData1();
            var entryLine = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget, statement);
            var entry = entryLine.Entries.First();
            entry.RemoveTransaction(entry.Transactions.First(t => t is DebitLedgerTransaction).Id);

            Assert.AreEqual(175M, entry.Balance);
        }

        [TestMethod]
        public void UsingTestData1_RemoveTransactionShouldGiveSurplus1537()
        {
            var book = TestData.LedgerBookTestData.TestData1();
            var budget = TestData.BudgetModelTestData.CreateTestData1();
            var statement = TestData.StatementModelTestData.TestData1();
            var entryLine = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget, statement);
            var entry = entryLine.Entries.First();
            entry.RemoveTransaction(entry.Transactions.First(t => t is DebitLedgerTransaction).Id);

            Assert.AreEqual(1537.33M, entryLine.CalculatedSurplus);
        }
        
        [TestMethod]
        public void UsingTestData1_Reconcile_WithStatementOutput()
        {
            var book = TestData.LedgerBookTestData.TestData1();
            var budget = TestData.BudgetModelTestData.CreateTestData1();
            var statement = TestData.StatementModelTestData.TestData1();

            var result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget, statement);
            book.Output(true);
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_WithStatementWithBalanceAdjustment599ShouldHaveSurplus993()
        {
            var book = TestData.LedgerBookTestData.TestData1();
            var budget = TestData.BudgetModelTestData.CreateTestData1();
            var statement = TestData.StatementModelTestData.TestData1();

            var result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget, statement);
            result.BalanceAdjustment(-599M, "Visa pmt not yet in statement");
            Assert.AreEqual(993.33M, result.CalculatedSurplus);
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_WithStatementShouldHaveSurplus1592()
        {
            var book = TestData.LedgerBookTestData.TestData1();
            var budget = TestData.BudgetModelTestData.CreateTestData1();
            var statement = TestData.StatementModelTestData.TestData1();

            var result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget, statement);
            Assert.AreEqual(1592.33M, result.CalculatedSurplus);
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_WithStatementShouldHave2PowerTransactions()
        {
            var book = TestData.LedgerBookTestData.TestData1();
            var budget = TestData.BudgetModelTestData.CreateTestData1();
            var statement = TestData.StatementModelTestData.TestData1();

            var result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget, statement);
            Assert.AreEqual(2, result.Entries.Single(e => e.Ledger.BudgetBucket.Code == TestDataConstants.PowerBucketCode).Transactions.Count());
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_WithStatementShouldHave0PhoneBalance()
        {
            var book = TestData.LedgerBookTestData.TestData1();
            var budget = TestData.BudgetModelTestData.CreateTestData1();
            var statement = TestData.StatementModelTestData.TestData1();

            var result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget, statement);
            Assert.AreEqual(0, result.Entries.Single(e => e.Ledger.BudgetBucket.Code == TestDataConstants.PhoneBucketCode).Balance);
            Assert.IsTrue(result.Entries.Single(e => e.Ledger.BudgetBucket.Code == TestDataConstants.PhoneBucketCode).NetAmount < 0);
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_Output()
        {
            var book = TestData.LedgerBookTestData.TestData1();
            var budget = TestData.BudgetModelTestData.CreateTestData1();

            var result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget);

            book.Output();
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_ShouldResultIn1383()
        {
            var book = TestData.LedgerBookTestData.TestData1();
            var budget = TestData.BudgetModelTestData.CreateTestData1();

            var result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget);

            Assert.AreEqual(1383.47M, result.CalculatedSurplus);
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_ShouldResultIn4Lines()
        {
            var book = TestData.LedgerBookTestData.TestData1();
            var budget = TestData.BudgetModelTestData.CreateTestData1();

            var result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget);

            Assert.AreEqual(4, book.DatedEntries.Count());
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_ShouldInsertLastestInFront()
        {
            var book = TestData.LedgerBookTestData.TestData1();
            var budget = TestData.BudgetModelTestData.CreateTestData1();

            var result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget);

            Assert.AreEqual(result, book.DatedEntries.First());
        }

        [TestMethod]
        public void UsingTestData1_UpdateRemarks_ShouldSetRemarks()
        {
            var book = TestData.LedgerBookTestData.TestData1();
            var budget = TestData.BudgetModelTestData.CreateTestData1();

            var result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget);
            result.UpdateRemarks("Foo bar");

            Assert.AreEqual("Foo bar", result.Remarks);
        }

        [TestMethod]
        public void UsingTestData1_AddLedger_ShouldAddToLedgersCollection()
        {
            var book = TestData.LedgerBookTestData.TestData1();
            book.AddLedger(new SavedUpForExpense("FOO", "Foo bar"));

            Assert.IsTrue(book.Ledgers.Any(l => l.BudgetBucket.Code == "FOO"));
        }

        [TestMethod]
        public void UsingTestData1_AddLedger_ShouldBeIncludedInNextReconcile()
        {
            var book = TestData.LedgerBookTestData.TestData1();
            book.AddLedger(new SavedUpForExpense("FOO", "Foo bar"));
            var budget = TestData.BudgetModelTestData.CreateTestData1();
            var result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget);

            Assert.IsTrue(result.Entries.Any(e => e.Ledger.BudgetBucket.Code == "FOO"));
        }

        [TestMethod]
        public void UsingTestData1_AddAdjustment_ShouldAddToAdjustmentCollection()
        {
            var book = TestData.LedgerBookTestData.TestData1();
            var budget = TestData.BudgetModelTestData.CreateTestData1();
            var result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget);
            result.BalanceAdjustment(101M, "foo dar far");

            Assert.AreEqual(1, result.BankBalanceAdjustments.Count());
        }

        [TestMethod]
        public void UsingTestData1_AddAdjustment_ShouldAffectLedgerBalance()
        {
            var book = TestData.LedgerBookTestData.TestData1();
            var budget = TestData.BudgetModelTestData.CreateTestData1();
            var result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget);
            result.BalanceAdjustment(-101M, "foo dar far");

            Assert.AreEqual(1749.50M, result.LedgerBalance);
        }

        [TestMethod]
        public void UsingTestData1_AddAdjustment_Output()
        {
            var book = TestData.LedgerBookTestData.TestData1();
            var budget = TestData.BudgetModelTestData.CreateTestData1();
            var result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget);
            result.BalanceAdjustment(101M, "foo dar far");

            book.Output();
        }
    }

    // ReSharper restore InconsistentNaming
}