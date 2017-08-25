using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Reconciliation;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.UnitTest.Helper;
using BudgetAnalyser.Engine.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Ledger
{
    // ReSharper disable InconsistentNaming
    [TestClass]
    public class LedgerBook_ReconcileTest
    {
        private static readonly BankBalance[] NextReconcileBankBalance = { new BankBalance(StatementModelTestData.ChequeAccount, 1850.5M) };
        private static readonly DateTime ReconcileDate = new DateTime(2013, 09, 15);
        private LedgerBook subject;
        private BudgetModel testDataBudget;
        private StatementModel testDataStatement;
        private IEnumerable<ToDoTask> testDataToDoList;

        [TestMethod]
        public void AddLedger_ShouldAddToLedgersCollection_GivenTestData1()
        {
            this.subject.AddLedger(LedgerBookTestData.RatesLedger);

            Assert.IsTrue(this.subject.Ledgers.Any(l => l.BudgetBucket == LedgerBookTestData.RatesLedger.BudgetBucket));
        }

        [TestMethod]
        public void AddLedger_ShouldBeIncludedInNextReconcile_GivenTestData1()
        {
            this.subject.AddLedger(LedgerBookTestData.RatesLedger);
            var result = Act();

            Assert.IsTrue(result.Reconciliation.Entries.Any(e => e.LedgerBucket == LedgerBookTestData.RatesLedger));
        }

        [TestMethod]
        public void CompareObjectMotherTestData1ToBuilderTestData1()
        {
            var book = new LedgerBookBuilder().TestData1().Build();
            book.Output(true);
        }

        [TestMethod]
        public void OutputTestData5()
        {
            LedgerBookTestData.TestData5().Output(true);
        }

        [TestMethod]
        public void Reconcile_Output_GivenTestData1()
        {
            Act();
            this.subject.Output();
        }

        [TestMethod]
        [Description("Ensures that the reconciliation process finds ledger transactions from the previous month that required funds to be transfered and matches these to " +
                     "statement transactions with automatching Id")]
        public void Reconcile_ShouldAutoMatchTransactionsAndLinkIdToStatementTransaction_GivenTestData5()
        {
            // The automatched credit ledger transaction from last month should be linked to the statement transaction.
            this.testDataStatement = StatementModelTestData.TestData5();
            List<Transaction> statementTransactions = this.testDataStatement.AllTransactions.Where(t => t.Reference1 == "agkT9kC").ToList();
            Debug.Assert(statementTransactions.Count() == 2);

            ActOnTestData5(this.testDataStatement);
            var previousMonthLine =
                this.subject.Reconciliations.Single(line => line.Date == new DateTime(2013, 08, 15)).Entries.Single(e => e.LedgerBucket.BudgetBucket == StatementModelTestData.InsHomeBucket);
            var previousLedgerTxn = previousMonthLine.Transactions.OfType<BudgetCreditLedgerTransaction>().Single();

            // Assert last month's ledger transaction has been linked to the credit 16/8/13
            Assert.AreEqual(statementTransactions.Single(t => t.Amount > 0).Id, previousLedgerTxn.Id);
        }

        [TestMethod]
        [Description("Ensures the reconciliation process matches transactions that should be automatched and then ignored - not imported into the Ledger Transaction listing." +
                     "If this is not working there will be more than 3 ledger transactions, because there are two statement transactions for INSHOME that should be matched and ignored.")]
        public void Reconcile_ShouldAutoMatchTransactionsAndResultIn3InsHomeTransactions_GivenTestData5()
        {
            // Two transactions should be removed as they are automatched to the previous month.
            ActOnTestData5();

            Assert.AreEqual(3, this.subject.Reconciliations.First().Entries.Single(e => e.LedgerBucket.BudgetBucket == StatementModelTestData.InsHomeBucket).Transactions.Count());
            // Assert last month's ledger transaction has been linked to the credit 16/8/13
        }

        [TestMethod]
        public void Reconcile_ShouldAutoMatchTransactionsAndResultInInsHomeBalance300_GivenTestData5()
        {
            ActOnTestData5();
            Assert.AreEqual(300M, this.subject.Reconciliations.First().Entries.Single(e => e.LedgerBucket.BudgetBucket == StatementModelTestData.InsHomeBucket).Balance);
        }

        [TestMethod]
        public void Reconcile_ShouldAutoMatchTransactionsAndUpdateLedgerAutoMatchRefSoItIsNotAutoMatchedAgain_GivenTestData5()
        {
            // Two transactions should be removed as they are automatched to the previous month.
            ActOnTestData5();
            var previousMonthLine =
                this.subject.Reconciliations.Single(line => line.Date == new DateTime(2013, 08, 15)).Entries.Single(e => e.LedgerBucket.BudgetBucket == StatementModelTestData.InsHomeBucket);
            var previousLedgerTxn = previousMonthLine.Transactions.OfType<BudgetCreditLedgerTransaction>().Single();

            Console.WriteLine(previousLedgerTxn.AutoMatchingReference);
            Assert.AreNotEqual("agkT9kC", previousLedgerTxn.AutoMatchingReference);
        }

        [TestMethod]
        public void Reconcile_ShouldCreateBalanceAdjustmentOf150_GivenSavingsMonthlyBudgetAmountsSumTo150()
        {
            // 95 Car Mtc Monthly budget
            // 55 Hair cut monthly budget
            // ===
            // 150 Balance Adjustment expected in Savings
            // Power 175 goes in Chq
            this.subject = new LedgerBookBuilder()
                .IncludeLedger(new SavedUpForLedger { BudgetBucket = StatementModelTestData.CarMtcBucket, StoredInAccount = LedgerBookTestData.SavingsAccount })
                .IncludeLedger(new SavedUpForLedger { BudgetBucket = StatementModelTestData.HairBucket, StoredInAccount = LedgerBookTestData.SavingsAccount })
                .IncludeLedger(LedgerBookTestData.PowerLedger)
                .Build();

            Act();

            this.subject.Output(true);
            var resultRecon = this.subject.Reconciliations.First();

            Assert.AreEqual(150M, resultRecon.BankBalanceAdjustments.Single(b => b.BankAccount == LedgerBookTestData.SavingsAccount).Amount);
            Assert.AreEqual(-150M, resultRecon.BankBalanceAdjustments.Single(b => b.BankAccount == LedgerBookTestData.ChequeAccount).Amount);
        }

        [TestMethod]
        public void Reconcile_ShouldCreateToDoEntries_GivenTestData5()
        {
            ActOnTestData5();
            OutputToDoList();
            Assert.AreEqual(1, this.testDataToDoList.OfType<TransferTask>().Count(t => t.Reference.IsSomething() && t.BucketCode.IsSomething()));
        }

        [TestMethod]
        public void Reconcile_ShouldInsertLastestInFront_GivenTestData1()
        {
            var result = Act();
            Assert.AreEqual(this.subject.Reconciliations.First(), result.Reconciliation);
        }

        [TestMethod]
        public void Reconcile_ShouldNotCreateTasksForUserTransfersOfBudgetedAmounts_GivenTestData5()
        {
            ActOnTestData5();
            OutputToDoList();

            // Given the test data 5 set, there should only be one transfer task.
            Assert.AreEqual(1, this.testDataToDoList.OfType<TransferTask>().Count());
        }

        [TestMethod]
        [Description("This test is effectively testing two things: First that budgeted amount doesn't show up as a payment when there is a payment going out." +
                     "Second, that a payment transfer task is created successfully.")]
        public void Reconcile_ShouldNotCreateTasksForUserTransfersOfBudgetedAmounts_GivenTestData5AndPaymentFromDifferentAccount()
        {
            // Modify a InsHome payment transaction, originally coming out of the Savings account where the ledger is stored, to the Cheque account.
            this.testDataStatement = StatementModelTestData.TestData5();
            var insHomePayment = this.testDataStatement.AllTransactions.Single(t => t.BudgetBucket == StatementModelTestData.InsHomeBucket && t.Amount == -1000M);
            insHomePayment.Account = StatementModelTestData.ChequeAccount;

            ActOnTestData5(this.testDataStatement);
            OutputToDoList();

            // Given the test data 5 set, there should only be one transfer task.
            Assert.AreEqual(2, this.testDataToDoList.OfType<TransferTask>().Count());
        }

        [TestMethod]
        public void Reconcile_ShouldResultIn1678_GivenTestData1()
        {
            var result = Act();
            this.subject.Output(true);
            Assert.AreEqual(1555.50M, result.Reconciliation.CalculatedSurplus);
        }

        [TestMethod]
        public void Reconcile_ShouldResultIn4Lines_GivenTestData1()
        {
            Act();

            Assert.AreEqual(4, this.subject.Reconciliations.Count());
        }

        [TestMethod]
        public void Reconcile_WithPaymentFromWrongAccountShouldCreateBalanceAdjustment_GivenTestData1()
        {
            this.subject = LedgerBookTestData.TestData5();
            var testTransaction = this.testDataStatement.AllTransactions.Last();
            testTransaction.BudgetBucket = LedgerBookTestData.HouseInsLedgerSavingsAccount.BudgetBucket;
            testTransaction.Account = StatementModelTestData.ChequeAccount;
            this.testDataStatement.Output(DateTime.MinValue);
            this.subject.Output();

            var reconResult = Act(bankBalances: new[] { new BankBalance(StatementModelTestData.ChequeAccount, 1850.5M), new BankBalance(StatementModelTestData.SavingsAccount, 1000M) });
            this.subject.Output(true);

            var savingsBal = reconResult.Reconciliation.BankBalances.Single(b => b.Account == LedgerBookTestData.SavingsAccount).Balance
                             + reconResult.Reconciliation.BankBalanceAdjustments.Where(b => b.BankAccount == LedgerBookTestData.SavingsAccount).Sum(b => b.Amount);
            var chqBal = reconResult.Reconciliation.BankBalances.Single(b => b.Account == LedgerBookTestData.ChequeAccount).Balance
                         + reconResult.Reconciliation.BankBalanceAdjustments.Where(b => b.BankAccount == LedgerBookTestData.ChequeAccount).Sum(b => b.Amount);

            Assert.AreEqual(650M, savingsBal, "Savings should be decreased because savings still has the funds and needs to payback the Cheque account."); 
            Assert.AreEqual(2200.50M, chqBal, "Chq should be increased after it has been paid back from savings.");
            Assert.AreEqual(0M, reconResult.Reconciliation.TotalBalanceAdjustments);
        }

        [TestMethod]
        public void Reconcile_WithStatementOutput_GivenTestData1()
        {
            Act();
            this.subject.Output(true);
        }

        [TestMethod]
        [Description("This test overdraws the Hair ledger and tests to make sure the reconciliation process compensates and leaves it with a balance equal to the monthly payment amount.")]
        public void Reconcile_WithStatementSavedUpForHairLedgerShouldHaveBalance55_GivenTestData1()
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

            var result = Act();
            this.subject.Output(true);

            Assert.AreEqual(55M, result.Reconciliation.Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode).Balance);
            Assert.IsTrue(result.Reconciliation.Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode).NetAmount < 0);
        }

        [TestMethod]
        public void Reconcile_WithStatementShouldHave2HairTransactions_GivenTestData1()
        {
            this.subject.Output();
            var result = Act();
            Assert.AreEqual(2, result.Reconciliation.Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode).Transactions.Count());
        }

        [TestMethod]
        public void Reconcile_WithStatementShouldHave3PowerTransactions_GivenTestData1()
        {
            this.subject.Output();
            var result = Act();
            this.subject.Output(true);
            Assert.AreEqual(3, result.Reconciliation.Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PowerBucketCode).Transactions.Count());
        }

        [TestMethod]
        public void Reconcile_WithStatementShouldHaveSurplus1613_GivenTestData1()
        {
            var result = Act();
            this.subject.Output(true);
            Assert.AreEqual(1555.50M, result.Reconciliation.CalculatedSurplus);
        }

        [TestMethod]
        public void Reconcile_WithStatementSpentMonthlyLedgerShouldSupplementShortfall_GivenTestData1()
        {
            var result = Act();
            this.subject.Output(true);
            Assert.AreEqual(0M, result.Reconciliation.Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PhoneBucketCode).Balance);
        }

        [TestMethod]
        public void Reconcile_WithStatementWithBalanceAdjustment599ShouldHaveSurplus1014_GivenTestData1()
        {
            var result = Act();
            result.Reconciliation.BalanceAdjustment(-599M, "Visa pmt not yet in statement", new ChequeAccount("Chq"));
            this.subject.Output(true);
            Assert.AreEqual(956.50M, result.Reconciliation.CalculatedSurplus);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            this.testDataBudget = BudgetModelTestData.CreateTestData1();
            this.testDataStatement = StatementModelTestData.TestData1();
            this.subject = LedgerBookTestData.TestData1();
        }


        private ReconciliationResult Act(DateTime? reconciliationDate = null, BankBalance[] bankBalances = null)
        {
            return this.subject.Reconcile(reconciliationDate ?? ReconcileDate, this.testDataBudget, this.testDataStatement, bankBalances ?? NextReconcileBankBalance);
        }

        private void ActOnTestData5(StatementModel statementModelTestData = null)
        {
            this.subject = LedgerBookTestData.TestData5();
            this.testDataBudget = BudgetModelTestData.CreateTestData5();
            this.testDataStatement = statementModelTestData ?? StatementModelTestData.TestData5();

            Console.WriteLine("********************** BEFORE RUNNING RECONCILIATION *******************************");
            this.testDataStatement.Output(ReconcileDate.AddMonths(-1));
            this.subject.Output(true);

            var result = Act(bankBalances: new[] { new BankBalance(StatementModelTestData.ChequeAccount, 1850.5M), new BankBalance(StatementModelTestData.SavingsAccount, 1200M) });
            this.testDataToDoList = result.Tasks.ToList();

            Console.WriteLine();
            Console.WriteLine("********************** AFTER RUNNING RECONCILIATION *******************************");
            this.subject.Output(true);
        }

        private void OutputToDoList()
        {
            Console.WriteLine("==================== TODO LIST ===========================");
            Console.WriteLine("Type       Generated  Reference  Amount     Description");
            foreach (var task in this.testDataToDoList)
            {
                Console.WriteLine(
                                  "{0} {1} {2} {3} {4}",
                                  task.GetType().Name.PadRight(10).Truncate(10),
                                  task.SystemGenerated.ToString().PadRight(10),
                                  (task as TransferTask)?.Reference?.PadRight(10).Truncate(10) ?? "          ",
                                  (task as TransferTask)?.Amount.ToString("C").PadRight(10).Truncate(10) ?? "          ",
                                  task.Description);
            }
        }
    }

    // ReSharper restore InconsistentNaming
}