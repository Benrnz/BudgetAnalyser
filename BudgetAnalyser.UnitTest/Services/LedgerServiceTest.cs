using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.UnitTest.Helper;
using BudgetAnalyser.UnitTest.TestData;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rees.TestUtilities;

namespace BudgetAnalyser.UnitTest.Services
{
    [TestClass]
    public class LedgerServiceTest
    {
        private static readonly DateTime ReconcileDate = new DateTime(2013, 09, 15);
        private static readonly IEnumerable<BankBalance> NextReconcileBankBalance = new[] { new BankBalance(StatementModelTestData.ChequeAccount, 2050M) };

        private IBudgetBucketRepository bucketRepo;
        private Mock<ILedgerBookRepository> mockLedgerRepo;
        private Mock<ITransactionRuleService> mockRuleService;
        private LedgerService subject;
        private IBudgetCurrencyContext testDataBudgetContext;
        private BudgetCollection testDataBudgets;
        private StatementModel testDataStatement;
        private ToDoCollection testDataToDoList;
        private Mock<IReconciliationConsistency> mockReconciliationConsistency;

        [TestMethod]
        public void MonthEndReconciliation_ShouldCreateSingleUseMatchingRulesForTransferToDos()
        {
            // Artifically create a transfer to do task when the reconciliation method is invoked on the LedgerBook.
            // Remember: the subject here is the LedgerService not the LedgerBook.
            ((LedgerBookTestHarness)this.subject.LedgerBook).ReconcileOverride = () =>
            {
                this.testDataToDoList.Add(
                    new TransferTask(string.Empty, true, true)
                    {
                        Reference = "sjghsh",
                        Amount = 12.22M,
                        BucketCode = StatementModelTestData.CarMtcBucket.Code
                    });
                return new LedgerEntryLine(ReconcileDate, new List<BankBalance>());
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
            this.mockLedgerRepo = new Mock<ILedgerBookRepository>();
            this.mockRuleService = new Mock<ITransactionRuleService>(MockBehavior.Strict);
            this.mockReconciliationConsistency = new Mock<IReconciliationConsistency>();
            this.bucketRepo = new BucketBucketRepoAlwaysFind();
            this.testDataBudgets = BudgetModelTestData.CreateCollectionWith1And2();
            this.testDataBudgetContext = new BudgetCurrencyContext(this.testDataBudgets, this.testDataBudgets.CurrentActiveBudget);
            this.testDataStatement = StatementModelTestData.TestData5();
            this.testDataToDoList = new ToDoCollection();
            this.subject = new LedgerService(this.mockLedgerRepo.Object, new InMemoryAccountTypeRepository(), new FakeLogger(), this.mockRuleService.Object, this.mockReconciliationConsistency.Object);

            // Inject the preconfigured LedgerBook into the Ledger Service.
            PrivateAccessor.SetProperty(
                this.subject,
                nameof(this.subject.LedgerBook),
                LedgerBookTestData.TestData5(() => new LedgerBookTestHarness(new Mock<IReconciliationBuilder>().Object)));

            // Inject the preconfigured to do list into the Ledger Service.
            PrivateAccessor.SetProperty(this.subject, nameof(this.subject.ReconciliationToDoList), this.testDataToDoList);

            this.mockReconciliationConsistency.Setup(m => m.EnsureConsistency(It.IsAny<LedgerBook>())).Returns(new Mock<IDisposable>().Object);
        }

        private void Act(DateTime? reconciliationDate = null, IEnumerable<BankBalance> bankBalances = null)
        {
            var balances = bankBalances ?? NextReconcileBankBalance;

            var ledgerBookTestHarness = (LedgerBookTestHarness)this.subject.LedgerBook;
            if (ledgerBookTestHarness.ReconcileOverride == null)
            {
                ledgerBookTestHarness.ReconcileOverride = () => new LedgerEntryLine(ReconcileDate, balances);
            }

            this.subject.MonthEndReconciliation(
                reconciliationDate ?? ReconcileDate,
                balances,
                this.testDataBudgetContext,
                this.testDataStatement);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UsingInvalidLedgerBook_Reconcile_ShouldThrow()
        {
            Act(new DateTime(2012, 02, 20));
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationWarningException))]
        public void UsingTestData1AndNoStatementModelTransactions_Reconcile_ShouldThrow()
        {
            this.testDataStatement = new StatementModel(new FakeLogger());
            Act(new DateTime(2013, 10, 15));
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationWarningException))]
        public void UsingTestData1AndUnclassifiedTransactions_Reconcile_ShouldThrow()
        {
            Transaction aTransaction = this.testDataStatement.AllTransactions.Last();
            PrivateAccessor.SetField(aTransaction, "budgetBucket", null);

            Act(new DateTime(2013, 9, 21));
        }

        [TestMethod]
        public void UsingTestData1AndUnclassifiedTransactionsOutsideReconPeriod_Reconcile_ShouldNotThrow()
        {
            Transaction aTransaction = this.testDataStatement.AllTransactions.First();
            PrivateAccessor.SetField(aTransaction, "budgetBucket", null);

            Act(new DateTime(2013, 9, 15));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UsingTestData1WithDateEqualToExistingLine_Reconcile_ShouldThrow()
        {
            Act(new DateTime(2013, 08, 15));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UsingTestData1WithDateLessThanExistingLine_Reconcile_ShouldThrow()
        {
            Act(new DateTime(2013, 07, 15));
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationWarningException))]
        public void UsingTestData5_Reconcile_ShouldThrowWhenAutoMatchingTransactionAreMissingFromStatement()
        {
            ActOnTestData5(StatementModelTestData.TestData4());
            Assert.Fail();
        }

        private LedgerBook testDataLedgerBook;

        private void ActOnTestData5(StatementModel statementModelTestData = null)
        {
            this.testDataLedgerBook = LedgerBookTestData.TestData5();
            this.testDataBudgets = new BudgetCollection(new [] { BudgetModelTestData.CreateTestData5() });
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