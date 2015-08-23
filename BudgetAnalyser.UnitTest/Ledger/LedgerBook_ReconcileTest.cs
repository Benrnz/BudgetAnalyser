using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private static readonly DateTime ReconcileDate = new DateTime(2013, 09, 15);
        private LedgerBook subject;
        private BudgetModel testDataBudget;
        private StatementModel testDataStatement;
        private ToDoCollection testDataToDoList;

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
            LedgerBookTestData.TestData5().Output(true);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            this.testDataBudget = BudgetModelTestData.CreateTestData1();
            this.testDataStatement = StatementModelTestData.TestData1();
            this.testDataToDoList = new ToDoCollection();
            this.subject = LedgerBookTestData.TestData1();
        }

        [TestMethod]
        public void UsingTestData1_AddLedger_ShouldAddToLedgersCollection()
        {
            this.subject.AddLedger(new SavedUpForExpenseBucket("FOO", "Foo bar"), null);

            Assert.IsTrue(this.subject.Ledgers.Any(l => l.BudgetBucket.Code == "FOO"));
        }

        [TestMethod]
        public void UsingTestData1_AddLedger_ShouldBeIncludedInNextReconcile()
        {
            this.subject.AddLedger(new SavedUpForExpenseBucket("FOO", "Foo bar"), null);
            LedgerEntryLine result = Act();

            Assert.IsTrue(result.Entries.Any(e => e.LedgerBucket.BudgetBucket.Code == "FOO"));
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_Output()
        {
            Act();
            this.subject.Output();
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_ShouldInsertLastestInFront()
        {
            LedgerEntryLine result = Act();
            Assert.AreEqual(result, this.subject.Reconciliations.First());
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_ShouldResultIn1613()
        {
            LedgerEntryLine result = Act();
            this.subject.Output(true);
            Assert.AreEqual(1613.47M, result.CalculatedSurplus);
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_ShouldResultIn4Lines()
        {
            LedgerEntryLine result = Act();

            Assert.AreEqual(4, this.subject.Reconciliations.Count());
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_WithStatementOutput()
        {
            Act();
            this.subject.Output(true);
        }

        [TestMethod]
        [Description("This test overdraws the Hair ledger and tests to make sure the reconciliation process compensates and leaves it with a balance equal to the monthly payment amount.")]
        public void UsingTestData1_Reconcile_WithStatementSavedUpForHairLedgerShouldHaveBalance55()
        {
            List<Transaction> additionalTransactions = this.testDataStatement.AllTransactions.ToList();

            additionalTransactions.Add(
                new Transaction
                {
                    Account = additionalTransactions.First().Account,
                    Amount = -264M,
                    BudgetBucket = additionalTransactions.First(t => t.BudgetBucket.Code == TestDataConstants.HairBucketCode).BudgetBucket,
                    Date = new DateTime(2013, 09, 13)
                });
            this.testDataStatement.LoadTransactions(additionalTransactions);

            LedgerEntryLine result = Act();
            this.subject.Output(true);

            Assert.AreEqual(55M, result.Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode).Balance);
            Assert.IsTrue(result.Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode).NetAmount < 0);
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_WithStatementShouldHave2HairTransactions()
        {
            this.subject.Output();
            LedgerEntryLine result = Act();
            Assert.AreEqual(2, result.Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode).Transactions.Count());
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_WithStatementShouldHave3PowerTransactions()
        {
            this.subject.Output();
            LedgerEntryLine result = Act();
            this.subject.Output(true);
            Assert.AreEqual(3, result.Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PowerBucketCode).Transactions.Count());
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_WithStatementShouldHaveSurplus1613()
        {
            LedgerEntryLine result = Act();
            this.subject.Output(true);
            Assert.AreEqual(1613.47M, result.CalculatedSurplus);
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_WithStatementSpentMonthlyLedgerShouldSupplementShortfall()
        {
            LedgerEntryLine result = Act();
            this.subject.Output(true);
            Assert.AreEqual(64.71M, result.Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PhoneBucketCode).Balance);
        }

        [TestMethod]
        public void UsingTestData1_Reconcile_WithStatementWithBalanceAdjustment599ShouldHaveSurplus1014()
        {
            LedgerEntryLine result = Act();
            result.BalanceAdjustment(-599M, "Visa pmt not yet in statement");
            this.subject.Output(true);
            Assert.AreEqual(1014.47M, result.CalculatedSurplus);
        }

        [TestMethod]
        public void UsingTestData5_Reconcile_ShouldAutoMatchTransactionsAndLinkToStatementTransaction()
        {
            // The automatched credit ledger transaction from last month should be linked to the statement transaction.
            this.testDataStatement = StatementModelTestData.TestData5();
            List<Transaction> statementTransactions = this.testDataStatement.AllTransactions.Where(t => t.Reference1 == "agkT9kC").ToList();
            Debug.Assert(statementTransactions.Count() == 2);

            ActOnTestData5(this.testDataStatement);
            LedgerEntry previousMonthLine =
                this.subject.Reconciliations.Single(line => line.Date == new DateTime(2013, 08, 15)).Entries.Single(e => e.LedgerBucket.BudgetBucket == StatementModelTestData.InsHomeBucket);
            BudgetCreditLedgerTransaction previousLedgerTxn = previousMonthLine.Transactions.OfType<BudgetCreditLedgerTransaction>().Single();

            // Assert last month's ledger transaction has been linked to the credit 16/8/13
            Assert.AreEqual(statementTransactions.Single(t => t.Amount > 0).Id, previousLedgerTxn.Id);
        }

        [TestMethod]
        public void UsingTestData5_Reconcile_ShouldAutoMatchTransactionsAndResultIn1InsHomeTransaction()
        {
            // Two transactions should be removed as they are automatched to the previous month.
            ActOnTestData5();

            Assert.AreEqual(1, this.subject.Reconciliations.First().Entries.Single(e => e.LedgerBucket.BudgetBucket == StatementModelTestData.InsHomeBucket).Transactions.Count());
            // Assert last month's ledger transaction has been linked to the credit 16/8/13
        }

        [TestMethod]
        public void UsingTestData5_Reconcile_ShouldAutoMatchTransactionsAndResultInInsHomeBalance1200()
        {
            ActOnTestData5();
            Assert.AreEqual(1200M, this.subject.Reconciliations.First().Entries.Single(e => e.LedgerBucket.BudgetBucket == StatementModelTestData.InsHomeBucket).Balance);
        }

        [TestMethod]
        public void UsingTestData5_Reconcile_ShouldAutoMatchTransactionsAndUpdateLedgerAutoMatchRefSoItIsNotAutoMatchedAgain()
        {
            // Two transactions should be removed as they are automatched to the previous month.
            ActOnTestData5();
            LedgerEntry previousMonthLine =
                this.subject.Reconciliations.Single(line => line.Date == new DateTime(2013, 08, 15)).Entries.Single(e => e.LedgerBucket.BudgetBucket == StatementModelTestData.InsHomeBucket);
            BudgetCreditLedgerTransaction previousLedgerTxn = previousMonthLine.Transactions.OfType<BudgetCreditLedgerTransaction>().Single();

            Console.WriteLine(previousLedgerTxn.AutoMatchingReference);
            Assert.AreNotEqual("agkT9kC", previousLedgerTxn.AutoMatchingReference);
        }

        [TestMethod]
        public void UsingTestData5_Reconcile_ShouldCreateToDoEntries()
        {
            ActOnTestData5();

            Assert.AreEqual(1, this.testDataToDoList.OfType<TransferTask>().Count(t => t.Reference.IsSomething() && t.BucketCode.IsSomething()));
        }

        private LedgerEntryLine Act(DateTime? reconciliationDate = null, IEnumerable<BankBalance> bankBalances = null)
        {
            return this.subject.Reconcile(reconciliationDate ?? ReconcileDate, bankBalances ?? NextReconcileBankBalance, this.testDataBudget, this.testDataToDoList, this.testDataStatement);
        }

        private void ActOnTestData5(StatementModel statementModelTestData = null)
        {
            this.subject = LedgerBookTestData.TestData5();
            this.testDataBudget = BudgetModelTestData.CreateTestData5();
            this.testDataStatement = statementModelTestData ?? StatementModelTestData.TestData5();

            Console.WriteLine("********************** BEFORE RUNNING RECONCILIATION *******************************");
            this.testDataStatement.Output(ReconcileDate.AddMonths(-1));
            this.subject.Output(true);

            Act(bankBalances: new[] { new BankBalance(StatementModelTestData.ChequeAccount, 1850.5M), new BankBalance(StatementModelTestData.SavingsAccount, 1200M) });

            Console.WriteLine();
            Console.WriteLine("********************** AFTER RUNNING RECONCILIATION *******************************");
            this.subject.Output(true);
        }
    }

    // ReSharper restore InconsistentNaming
}