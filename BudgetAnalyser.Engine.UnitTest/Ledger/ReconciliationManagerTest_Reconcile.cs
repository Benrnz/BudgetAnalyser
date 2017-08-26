using System;
using System.Collections.Generic;
using System.Linq;
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
        private ReconciliationCreationManager subject;
        private IBudgetCurrencyContext testDataBudgetContext;
        private BudgetCollection testDataBudgets;

        private LedgerBook testDataLedgerBook;
        private StatementModel testDataStatement;
        private IList<ToDoTask> testDataToDoList;
        private IEnumerable<BankBalance> currentBankBalances;

        [TestMethod]
        public void MonthEndReconciliation_ShouldCreateSingleUseMatchingRulesForTransferToDos()
        {
            // Artifically create a transfer to do task when the reconciliation method is invoked on the LedgerBook.
            // Remember: the subject here is the LedgerService not the LedgerBook.
            ((LedgerBookTestHarness)this.testDataLedgerBook).ReconcileOverride = () =>
            {
                this.testDataToDoList.Add(
                    new TransferTask(string.Empty, true, true)
                    {
                        Reference = "sjghsh",
                        Amount = 12.22M,
                        BucketCode = StatementModelTestData.CarMtcBucket.Code
                    });
                return new ReconciliationResult { Reconciliation = new LedgerEntryLine(ReconcileDate, this.currentBankBalances), Tasks = this.testDataToDoList };
            };

            // Expect a call to the Rule service to create the single use rule for the transfer.
            this.mockRuleService.Setup(m => m.CreateNewSingleUseRule(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<decimal?>(), true))
                .Returns(new SingleUseMatchingRule(this.bucketRepo));

            Act();

            // Ensure the rule service was called with the appropriate parameters.
            this.mockRuleService.VerifyAll();
        }

        [TestInitialize]
        public void TestIntialise()
        {
            this.mockRuleService = new Mock<ITransactionRuleService>(MockBehavior.Strict);
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
            this.subject = new ReconciliationCreationManager(this.mockRuleService.Object, this.mockReconciliationConsistency.Object, new FakeLogger());

            this.testDataLedgerBook = LedgerBookTestData.TestData5(() => new LedgerBookTestHarness(new Mock<IReconciliationBuilder>().Object));

            this.mockReconciliationConsistency.Setup(m => m.EnsureConsistency(It.IsAny<LedgerBook>())).Returns(new Mock<IDisposable>().Object);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Reconcile_ShouldThrow_GivenInvalidLedgerBook()
        {
            Act(new DateTime(2012, 02, 20));
        }

        [TestMethod]
        public void Reconcile_ShouldThrow_GivenTestData1AndNoStatementModelTransactions()
        {
            this.testDataStatement = new StatementModel(new FakeLogger()) { StorageKey = "C:\\Foo.xml" };
            try
            {
                Act(new DateTime(2013, 10, 15));
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
                Act();
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
            Transaction aTransaction = this.testDataStatement.AllTransactions.First();
            PrivateAccessor.SetField(aTransaction, "budgetBucket", null);

            Act(new DateTime(2013, 9, 15));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Reconcile_ShouldThrow_GivenTestData1WithDateEqualToExistingLine()
        {
            Act(new DateTime(2013, 08, 15));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Reconcile_ShouldThrow_GivenTestData1WithDateLessThanExistingLine()
        {
            Act(new DateTime(2013, 07, 15));
        }

        [TestMethod]
        public void Reconcile_ShouldThrowWhenAutoMatchingTransactionAreMissingFromStatement_GivenTestData5()
        {
            try
            {
                ActOnTestData5(StatementModelTestData.TestData4());
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
                ActOnTestData5();
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
                Act();
            }
            catch (ValidationWarningException ex)
            {
                if (ex.Source != "3") Assert.Fail();
            }

            // Second time thru, we choose to ignore validation warnings messages we've seen before.
            try
            {
                ActOnTestData5(ignoreWarnings: true);
            }
            catch (ValidationWarningException ex)
            {
                if (ex.Source == "2") return;
            }

            Assert.Fail();
        }

        private void Act(DateTime? reconciliationDate = null, IEnumerable<BankBalance> bankBalances = null, bool ignoreWarnings = false)
        {
            this.currentBankBalances = bankBalances ?? NextReconcileBankBalance;

            var ledgerBookTestHarness = (LedgerBookTestHarness)this.testDataLedgerBook;
            if (ledgerBookTestHarness.ReconcileOverride == null)
            {
                ledgerBookTestHarness.ReconcileOverride = () => new ReconciliationResult { Reconciliation = new LedgerEntryLine(ReconcileDate, this.currentBankBalances), Tasks = this.testDataToDoList };
            }

            this.subject.MonthEndReconciliation(
                this.testDataLedgerBook,
                reconciliationDate ?? ReconcileDate,
                this.testDataBudgetContext,
                this.testDataStatement,
                ignoreWarnings,
                this.currentBankBalances.ToArray());
        }

        private void ActOnTestData5(StatementModel statementModelTestData = null, bool ignoreWarnings = false)
        {
            this.testDataLedgerBook = LedgerBookTestData.TestData5(() => new LedgerBookTestHarness(new Mock<IReconciliationBuilder>().Object));
            this.testDataBudgets = new BudgetCollection(new[] { BudgetModelTestData.CreateTestData5() });
            this.testDataBudgetContext = new BudgetCurrencyContext(this.testDataBudgets, this.testDataBudgets.CurrentActiveBudget);
            this.testDataStatement = statementModelTestData ?? StatementModelTestData.TestData5();

            Console.WriteLine("********************** BEFORE RUNNING RECONCILIATION *******************************");
            this.testDataStatement.Output(ReconcileDate.AddMonths(-1));
            this.testDataLedgerBook.Output(true);

            Act(bankBalances: new[] { new BankBalance(StatementModelTestData.ChequeAccount, 1850.5M), new BankBalance(StatementModelTestData.SavingsAccount, 1200M) },
                ignoreWarnings: ignoreWarnings);

            Console.WriteLine();
            Console.WriteLine("********************** AFTER RUNNING RECONCILIATION *******************************");
            this.testDataLedgerBook.Output(true);
        }
    }
}