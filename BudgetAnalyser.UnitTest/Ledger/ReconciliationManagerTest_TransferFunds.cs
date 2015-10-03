using System;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.UnitTest.TestData;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BudgetAnalyser.UnitTest.Ledger
{
    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class ReconciliationManagerTest_TransferFunds
    {
        private LedgerBucket insHomeSavLedger;
        private Mock<IReconciliationConsistency> mockReconciliationConsistency;
        private Mock<ITransactionRuleService> mockRuleService;
        private ReconciliationManager subject;
        private LedgerBucket surplusChqLedger;
        private LedgerEntryLine testDataEntryLine;
        private LedgerBook testDataLedgerBook;

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
        public void TransferFunds_ShouldDecreaseChqBalance_GivenTransferFromChqSurplusToSavSavings()
        {
            var transferDetails = new TransferFundsCommand
            {
                FromLedger = this.surplusChqLedger,
                ToLedger = this.insHomeSavLedger,
                TransferAmount = 22.00M,
                Narrative = "Testing 123"
            };

            decimal beforeBalance = this.testDataEntryLine.BankBalances.First(b => b.Account == StatementModelTestData.ChequeAccount).Balance
                                    + this.testDataEntryLine.BankBalanceAdjustments
                                        .Where(a => a.BankAccount == StatementModelTestData.ChequeAccount)
                                        .Sum(a => a.Amount);
            this.subject.TransferFunds(transferDetails, this.testDataEntryLine);
            decimal afterBalance = this.testDataEntryLine.BankBalances.First(b => b.Account == StatementModelTestData.ChequeAccount).Balance
                                   + this.testDataEntryLine.BankBalanceAdjustments
                                       .Where(a => a.BankAccount == StatementModelTestData.ChequeAccount)
                                       .Sum(a => a.Amount);

            Assert.AreEqual(beforeBalance - transferDetails.TransferAmount, afterBalance);
        }

        [TestMethod]
        public void TransferFunds_ShouldDecreaseChqSurplus_GivenTransferFromChqSurplusToSavSavings()
        {
            var transferDetails = new TransferFundsCommand
            {
                FromLedger = this.surplusChqLedger,
                ToLedger = this.insHomeSavLedger,
                TransferAmount = 22.00M,
                Narrative = "Testing 123"
            };

            decimal beforeBalance = this.testDataEntryLine.SurplusBalances.First(b => b.Account == StatementModelTestData.ChequeAccount).Balance;
            this.subject.TransferFunds(transferDetails, this.testDataEntryLine);
            decimal afterBalance = this.testDataEntryLine.SurplusBalances.First(b => b.Account == StatementModelTestData.ChequeAccount).Balance;

            Assert.AreEqual(beforeBalance - transferDetails.TransferAmount, afterBalance);
        }

        [TestMethod]
        public void TransferFunds_ShouldIncreaseSavBalance_GivenTransferFromChqSurplusToSavSavings()
        {
            var transferDetails = new TransferFundsCommand
            {
                FromLedger = this.surplusChqLedger,
                ToLedger = this.insHomeSavLedger,
                TransferAmount = 22.00M,
                Narrative = "Testing 123"
            };

            decimal beforeBalance = this.testDataEntryLine.BankBalances.First(b => b.Account == StatementModelTestData.SavingsAccount).Balance
                                    + this.testDataEntryLine.BankBalanceAdjustments
                                        .Where(a => a.BankAccount == StatementModelTestData.SavingsAccount)
                                        .Sum(a => a.Amount);
            this.subject.TransferFunds(transferDetails, this.testDataEntryLine);
            decimal afterBalance = this.testDataEntryLine.BankBalances.First(b => b.Account == StatementModelTestData.SavingsAccount).Balance
                                   + this.testDataEntryLine.BankBalanceAdjustments
                                       .Where(a => a.BankAccount == StatementModelTestData.SavingsAccount)
                                       .Sum(a => a.Amount);

            Assert.AreEqual(beforeBalance + transferDetails.TransferAmount, afterBalance);
        }

        [TestMethod]
        public void TransferFunds_ShouldIncreaseSavInsHome_GivenTransferFromChqSurplusToSavSavings()
        {
            var transferDetails = new TransferFundsCommand
            {
                FromLedger = this.surplusChqLedger,
                ToLedger = this.insHomeSavLedger,
                TransferAmount = 22.00M,
                Narrative = "Testing 123"
            };

            decimal beforeBalance = this.testDataEntryLine.Entries.First(e => e.LedgerBucket == this.insHomeSavLedger).Balance;
            this.subject.TransferFunds(transferDetails, this.testDataEntryLine);
            decimal afterBalance = this.testDataEntryLine.Entries.First(e => e.LedgerBucket == this.insHomeSavLedger).Balance;

            Assert.AreEqual(beforeBalance + transferDetails.TransferAmount, afterBalance);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TransferFunds_ShouldThrow_GivenInvalidTransferDetails()
        {
            this.subject.TransferFunds(new TransferFundsCommand(), new LedgerEntryLine());
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
        [ExpectedException(typeof(ArgumentNullException))]
        public void TransferFunds_ShouldThrow_GivenNullTransferDetails()
        {
            this.subject.TransferFunds(null, new LedgerEntryLine());
            Assert.Fail();
        }

        private void Act()
        {
            //this.subject.TransferFunds();
        }
    }
}