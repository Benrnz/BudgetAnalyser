using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
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
        private ToDoCollection toDoCollection = new ToDoCollection();

        [TestMethod]
        public void DuplicateReferenceNumberTest()
        {
            // ReSharper disable once CollectionNeverQueried.Local
            var duplicateCheck = new Dictionary<string, string>();
            for (var i = 0; i < 1000; i++)
            {
                var result = PrivateAccessor.InvokeStaticFunction<string>(typeof(ReconciliationBuilder), "IssueTransactionReferenceNumber");
                Console.WriteLine(result);
                Assert.IsNotNull(result);
                duplicateCheck.Add(result, result);
            }
        }

        [TestMethod]
        public void OutputTestData5()
        {
            LedgerBook testData = LedgerBookTestData.TestData5();
            testData.Output(true);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UsingInvalidLedgerBook_Reconcile_ShouldThrow()
        {
            var subject = new LedgerBook(new FakeLogger(), new ReconciliationBuilder(new FakeLogger()))
            {
                Name = "Foo",
                Modified = new DateTime(2012, 02, 29),
                FileName = ""
            };

            subject.Reconcile(
                new DateTime(2012, 02, 20),
                new[] { new BankBalance(StatementModelTestData.ChequeAccount, 2050M) },
                BudgetModelTestData.CreateTestData1(),
                statement: StatementModelTestData.TestData1());
        }

        [TestMethod]
        public void UsingTestData1_AddLedger_ShouldAddToLedgersCollection()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            book.AddLedger(new SavedUpForExpenseBucket("FOO", "Foo bar"), null);

            Assert.IsTrue(book.Ledgers.Any(l => l.BudgetBucket.Code == "FOO"));
        }

        [TestMethod]
        public void UsingTestData1_AddLedger_ShouldBeIncludedInNextReconcile()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            book.AddLedger(new SavedUpForExpenseBucket("FOO", "Foo bar"), null);
            BudgetModel budget = BudgetModelTestData.CreateTestData1();
            LedgerEntryLine result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget);

            Assert.IsTrue(result.Entries.Any(e => e.LedgerBucket.BudgetBucket.Code == "FOO"));
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

            Assert.AreEqual(result, book.Reconciliations.First());
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_ShouldResultIn1558()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            BudgetModel budget = BudgetModelTestData.CreateTestData1();

            LedgerEntryLine result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget);
            book.Output(true);
            Assert.AreEqual(1558.47M, result.CalculatedSurplus);
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_ShouldResultIn4Lines()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            BudgetModel budget = BudgetModelTestData.CreateTestData1();

            LedgerEntryLine result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget);

            Assert.AreEqual(4, book.Reconciliations.Count());
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_WithStatementOutput()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            BudgetModel budget = BudgetModelTestData.CreateTestData1();
            StatementModel statement = StatementModelTestData.TestData1();

            LedgerEntryLine result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget, statement: statement);
            book.Output(true);
        }

        [TestMethod]
        [Description("This test overdraws the Hair ledger and tests to make sure the reconciliation process compensates and leaves it with a balance equal to the monthly payment amount.")]
        public void UsingTestData1_Reconcile_WithStatementSavedUpForHairLedgerShouldHaveBalance55()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            BudgetModel budget = BudgetModelTestData.CreateTestData1();
            StatementModel statement = StatementModelTestData.TestData1();
            List<Transaction> additionalTransactions = statement.AllTransactions.ToList();

            additionalTransactions.Add(
                new Transaction
                {
                    Account = additionalTransactions.First().Account,
                    Amount = -264M,
                    BudgetBucket = additionalTransactions.First(t => t.BudgetBucket.Code == TestDataConstants.HairBucketCode).BudgetBucket,
                    Date = new DateTime(2013, 09, 13)
                });
            statement.LoadTransactions(additionalTransactions);

            LedgerEntryLine result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget, statement: statement);
            book.Output(true);

            Assert.AreEqual(55M, result.Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode).Balance);
            Assert.IsTrue(result.Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode).NetAmount < 0);
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_WithStatementShouldHave2HairTransactions()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            BudgetModel budget = BudgetModelTestData.CreateTestData1();
            StatementModel statement = StatementModelTestData.TestData1();
            book.Output();
            LedgerEntryLine result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget, statement: statement);
            Assert.AreEqual(2, result.Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode).Transactions.Count());
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_WithStatementShouldHave3PowerTransactions()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            BudgetModel budget = BudgetModelTestData.CreateTestData1();
            StatementModel statement = StatementModelTestData.TestData1();
            book.Output();
            LedgerEntryLine result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget, statement: statement);
            book.Output(true);
            Assert.AreEqual(3, result.Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PowerBucketCode).Transactions.Count());
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_WithStatementShouldHaveSurplus1613()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            BudgetModel budget = BudgetModelTestData.CreateTestData1();
            StatementModel statement = StatementModelTestData.TestData1();
            LedgerEntryLine result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget, statement: statement);
            book.Output(true);
            Assert.AreEqual(1613.47M, result.CalculatedSurplus);
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_WithStatementSpentMonthlyLedgerShouldSupplementShortfall()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            BudgetModel budget = BudgetModelTestData.CreateTestData1();
            StatementModel statement = StatementModelTestData.TestData1();

            LedgerEntryLine result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget, statement: statement);
            book.Output(true);

            Assert.AreEqual(64.71M, result.Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PhoneBucketCode).Balance);
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_WithStatementWithBalanceAdjustment599ShouldHaveSurplus1014()
        {
            LedgerBook book = LedgerBookTestData.TestData1();
            BudgetModel budget = BudgetModelTestData.CreateTestData1();
            StatementModel statement = StatementModelTestData.TestData1();

            LedgerEntryLine result = book.Reconcile(NextReconcileDate, NextReconcileBankBalance, budget, statement: statement);
            result.BalanceAdjustment(-599M, "Visa pmt not yet in statement");
            book.Output(true);
            Assert.AreEqual(1014.47M, result.CalculatedSurplus);
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
                statement: StatementModelTestData.TestData1());
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
                statement: statement);
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
                statement: StatementModelTestData.TestData1());
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
                statement: StatementModelTestData.TestData1());
        }

        [TestMethod]
        public void UsingTestData5_Reconcile_ShouldAutoMatchTransactionsAndLinkToStatementTransaction()
        {
            // The automatched credit ledger transaction from last month should be linked to the statement transaction.
            StatementModel statementModelTestData = StatementModelTestData.TestData5();
            List<Transaction> statementTransactions = statementModelTestData.AllTransactions.Where(t => t.Reference1 == "agkT9kC").ToList();
            Debug.Assert(statementTransactions.Count() == 2);

            LedgerBook book = ActOnTestData5(statementModelTestData);
            LedgerEntry previousMonthLine =
                book.Reconciliations.Single(line => line.Date == new DateTime(2013, 08, 15)).Entries.Single(e => e.LedgerBucket.BudgetBucket == StatementModelTestData.InsHomeBucket);
            BudgetCreditLedgerTransaction previousLedgerTxn = previousMonthLine.Transactions.OfType<BudgetCreditLedgerTransaction>().Single();

            // Assert last month's ledger transaction has been linked to the credit 16/8/13
            Assert.AreEqual(statementTransactions.Single(t => t.Amount > 0).Id, previousLedgerTxn.Id);
        }

        [TestMethod]
        public void UsingTestData5_Reconcile_ShouldAutoMatchTransactionsAndResultIn1InsHomeTransaction()
        {
            // Two transactions should be removed as they are automatched to the previous month.
            LedgerBook book = ActOnTestData5();

            Assert.AreEqual(1, book.Reconciliations.First().Entries.Single(e => e.LedgerBucket.BudgetBucket == StatementModelTestData.InsHomeBucket).Transactions.Count());
            // Assert last month's ledger transaction has been linked to the credit 16/8/13
        }

        [TestMethod]
        public void UsingTestData5_Reconcile_ShouldAutoMatchTransactionsAndResultInInsHomeBalance1200()
        {
            LedgerBook book = ActOnTestData5();
            Assert.AreEqual(1200M, book.Reconciliations.First().Entries.Single(e => e.LedgerBucket.BudgetBucket == StatementModelTestData.InsHomeBucket).Balance);
        }

        [TestMethod]
        public void UsingTestData5_Reconcile_ShouldAutoMatchTransactionsAndUpdateLedgerAutoMatchRefSoItIsNotAutoMatchedAgain()
        {
            // Two transactions should be removed as they are automatched to the previous month.
            LedgerBook book = ActOnTestData5();
            LedgerEntry previousMonthLine =
                book.Reconciliations.Single(line => line.Date == new DateTime(2013, 08, 15)).Entries.Single(e => e.LedgerBucket.BudgetBucket == StatementModelTestData.InsHomeBucket);
            BudgetCreditLedgerTransaction previousLedgerTxn = previousMonthLine.Transactions.OfType<BudgetCreditLedgerTransaction>().Single();

            Console.WriteLine(previousLedgerTxn.AutoMatchingReference);
            Assert.AreNotEqual("agkT9kC", previousLedgerTxn.AutoMatchingReference);
        }

        [TestMethod]
        public void UsingTestData5_Reconcile_ShouldCreateToDoEntries()
        {
            ActOnTestData5();
            
            Assert.AreEqual(1, this.toDoCollection.OfType<TransferTask>().Count(t => t.Reference.IsSomething() && t.BucketCode.IsSomething()));
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationWarningException))]
        public void UsingTestData5_Reconcile_ShouldThrowWhenAutoMatchingTransactionAreMissingFromStatement()
        {
            LedgerBook book = ActOnTestData5(StatementModelTestData.TestData4());
            Assert.Fail();
        }

        private LedgerBook ActOnTestData5(StatementModel statementTestData = null)
        {
            LedgerBook book = LedgerBookTestData.TestData5();
            BudgetModel budget = BudgetModelTestData.CreateTestData5();
            if (statementTestData == null)
            {
                statementTestData = StatementModelTestData.TestData5();
            }

            Console.WriteLine("********************** BEFORE RUNNING RECONCILIATION *******************************");
            statementTestData.Output(NextReconcileDate.AddMonths(-1));
            book.Output(true);

            book.Reconcile(
                NextReconcileDate,
                new[] { new BankBalance(StatementModelTestData.ChequeAccount, 1850.5M), new BankBalance(StatementModelTestData.SavingsAccount, 1200M) },
                budget,
                statement: statementTestData,
                toDoList: this.toDoCollection);

            Console.WriteLine();
            Console.WriteLine("********************** AFTER RUNNING RECONCILIATION *******************************");
            book.Output(true);
            return book;
        }
    }

    // ReSharper restore InconsistentNaming
}