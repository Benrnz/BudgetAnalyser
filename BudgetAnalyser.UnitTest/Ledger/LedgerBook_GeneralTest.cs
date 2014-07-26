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

        [TestMethod]
        public void UnlockMostRecentLineShouldNotThrowIfBookIsEmpty()
        {
            var subject = new LedgerBook(new FakeLogger())
            {
                Name = "Foo", Modified = new DateTime(2011, 12, 4), FileName = @"C:\TestLedgerBook.xml",
            };
            LedgerEntryLine result = subject.UnlockMostRecentLine();

            Assert.IsNull(result);
        }

        [TestMethod]
        public void UnlockMostRecentLineShouldReturnMostRecentLine()
        {
            LedgerBook subject = ArrangeAndAct();
            LedgerEntryLine result = subject.UnlockMostRecentLine();
            LedgerEntryLine expectedLine = subject.DatedEntries.OrderByDescending(e => e.Date).First();

            Assert.AreSame(expectedLine, result);
        }

        [TestMethod]
        public void UnlockMostRecentLineShouldUnlockMostRecentLine()
        {
            LedgerBook subject = ArrangeAndAct();
            LedgerEntryLine result = subject.UnlockMostRecentLine();

            Assert.IsTrue(result.IsNew);
        }

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

        [TestMethod]
        public void UsingTestData2_Output()
        {
            LedgerBook book = LedgerBookTestData.TestData2();
            book.Output();
        }


        private LedgerBook ArrangeAndAct()
        {
            return LedgerBookTestData.TestData1();
        }
    }
}