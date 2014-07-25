using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.UnitTest.Helper;
using BudgetAnalyser.UnitTest.TestData;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rees.TestUtilities;

namespace BudgetAnalyser.UnitTest.Ledger
{
    // ReSharper disable InconsistentNaming
    [TestClass]
    public class LedgerBook_ReconcileTest
    {
        private static readonly IEnumerable<BankBalance> NextReconcileBankBalance = new[] { new BankBalance(StatementModelTestData.ChequeAccount, 1850.5M) };
        private static readonly DateTime NextReconcileDate = new DateTime(2013, 09, 15);

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UsingInvalidLedgerBook_Reconcile_ShouldThrow()
        {
            var subject = new LedgerBook("Foo", new DateTime(2012, 02, 29), "", new FakeLogger());

            subject.Reconcile(
                new DateTime(2012, 02, 20),
                new[] { new BankBalance(StatementModelTestData.ChequeAccount, 2050M) },
                BudgetModelTestData.CreateTestData1(),
                StatementModelTestData.TestData1());
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationWarningException))]
        public void UsingTestData1AndNoStatementModelTransactions_Reconcile_ShouldThrow()
        {
            LedgerBook subject = LedgerBookTestData.TestData1();

            subject.Reconcile(
                new DateTime(2013, 10, 15),
                new[] { new BankBalance(StatementModelTestData.ChequeAccount, 2050M) },
                BudgetModelTestData.CreateTestData1(),
                StatementModelTestData.TestData1());
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationWarningException))]
        public void UsingTestData1AndUnclassifiedTransactions_Reconcile_ShouldThrow()
        {
            LedgerBook subject = LedgerBookTestData.TestData1();
            StatementModel statement = StatementModelTestData.TestData1();
            Transaction aTransaction = statement.AllTransactions.First();
            PrivateAccessor.SetField(aTransaction, "budgetBucket", null);

            subject.Reconcile(
                new DateTime(2013, 9, 15),
                new[] { new BankBalance(StatementModelTestData.ChequeAccount, 2050M) },
                BudgetModelTestData.CreateTestData1(),
                statement);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UsingTestData1WithDateEqualToExistingLine_Reconcile_ShouldThrow()
        {
            LedgerBook subject = LedgerBookTestData.TestData1();

            subject.Reconcile(
                new DateTime(2013, 08, 15),
                new[] { new BankBalance(StatementModelTestData.ChequeAccount, 2050M) },
                BudgetModelTestData.CreateTestData1(),
                StatementModelTestData.TestData1());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UsingTestData1WithDateLessThanExistingLine_Reconcile_ShouldThrow()
        {
            LedgerBook subject = LedgerBookTestData.TestData1();

            subject.Reconcile(
                new DateTime(2013, 07, 15),
                new[] { new BankBalance(StatementModelTestData.ChequeAccount, 2050M) },
                BudgetModelTestData.CreateTestData1(),
                StatementModelTestData.TestData1());
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

            Assert.IsTrue(result.Entries.Any(e => e.LedgerColumn.BudgetBucket.Code == "FOO"));
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

            Assert.AreEqual(0, result.Entries.Single(e => e.LedgerColumn.BudgetBucket.Code == TestDataConstants.HairBucketCode).Balance);
            Assert.IsTrue(result.Entries.Single(e => e.LedgerColumn.BudgetBucket.Code == TestDataConstants.HairBucketCode).NetAmount < 0);
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_WithStatementShouldHave2HairTransactions()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            BudgetModel budget = BudgetModelTestData.CreateTestData1();
            StatementModel statement = StatementModelTestData.TestData1();
            book.Output();
            LedgerEntryLine result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget, statement);
            Assert.AreEqual(2, result.Entries.Single(e => e.LedgerColumn.BudgetBucket.Code == TestDataConstants.HairBucketCode).Transactions.Count());
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_WithStatementShouldHave3PowerTransactions()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            BudgetModel budget = BudgetModelTestData.CreateTestData1();
            StatementModel statement = StatementModelTestData.TestData1();
            book.Output();
            LedgerEntryLine result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget, statement);
            Assert.AreEqual(3, result.Entries.Single(e => e.LedgerColumn.BudgetBucket.Code == TestDataConstants.PowerBucketCode).Transactions.Count());
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
        public void UsingTestData1_Reconcile_WithStatementSpentMonthlyLedgerShouldSupplementShortfall()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            BudgetModel budget = BudgetModelTestData.CreateTestData1();
            StatementModel statement = StatementModelTestData.TestData1();

            LedgerEntryLine result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget, statement);
            book.Output(true);

            Assert.AreEqual(64.71M, result.Entries.Single(e => e.LedgerColumn.BudgetBucket.Code == TestDataConstants.PhoneBucketCode).Balance);
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
    }

    // ReSharper restore InconsistentNaming
}