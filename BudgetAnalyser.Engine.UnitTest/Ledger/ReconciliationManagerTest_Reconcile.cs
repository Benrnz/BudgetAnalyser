using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
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
        private ReconciliationManager subject;
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
            this.testDataStatement = StatementModelTestData.TestData5();
            this.testDataToDoList = new List<ToDoTask>();
            this.subject = new ReconciliationManager(this.mockRuleService.Object, this.mockReconciliationConsistency.Object, new FakeLogger());

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
        [ExpectedException(typeof(ValidationWarningException))]
        public void Reconcile_ShouldThrow_GivenTestData1AndNoStatementModelTransactions()
        {
            this.testDataStatement = new StatementModel(new FakeLogger()) { StorageKey = "C:\\Foo.xml" };
            Act(new DateTime(2013, 10, 15));
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationWarningException))]
        public void Reconcile_ShouldThrow_GivenTestData1AndUnclassifiedTransactions()
        {
            Transaction aTransaction = this.testDataStatement.AllTransactions.Last();
            PrivateAccessor.SetField(aTransaction, "budgetBucket", null);

            Act(new DateTime(2013, 9, 21));
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
        [ExpectedException(typeof(ValidationWarningException))]
        public void Reconcile_ShouldThrowWhenAutoMatchingTransactionAreMissingFromStatement_GivenTestData5()
        {
            ActOnTestData5(StatementModelTestData.TestData4());
            Assert.Fail();
        }

        private void Act(DateTime? reconciliationDate = null, IEnumerable<BankBalance> bankBalances = null)
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
                false,
                this.currentBankBalances.ToArray());
        }

        private void ActOnTestData5(StatementModel statementModelTestData = null)
        {
            this.testDataLedgerBook = LedgerBookTestData.TestData5(() => new LedgerBookTestHarness(new Mock<IReconciliationBuilder>().Object));
            this.testDataBudgets = new BudgetCollection(new[] { BudgetModelTestData.CreateTestData5() });
            this.testDataBudgetContext = new BudgetCurrencyContext(this.testDataBudgets, this.testDataBudgets.CurrentActiveBudget);
            this.testDataStatement = statementModelTestData ?? StatementModelTestData.TestData5();

            Console.WriteLine("********************** BEFORE RUNNING RECONCILIATION *******************************");
            this.testDataStatement.Output(ReconcileDate.AddMonths(-1));
            this.testDataLedgerBook.Output(true);

            Act(bankBalances: new[] { new BankBalance(StatementModelTestData.ChequeAccount, 1850.5M), new BankBalance(StatementModelTestData.SavingsAccount, 1200M) });

            Console.WriteLine();
            Console.WriteLine("********************** AFTER RUNNING RECONCILIATION *******************************");
            this.testDataLedgerBook.Output(true);
        }
    }
}