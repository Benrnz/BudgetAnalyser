using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine;
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

namespace BudgetAnalyser.UnitTest.Ledger
{
    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class ReconciliationManagerTest_TransferFunds
    {
        private Mock<IReconciliationConsistency> mockReconciliationConsistency;
        private Mock<ITransactionRuleService> mockRuleService;
        private ReconciliationManager subject;

        private LedgerBook testDataLedgerBook;
        private LedgerEntryLine testDataEntryLine;

        private LedgerBucket surplusChqLedger;
        private LedgerBucket insHomeSavLedger;

        [TestInitialize]
        public void TestIntialise()
        {
            this.mockRuleService = new Mock<ITransactionRuleService>(MockBehavior.Strict);
            this.mockReconciliationConsistency = new Mock<IReconciliationConsistency>();
            this.subject = new ReconciliationManager(this.mockRuleService.Object, this.mockReconciliationConsistency.Object, new FakeLogger());

            this.testDataLedgerBook = LedgerBookTestData.TestData5(() => new LedgerBookTestHarness(new Mock<IReconciliationBuilder>().Object));
            this.testDataEntryLine = this.testDataLedgerBook.Reconciliations.First();
            this.testDataEntryLine.Unlock();

            this.surplusChqLedger = new LedgerBucket { BudgetBucket = new SurplusBucket(), StoredInAccount = StatementModelTestData.ChequeAccount };
            this.insHomeSavLedger = this.testDataLedgerBook.Ledgers.Single(l => l.BudgetBucket == StatementModelTestData.InsHomeBucket);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TransferFunds_ShouldThrow_GivenNullTransferDetails()
        {
            this.subject.TransferFunds(null, new LedgerEntryLine());
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TransferFunds_ShouldThrow_GivenNullLedgerEntryLine()
        {
            this.subject.TransferFunds(new TransferFundsCommand(), null);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TransferFunds_ShouldThrow_GivenInvalidTransferDetails()
        {
            this.subject.TransferFunds(new TransferFundsCommand(), new LedgerEntryLine());
            Assert.Fail();
        }

        [TestMethod]
        public void TransferFunds_ShouldDecreaseChqSurplus_GivenTransferFromChqSurplusToSavSavings()
        {
            var transferDetails = new TransferFundsCommand
            {
                FromLedger = this.surplusChqLedger,
                ToLedger = this.insHomeSavLedger,
                TransferAmount = 22.00M,
                Narrative = "Testing 123",
            };

            var beforeBalance = this.testDataEntryLine.SurplusBalances.First(b => b.Account == StatementModelTestData.ChequeAccount).Balance;
            this.subject.TransferFunds(transferDetails, this.testDataEntryLine);
            var afterBalance = this.testDataEntryLine.SurplusBalances.First(b => b.Account == StatementModelTestData.ChequeAccount).Balance;

            Assert.AreEqual(beforeBalance - transferDetails.TransferAmount, afterBalance);
        }

        private void Act()
        {
            //this.subject.TransferFunds();
        }
    }
}