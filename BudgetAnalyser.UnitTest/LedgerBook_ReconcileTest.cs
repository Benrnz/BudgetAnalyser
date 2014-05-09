using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.UnitTest.Helper;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest
{
    // ReSharper disable InconsistentNaming
    [TestClass]
    public class LedgerBook_ReconcileTest
    {
        private const decimal NextReconcileBankBalance = 1850.5M;
        private static readonly DateTime NextReconcileDate = new DateTime(2013, 09, 15);

        [TestMethod]
        public void UsingTestData1_AddAdjustment_Output()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            BudgetModel budget = BudgetModelTestData.CreateTestData1();
            LedgerEntryLine result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget);
            result.BalanceAdjustment(101M, "foo dar far");

            book.Output();
        }

        [TestMethod]
        public void UsingTestData1_AddAdjustment_ShouldAddToAdjustmentCollection()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            BudgetModel budget = BudgetModelTestData.CreateTestData1();
            LedgerEntryLine result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget);
            result.BalanceAdjustment(101M, "foo dar far");

            Assert.AreEqual(1, result.BankBalanceAdjustments.Count());
        }

        [TestMethod]
        public void UsingTestData1_AddAdjustment_ShouldAffectLedgerBalance()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            BudgetModel budget = BudgetModelTestData.CreateTestData1();
            LedgerEntryLine result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget);
            result.BalanceAdjustment(-101M, "foo dar far");

            Assert.AreEqual(1749.50M, result.LedgerBalance);
        }

        [TestMethod]
        public void UsingTestData1_AddLedger_ShouldAddToLedgersCollection()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            book.AddLedger(new SavedUpForExpenseBucket("FOO", "Foo bar"));

            Assert.IsTrue(book.Ledgers.Any(l => l.BudgetBucket.Code == "FOO"));
        }

        [TestMethod]
        public void UsingTestData1_AddLedger_ShouldBeIncludedInNextReconcile()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            book.AddLedger(new SavedUpForExpenseBucket("FOO", "Foo bar"));
            BudgetModel budget = BudgetModelTestData.CreateTestData1();
            LedgerEntryLine result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget);

            Assert.IsTrue(result.Entries.Any(e => e.Ledger.BudgetBucket.Code == "FOO"));
        }

        [TestMethod]
        public void UsingTestData1_AddTransactionShouldEffectEntryBalance()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            BudgetModel budget = BudgetModelTestData.CreateTestData1();
            StatementModel statement = StatementModelTestData.TestData1();
            LedgerEntryLine entryLine = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget, statement);
            var newTransaction = new DebitLedgerTransaction { Debit = 100 };
            LedgerEntry entry = entryLine.Entries.First();
            entry.AddTransaction(newTransaction);

            book.Output();
            Assert.AreEqual(20, entry.Balance);
        }

        [TestMethod]
        public void UsingTestData1_AddTransactionShouldEffectEntryNetAmount()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            BudgetModel budget = BudgetModelTestData.CreateTestData1();
            StatementModel statement = StatementModelTestData.TestData1();
            LedgerEntryLine entryLine = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget, statement);
            var newTransaction = new DebitLedgerTransaction { Debit = 100 };
            LedgerEntry entry = entryLine.Entries.First();
            entry.AddTransaction(newTransaction);

            Assert.AreEqual(-100, entry.NetAmount);
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_Output()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            BudgetModel budget = BudgetModelTestData.CreateTestData1();

            LedgerEntryLine result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget);

            book.Output();
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_ShouldInsertLastestInFront()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            BudgetModel budget = BudgetModelTestData.CreateTestData1();

            LedgerEntryLine result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget);

            Assert.AreEqual(result, book.DatedEntries.First());
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_ShouldResultIn1383()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            BudgetModel budget = BudgetModelTestData.CreateTestData1();

            LedgerEntryLine result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget);
            book.Output();
            Assert.AreEqual(1558.47M, result.CalculatedSurplus);
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_ShouldResultIn4Lines()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            BudgetModel budget = BudgetModelTestData.CreateTestData1();

            LedgerEntryLine result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget);

            Assert.AreEqual(4, book.DatedEntries.Count());
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_WithStatementOutput()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            BudgetModel budget = BudgetModelTestData.CreateTestData1();
            StatementModel statement = StatementModelTestData.TestData1();

            LedgerEntryLine result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget, statement);
            book.Output(true);
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_WithStatementSpentMonthlyLedgerShouldSupplementShortfall()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            BudgetModel budget = BudgetModelTestData.CreateTestData1();
            StatementModel statement = StatementModelTestData.TestData1();

            LedgerEntryLine result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget, statement);
            book.Output(true);

            Assert.AreEqual(64.71M, result.Entries.Single(e => e.Ledger.BudgetBucket.Code == TestDataConstants.PhoneBucketCode).Balance);
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_WithStatementSavedUpForLedgerShouldHave0Balance()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            BudgetModel budget = BudgetModelTestData.CreateTestData1();
            StatementModel statement = StatementModelTestData.TestData1();
            List<Transaction> additionalTransactions = statement.AllTransactions.ToList();
            additionalTransactions.Add(new Transaction
            {
                AccountType = additionalTransactions.First().AccountType,
                Amount = -264M,
                BudgetBucket = additionalTransactions.First(t => t.BudgetBucket.Code == TestDataConstants.HairBucketCode).BudgetBucket,
                Date = new DateTime(2013, 09, 13),
            });
            statement.LoadTransactions(additionalTransactions);

            LedgerEntryLine result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget, statement);
            book.Output(true);

            Assert.AreEqual(0, result.Entries.Single(e => e.Ledger.BudgetBucket.Code == TestDataConstants.HairBucketCode).Balance);
            Assert.IsTrue(result.Entries.Single(e => e.Ledger.BudgetBucket.Code == TestDataConstants.HairBucketCode).NetAmount < 0);
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_WithStatementShouldHave3PowerTransactions()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            BudgetModel budget = BudgetModelTestData.CreateTestData1();
            StatementModel statement = StatementModelTestData.TestData1();
            book.Output();
            LedgerEntryLine result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget, statement);
            Assert.AreEqual(3, result.Entries.Single(e => e.Ledger.BudgetBucket.Code == TestDataConstants.PowerBucketCode).Transactions.Count());
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_WithStatementShouldHave2HairTransactions()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            BudgetModel budget = BudgetModelTestData.CreateTestData1();
            StatementModel statement = StatementModelTestData.TestData1();
            book.Output();
            LedgerEntryLine result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget, statement);
            Assert.AreEqual(2, result.Entries.Single(e => e.Ledger.BudgetBucket.Code == TestDataConstants.HairBucketCode).Transactions.Count());
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_WithStatementShouldHaveSurplus1613()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            BudgetModel budget = BudgetModelTestData.CreateTestData1();
            StatementModel statement = StatementModelTestData.TestData1();
            LedgerEntryLine result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget, statement);
            book.Output(true);
            Assert.AreEqual(1613.47M, result.CalculatedSurplus);
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_WithStatementWithBalanceAdjustment599ShouldHaveSurplus1014()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            BudgetModel budget = BudgetModelTestData.CreateTestData1();
            StatementModel statement = StatementModelTestData.TestData1();

            LedgerEntryLine result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget, statement);
            result.BalanceAdjustment(-599M, "Visa pmt not yet in statement");
            book.Output(true);
            Assert.AreEqual(1014.47M, result.CalculatedSurplus);
        }

        [TestMethod]
        public void UsingTestData1_RemoveTransactionShouldEffectEntryBalance()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            BudgetModel budget = BudgetModelTestData.CreateTestData1();
            StatementModel statement = StatementModelTestData.TestData1();
            LedgerEntryLine entryLine = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget, statement);
            LedgerEntry entry = entryLine.Entries.First();
            entry.RemoveTransaction(entry.Transactions.First(t => t is DebitLedgerTransaction).Id);

            Assert.AreEqual(175M, entry.Balance);
        }

        [TestMethod]
        public void UsingTestData1_RemoveTransactionShouldEffectEntryNetAmount()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            BudgetModel budget = BudgetModelTestData.CreateTestData1();
            StatementModel statement = StatementModelTestData.TestData1();
            LedgerEntryLine entryLine = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget, statement);
            LedgerEntry entry = entryLine.Entries.First();
            entry.RemoveTransaction(entry.Transactions.First(t => t is DebitLedgerTransaction).Id);

            Assert.AreEqual(55M, entry.NetAmount);
        }

        [TestMethod]
        public void UsingTestData1_RemoveTransactionShouldGiveSurplus1558()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            BudgetModel budget = BudgetModelTestData.CreateTestData1();
            StatementModel statement = StatementModelTestData.TestData1();
            LedgerEntryLine entryLine = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget, statement);
            LedgerEntry entry = entryLine.Entries.First();
            entry.RemoveTransaction(entry.Transactions.First(t => t is DebitLedgerTransaction).Id);

            book.Output();
            Assert.AreEqual(1558.47M, entryLine.CalculatedSurplus);
        }

        [TestMethod]
        public void UsingTestData1_UpdateRemarks_ShouldSetRemarks()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            BudgetModel budget = BudgetModelTestData.CreateTestData1();

            LedgerEntryLine result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget);
            result.UpdateRemarks("Foo bar");

            Assert.AreEqual("Foo bar", result.Remarks);
        }
    }

    // ReSharper restore InconsistentNaming
}