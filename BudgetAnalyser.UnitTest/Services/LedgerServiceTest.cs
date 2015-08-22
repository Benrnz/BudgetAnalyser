using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Services;
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
        private static readonly DateTime ReconcileStartDate = new DateTime(2013, 09, 15);
        private readonly BudgetCollection budgetTestData = BudgetModelTestData.CreateCollectionWith1And2();
        private IBudgetBucketRepository bucketRepo;
        private Mock<ILedgerBookRepository> mockLedgerRepo;
        private Mock<ITransactionRuleService> mockRuleService;
        private LedgerService subject;

        [TestMethod]
        public void MonthEndReconciliation_ShouldCreateSingleUseMatchingRulesForTransferToDos()
        {
            var testTodoList = new ToDoCollection();

            PrivateAccessor.SetProperty(
                this.subject, 
                nameof(this.subject.LedgerBook), 
                LedgerBookTestData.TestData5(() => new LedgerBookTestHarness(new Mock<IReconciliationBuilder>().Object)));

            ((LedgerBookTestHarness)this.subject.LedgerBook).ReconcileOverride = () =>
            {
                testTodoList.Add(
                    new TransferTask(string.Empty, true, true)
                    {
                        Reference = "sjghsh",
                        Amount = 12.22M,
                        BucketCode = StatementModelTestData.CarMtcBucket.Code,
                    });
                return new LedgerEntryLine(ReconcileStartDate, new List<BankBalance>());
            };

            PrivateAccessor.SetProperty(this.subject, nameof(this.subject.ReconciliationToDoList), testTodoList);

            this.mockRuleService.Setup(m => m.CreateNewSingleUseRule(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<decimal?>(), true))
                .Returns(new SingleUseMatchingRule(this.bucketRepo));

            this.subject.MonthEndReconciliation(
                ReconcileStartDate,
                new List<BankBalance>(),
                new BudgetCurrencyContext(this.budgetTestData, this.budgetTestData.CurrentActiveBudget),
                StatementModelTestData.TestData5());

            this.mockRuleService.VerifyAll();
        }

        [TestInitialize]
        public void TestIntialise()
        {
            this.mockLedgerRepo = new Mock<ILedgerBookRepository>();
            this.mockRuleService = new Mock<ITransactionRuleService>(MockBehavior.Strict);
            this.bucketRepo = new BucketBucketRepoAlwaysFind();
            this.subject = new LedgerService(this.mockLedgerRepo.Object, new InMemoryAccountTypeRepository(), new FakeLogger(), this.mockRuleService.Object);
        }
    }
}