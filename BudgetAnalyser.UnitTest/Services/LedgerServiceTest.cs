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
            PrivateAccessor.SetProperty(this.subject, nameof(this.subject.LedgerBook), new LedgerBookTestHarness(new Mock<IReconciliationBuilder>().Object));
            PrivateAccessor.SetProperty(this.subject, nameof(this.subject.ReconciliationToDoList), testTodoList);
            this.mockRuleService.Setup(m => m.CreateNewSingleUseRule(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<decimal?>(), true, 2))
                .Returns(new SingleUseMatchingRule(this.bucketRepo));

            this.subject.MonthEndReconciliation(
                ReconcileStartDate,
                new List<BankBalance>(),
                new BudgetCurrencyContext(this.budgetTestData, this.budgetTestData.CurrentActiveBudget),
                StatementModelTestData.TestData1());

            this.mockRuleService.Verify();
        }

        [TestInitialize]
        public void TestIntialise()
        {
            this.mockLedgerRepo = new Mock<ILedgerBookRepository>();
            this.mockRuleService = new Mock<ITransactionRuleService>();
            this.bucketRepo = new BucketBucketRepoAlwaysFind();
            this.subject = new LedgerService(this.mockLedgerRepo.Object, new InMemoryAccountTypeRepository(), new FakeLogger(), this.mockRuleService.Object);
        }
    }
}