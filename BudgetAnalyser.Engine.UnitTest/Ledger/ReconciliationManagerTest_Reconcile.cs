using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Reconciliation;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.UnitTest.Helper;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rees.UnitTestUtilities;

namespace BudgetAnalyser.Engine.UnitTest.Ledger
{
    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class ReconciliationManagerTest_Reconcile
    {
        private static readonly IEnumerable<BankBalance> NextReconcileBankBalance = new[] { new BankBalance(StatementModelTestData.ChequeAccount, 2050M) };
        private static readonly DateTime ReconcileDate = new DateTime(2013, 09, 15);

        private IBudgetBucketRepository bucketRepo;
        private Mock<IReconciliationConsistency> mockReconciliationConsistency;
        private Mock<ITransactionRuleService> mockRuleService;
        private Mock<IReconciliationBuilder> mockReconciliationBuilder;
        private ReconciliationCreationManager subject;
        private IBudgetCurrencyContext testDataBudgetContext;
        private BudgetCollection testDataBudgets;
        private LedgerBook testDataLedgerBook;
        private StatementModel testDataStatement;
        private IList<ToDoTask> testDataToDoList;
        private IEnumerable<BankBalance> currentBankBalances;
        private ReconciliationResult testDataReconResult;
        private IDictionary<BudgetBucket, int> ledgerOrder;

        [TestMethod]
        public void MonthEndReconciliation_ShouldCreateSingleUseMatchingRulesForTransferToDos()
        {
            // Artificially create a transfer to do task when the reconciliation method is invoked on the LedgerBook.
            // Remember: the subject here is the ReconciliationCreationManager not the LedgerBook.
            ((LedgerBookTestHarness)this.testDataLedgerBook).ReconcileOverride = recon =>
            {
                this.testDataToDoList.Add(
                                          new TransferTask(string.Empty, true, true)
                                          {
                                              Reference = "sjghsh",
                                              Amount = 12.22M,
                                              BucketCode = StatementModelTestData.CarMtcBucket.Code
                                          });
                recon.Tasks = this.testDataToDoList;
            };

            // Expect a call to the Rule service to create the single use rule for the transfer.
            this.mockRuleService.Setup(m => m.CreateNewSingleUseRule(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<decimal?>(), true))
                .Returns(new SingleUseMatchingRule(this.bucketRepo));

            this.mockReconciliationBuilder.Setup(m => m.CreateNewMonthlyReconciliation(ReconcileDate, this.testDataBudgets.CurrentActiveBudget, this.testDataStatement, It.IsAny<BankBalance[]>()))
                .Returns(this.testDataReconResult);

            ActPeriodEndReconciliation();

            // Ensure the rule service was called with the appropriate parameters.
            this.mockRuleService.VerifyAll();
        }

        [TestInitialize]
        public void TestIntialise()
        {
            this.mockRuleService = new Mock<ITransactionRuleService>(MockBehavior.Strict);
            this.mockReconciliationBuilder = new Mock<IReconciliationBuilder>();
            this.mockReconciliationConsistency = new Mock<IReconciliationConsistency>();
            this.bucketRepo = new BucketBucketRepoAlwaysFind();
            this.testDataBudgets = BudgetModelTestData.CreateCollectionWith1And2();
            this.testDataBudgetContext = new BudgetCurrencyContext(this.testDataBudgets, this.testDataBudgets.CurrentActiveBudget);
            this.testDataStatement = new StatementModelBuilder()
                .TestData5()
                .AppendTransaction(new Transaction
                {
                    Account = StatementModelTestData.ChequeAccount,
                    Amount = -23.56M,
                    BudgetBucket = StatementModelTestData.RegoBucket,
                    Date = ReconcileDate.Date.AddDays(-1),
                    TransactionType = new NamedTransaction("Foo"),
                    Description = "Last transaction"
                })
                .Build();
            this.testDataToDoList = new List<ToDoTask>();
            this.testDataReconResult = new ReconciliationResult { Tasks = this.testDataToDoList };
            
            this.subject = new ReconciliationCreationManager(this.mockRuleService.Object, this.mockReconciliationConsistency.Object, this.mockReconciliationBuilder.Object, new FakeLogger());

            this.testDataLedgerBook = LedgerBookTestData.TestData5(() => new LedgerBookTestHarness());

            this.mockReconciliationConsistency.Setup(m => m.EnsureConsistency(It.IsAny<LedgerBook>())).Returns(new Mock<IDisposable>().Object);

            this.ledgerOrder = LedgerBookHelper.LedgerOrder(this.testDataLedgerBook);
        }

        [TestMethod]
        public void OutputTestData1()
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
        [Description("Ensures that the reconciliation process finds ledger transactions from the previous month that required funds to be transfered and matches these to " +
                     "statement transactions with automatching Id")]
        public void Reconcile_ShouldAutoMatchTransactionsAndLinkIdToStatementTransaction_GivenTestData5()
        {
            // The automatched credit ledger transaction from last month should be linked to the statement transaction.
            this.testDataStatement = StatementModelTestData.TestData5();
            List<Transaction> statementTransactions = this.testDataStatement.AllTransactions.Where(t => t.Reference1 == "agkT9kC").ToList();
            Debug.Assert(statementTransactions.Count() == 2);

            ActPeriodEndReconciliationOnTestData5(this.testDataStatement);
            var previousMonthLine =
                this.testDataLedgerBook.Reconciliations.Single(line => line.Date == new DateTime(2013, 08, 15)).Entries.Single(e => e.LedgerBucket.BudgetBucket == StatementModelTestData.InsHomeBucket);
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
            ActPeriodEndReconciliationOnTestData5();

            Assert.AreEqual(3, this.testDataLedgerBook.Reconciliations.First().Entries.Single(e => e.LedgerBucket.BudgetBucket == StatementModelTestData.InsHomeBucket).Transactions.Count());
            // Assert last month's ledger transaction has been linked to the credit 16/8/13
        }

        [TestMethod]
        public void Reconcile_ShouldAutoMatchTransactionsAndResultInInsHomeBalance300_GivenTestData5()
        {
            ActPeriodEndReconciliationOnTestData5();
            Assert.AreEqual(300M, this.testDataLedgerBook.Reconciliations.First().Entries.Single(e => e.LedgerBucket.BudgetBucket == StatementModelTestData.InsHomeBucket).Balance);
        }

        [TestMethod]
        public void Reconcile_ShouldNotCreateTasksForUserTransfersOfBudgetedAmounts_GivenTestData5()
        {
            ActPeriodEndReconciliationOnTestData5();
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

            ActPeriodEndReconciliationOnTestData5(this.testDataStatement);
            OutputToDoList();

            // Given the test data 5 set, there should only be one transfer task.
            Assert.AreEqual(2, this.testDataToDoList.OfType<TransferTask>().Count());
        }

        [TestMethod]
        public void Reconcile_ShouldResultIn1678_GivenTestData1()
        {
            this.testDataLedgerBook.Output(true);
            var result = ActPeriodEndReconciliation();
            result.Reconciliation.Output(this.ledgerOrder, false, true);
            Assert.AreEqual(1555.50M, result.Reconciliation.CalculatedSurplus);
        }

        [TestMethod]
        public void Reconcile_WithPaymentFromWrongAccountShouldCreateBalanceAdjustment_GivenTestData5()
        {
            this.testDataBudgetContext.Model.Output();
            var testTransaction = this.testDataStatement.AllTransactions.Last();
            testTransaction.BudgetBucket = LedgerBookTestData.HouseInsLedgerSavingsAccount.BudgetBucket;
            testTransaction.Account = StatementModelTestData.ChequeAccount;
            this.testDataStatement.Output(DateTime.MinValue);
            this.testDataLedgerBook.Output();

            var reconResult = ActPeriodEndReconciliation(bankBalances: new[] { new BankBalance(StatementModelTestData.ChequeAccount, 1850.5M), new BankBalance(StatementModelTestData.SavingsAccount, 1000M) });
            this.testDataLedgerBook.Output(true);

            var savingsBal = reconResult.Reconciliation.BankBalances.Single(b => b.Account == LedgerBookTestData.SavingsAccount).Balance
                             + reconResult.Reconciliation.BankBalanceAdjustments.Where(b => b.BankAccount == LedgerBookTestData.SavingsAccount).Sum(b => b.Amount);
            var chqBal = reconResult.Reconciliation.BankBalances.Single(b => b.Account == LedgerBookTestData.ChequeAccount).Balance
                         + reconResult.Reconciliation.BankBalanceAdjustments.Where(b => b.BankAccount == LedgerBookTestData.ChequeAccount).Sum(b => b.Amount);

            Assert.AreEqual(650M, savingsBal, "Savings should be decreased because savings still has the funds and needs to payback the Cheque account.");
            Assert.AreEqual(2200.50M, chqBal, "Chq should be increased after it has been paid back from savings.");
            Assert.AreEqual(0M, reconResult.Reconciliation.TotalBalanceAdjustments);
        }

        [TestMethod]
        public void Reconcile_WithPaymentFromWrongAccountShouldUpdateLedgerBalance_GivenTestData5()
        {
            this.testDataBudgets = BudgetModelTestData.CreateCollectionWith5();
            this.testDataBudgetContext = new BudgetCurrencyContext(this.testDataBudgets, this.testDataBudgets.CurrentActiveBudget);
            var testTransaction = this.testDataStatement.AllTransactions.Last();
            testTransaction.BudgetBucket = LedgerBookTestData.HouseInsLedgerSavingsAccount.BudgetBucket;
            testTransaction.Account = StatementModelTestData.ChequeAccount;
            testTransaction.Amount = -1250;
            this.testDataStatement.Output(DateTime.MinValue);
            this.testDataLedgerBook.Output();

            var reconResult = ActPeriodEndReconciliation(bankBalances: new[] { new BankBalance(StatementModelTestData.ChequeAccount, 1850.5M), new BankBalance(StatementModelTestData.SavingsAccount, 1000M) });
            reconResult.Reconciliation.Output(this.ledgerOrder, true, true);

            Assert.AreEqual(300M, reconResult.Reconciliation.Entries.Single(e => e.LedgerBucket == LedgerBookTestData.HouseInsLedgerSavingsAccount).Balance);
        }

        [TestMethod]
        [Description("This test overdraws the Hair ledger and tests to make sure the reconciliation process compensates and leaves it with a balance equal to the monthly budget amount.")]
        public void Reconcile_WithStatementSavedUpForHairLedgerShouldHaveBalance55_GivenTestData1()
        {
            List<Transaction> additionalTransactions = this.testDataStatement.AllTransactions.ToList();

            additionalTransactions.Add(new Transaction
            {
                Account = additionalTransactions.First().Account,
                Amount = -264M,
                BudgetBucket = additionalTransactions.First(t => t.BudgetBucket.Code == TestDataConstants.HairBucketCode).BudgetBucket,
                Date = new DateTime(2013, 09, 13)
            });
            this.testDataStatement.LoadTransactions(additionalTransactions);

            var result = ActPeriodEndReconciliation();
            result.Reconciliation.Output(this.ledgerOrder, true, true);

            Assert.AreEqual(55M, result.Reconciliation.Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode).Balance);
            Assert.IsTrue(result.Reconciliation.Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode).NetAmount < 0);
        }

        [TestMethod]
        public void Reconcile_WithStatementShouldHave2HairTransactions_GivenTestData1()
        {
            this.testDataLedgerBook.Output();
            var result = ActPeriodEndReconciliation();
            Assert.AreEqual(2, result.Reconciliation.Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode).Transactions.Count());
        }

        [TestMethod]
        public void Reconcile_WithStatementShouldHave3PowerTransactions_GivenTestData1()
        {
            this.testDataLedgerBook.Output();
            var result = ActPeriodEndReconciliation();
            result.Reconciliation.Output(this.ledgerOrder, true, true);
            Assert.AreEqual(3, result.Reconciliation.Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PowerBucketCode).Transactions.Count());
        }

        [TestMethod]
        public void Reconcile_WithStatementShouldHaveSurplus1613_GivenTestData1()
        {
            var result = ActPeriodEndReconciliation();
            result.Reconciliation.Output(this.ledgerOrder, true, true);
            Assert.AreEqual(1555.50M, result.Reconciliation.CalculatedSurplus);
        }

        [TestMethod]
        public void Reconcile_WithStatementSpentMonthlyLedgerShouldSupplementShortfall_GivenTestData1()
        {
            var result = ActPeriodEndReconciliation();
            result.Reconciliation.Output(this.ledgerOrder, true, true);
            Assert.AreEqual(0M, result.Reconciliation.Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PhoneBucketCode).Balance);
        }

        [TestMethod]
        public void Reconcile_WithStatementWithBalanceAdjustment599ShouldHaveSurplus1014_GivenTestData1()
        {
            var result = ActPeriodEndReconciliation();
            result.Reconciliation.BalanceAdjustment(-599M, "Visa pmt not yet in statement", new ChequeAccount("Chq"));
            result.Reconciliation.Output(this.ledgerOrder, true, true);
            Assert.AreEqual(956.50M, result.Reconciliation.CalculatedSurplus);
        }

        [TestMethod]
        public void Reconcile_ShouldCreateToDoEntries_GivenTestData5()
        {
            ActPeriodEndReconciliationOnTestData5();
            OutputToDoList();
            Assert.AreEqual(1, this.testDataToDoList.OfType<TransferTask>().Count(t => t.Reference.IsSomething() && t.BucketCode.IsSomething()));
        }

        [TestMethod]
        public void Reconcile_ShouldCreateBalanceAdjustmentOf150_GivenSavingsMonthlyBudgetAmountsSumTo150()
        {
            // 95 Car Mtc Monthly budget
            // 55 Hair cut monthly budget
            // ===
            // 150 Balance Adjustment expected in Savings
            // Power 175 goes in Chq
            this.testDataLedgerBook = new LedgerBookBuilder()
                .IncludeLedger(new SavedUpForLedger { BudgetBucket = StatementModelTestData.CarMtcBucket, StoredInAccount = LedgerBookTestData.SavingsAccount })
                .IncludeLedger(new SavedUpForLedger { BudgetBucket = StatementModelTestData.HairBucket, StoredInAccount = LedgerBookTestData.SavingsAccount })
                .IncludeLedger(LedgerBookTestData.PowerLedger)
                .Build();

            var result = ActPeriodEndReconciliation();

            result.Reconciliation.Output(this.ledgerOrder, true, true);
            
            Assert.AreEqual(150M, result.Reconciliation.BankBalanceAdjustments.Single(b => b.BankAccount == LedgerBookTestData.SavingsAccount).Amount);
            Assert.AreEqual(-150M, result.Reconciliation.BankBalanceAdjustments.Single(b => b.BankAccount == LedgerBookTestData.ChequeAccount).Amount);
        }

        [TestMethod]
        public void Reconcile_ShouldAutoMatchTransactionsAndUpdateLedgerAutoMatchRefSoItIsNotAutoMatchedAgain_GivenTestData5()
        {
            // Two transactions should be removed as they are automatched to the previous month.
            var result = ActPeriodEndReconciliationOnTestData5();
            var previousMonthLine =
                this.testDataLedgerBook.Reconciliations.Single(line => line.Date == new DateTime(2013, 08, 15))
                    .Entries.Single(e => e.LedgerBucket.BudgetBucket == StatementModelTestData.InsHomeBucket);
            var previousLedgerTxn = previousMonthLine.Transactions.OfType<BudgetCreditLedgerTransaction>().Single();

            Console.WriteLine(previousLedgerTxn.AutoMatchingReference);
            Assert.AreNotEqual("agkT9kC", previousLedgerTxn.AutoMatchingReference);
        }

        [TestMethod]
        public void AddLedger_ShouldNewLedgerInNextReconcile_GivenTestData1()
        {
            this.testDataLedgerBook.AddLedger(LedgerBookTestData.RatesLedger);
            var result = ActPeriodEndReconciliation();

            Assert.IsTrue(result.Reconciliation.Entries.Any(e => e.LedgerBucket == LedgerBookTestData.RatesLedger));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Reconcile_ShouldThrow_GivenInvalidLedgerBook()
        {
            ActPeriodEndReconciliation(new DateTime(2012, 02, 20));
        }

        [TestMethod]
        public void Reconcile_ShouldThrow_GivenTestData1AndNoStatementModelTransactions()
        {
            this.testDataStatement = new StatementModel(new FakeLogger()) { StorageKey = "C:\\Foo.xml" };
            try
            {
                ActPeriodEndReconciliation(new DateTime(2013, 10, 15));
            }
            catch (ValidationWarningException ex)
            {
                if (ex.Source == "5") return;
            }

            Assert.Fail();
        }

        [TestMethod]
        public void Reconcile_ShouldThrow_GivenTestData1AndUnclassifiedTransactions()
        {
            this.testDataStatement = new StatementModelBuilder()
                .TestData1()
                .AppendTransaction(new Transaction
                {
                    Account = StatementModelTestData.ChequeAccount,
                    Amount = 12.45M,
                    Date = ReconcileDate.AddDays(-1),
                    Description = "Foo bar"
                })
                .Build();
            try
            {
                ActPeriodEndReconciliation();
            }
            catch (ValidationWarningException ex)
            {
                if (ex.Source == "3") return;
            }

            Assert.Fail();
        }

        [TestMethod]
        public void Reconcile_ShouldNotThrow_GivenTestData1AndUnclassifiedTransactionsOutsideReconPeriod()
        {
            var reconDate = new DateTime(2013, 9, 15);
            this.mockReconciliationBuilder.Setup(m => m.CreateNewMonthlyReconciliation(reconDate, this.testDataBudgets.CurrentActiveBudget, this.testDataStatement, It.IsAny<BankBalance[]>()))
                .Returns(this.testDataReconResult);
            Transaction aTransaction = this.testDataStatement.AllTransactions.First();
            PrivateAccessor.SetField(aTransaction, "budgetBucket", null);

            ActPeriodEndReconciliation(reconDate);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Reconcile_ShouldThrow_GivenTestData1WithDateEqualToExistingLine()
        {
            ActPeriodEndReconciliation(new DateTime(2013, 08, 15));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Reconcile_ShouldThrow_GivenTestData1WithDateLessThanExistingLine()
        {
            ActPeriodEndReconciliation(new DateTime(2013, 07, 15));
        }

        [TestMethod]
        public void Reconcile_ShouldThrowWhenAutoMatchingTransactionAreMissingFromStatement_GivenTestData5()
        {
            try
            {
                ActPeriodEndReconciliationOnTestData5(StatementModelTestData.TestData4());
            }
            catch (ValidationWarningException ex)
            {
                if (ex.Source == "1") return;
            }

            Assert.Fail();
        }

        [TestMethod]
        public void Reconcile_ShouldThrowValidationWarning_GivenStatmentIsMissingTransactionsForLastDay()
        {
            try
            {
                ActPeriodEndReconciliationOnTestData5();
            }
            catch (ValidationWarningException ex)
            {
                if (ex.Source == "2") return;
            }

            Assert.Fail();
        }

        [TestMethod]
        [Description("When there is more than one problem, the first exception should not prevent the user from seeing the other different exception.")]
        public void Reconcile_ShouldThrowValidationWarning_GivenTwoOrMoreWarningsHaveAlreadyBeenThrown()
        {
            // First the statement has a transaction that is not classified with a bucket.
            this.testDataStatement = new StatementModelBuilder()
                .TestData1()
                .AppendTransaction(new Transaction
                {
                    Account = StatementModelTestData.ChequeAccount,
                    Amount = 12.45M,
                    Date = ReconcileDate.AddDays(-1),
                    Description = "Foo bar"
                })
                .Build();
            try
            {
                ActPeriodEndReconciliation();
            }
            catch (ValidationWarningException ex)
            {
                if (ex.Source != "3") Assert.Fail();
            }

            // Second time thru, we choose to ignore validation warnings messages we've seen before.
            try
            {
                ActPeriodEndReconciliationOnTestData5(ignoreWarnings: true);
            }
            catch (ValidationWarningException ex)
            {
                if (ex.Source == "2") return;
            }

            Assert.Fail();
        }

        private ReconciliationResult ActPeriodEndReconciliation(DateTime? reconciliationDate = null, IEnumerable<BankBalance> bankBalances = null, bool ignoreWarnings = false)
        {
            this.currentBankBalances = bankBalances ?? NextReconcileBankBalance;

            return this.testDataReconResult = this.subject.PeriodEndReconciliation(this.testDataLedgerBook,
                                                        reconciliationDate ?? ReconcileDate,
                                                        this.testDataBudgetContext,
                                                        this.testDataStatement,
                                                        ignoreWarnings,
                                                        this.currentBankBalances.ToArray());
        }

        private ReconciliationResult ActPeriodEndReconciliationOnTestData5(StatementModel statementModelTestData = null, bool ignoreWarnings = false)
        {
            this.testDataBudgets = BudgetModelTestData.CreateCollectionWith5();
            this.testDataBudgetContext = new BudgetCurrencyContext(this.testDataBudgets, this.testDataBudgets.CurrentActiveBudget);
            this.testDataStatement = statementModelTestData ?? StatementModelTestData.TestData5();

            Console.WriteLine("********************** BEFORE RUNNING RECONCILIATION *******************************");
            this.testDataStatement.Output(ReconcileDate.AddMonths(-1));
            this.testDataLedgerBook.Output(true);

            this.testDataReconResult = ActPeriodEndReconciliation(bankBalances: new[] { new BankBalance(StatementModelTestData.ChequeAccount, 1850.5M), new BankBalance(StatementModelTestData.SavingsAccount, 1200M) },
                                       ignoreWarnings: ignoreWarnings);

            Console.WriteLine();
            Console.WriteLine("********************** AFTER RUNNING RECONCILIATION *******************************");
            this.testDataLedgerBook.Output(true);
            return this.testDataReconResult;
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
}