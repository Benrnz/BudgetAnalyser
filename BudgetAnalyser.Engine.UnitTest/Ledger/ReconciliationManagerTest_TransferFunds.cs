using System;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Reconciliation;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BudgetAnalyser.Engine.UnitTest.Ledger
{
    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class ReconciliationManagerTest_TransferFunds
    {
        private LedgerBucket insHomeSavLedger;
        private Mock<IBudgetBucketRepository> mockBucketRepo;
        private Mock<IReconciliationConsistency> mockReconciliationConsistency;
        private Mock<ITransactionRuleService> mockRuleService;
        private LedgerBucket phNetChqLedger;
        private ReconciliationCreationManager subject;
        private LedgerBucket surplusChqLedger;
        private LedgerEntryLine testDataEntryLine;
        private LedgerBook testDataLedgerBook;

        [TestInitialize]
        public void TestIntialise()
        {
            this.mockBucketRepo = new Mock<IBudgetBucketRepository>();
            this.mockRuleService = new Mock<ITransactionRuleService>();
            this.mockReconciliationConsistency = new Mock<IReconciliationConsistency>();
            this.subject = new ReconciliationCreationManager(this.mockRuleService.Object, this.mockReconciliationConsistency.Object, new FakeLogger());

            this.testDataLedgerBook = LedgerBookTestData.TestData5(() => new LedgerBookTestHarness(new Mock<IReconciliationBuilder>().Object));
            this.testDataEntryLine = this.testDataLedgerBook.Reconciliations.First();
            this.testDataEntryLine.Unlock();

            this.surplusChqLedger = new SurplusLedger { StoredInAccount = StatementModelTestData.ChequeAccount };
            this.insHomeSavLedger = this.testDataLedgerBook.Ledgers.Single(l => l.BudgetBucket == StatementModelTestData.InsHomeBucket);
            this.phNetChqLedger = this.testDataLedgerBook.Ledgers.Single(l => l.BudgetBucket == StatementModelTestData.PhoneBucket);
        }

        [TestMethod]
        public void TransferFunds_ShouldCreateAutoMatchingRule_GivenTransferFromChqSurplusToSavingsInsHome()
        {
            var transferFundsData = new TransferFundsCommand
            {
                AutoMatchingReference = "FooTest12345",
                BankTransferRequired = true,
                FromLedger = LedgerBookTestData.SurplusLedger,
                Narrative = "Save excess for November",
                ToLedger = LedgerBookTestData.HouseInsLedgerSavingsAccount,
                TransferAmount = 200M
            };

            this.mockRuleService.Setup(m => m.CreateNewSingleUseRule(transferFundsData.FromLedger.BudgetBucket.Code, null, new[] { "FooTest12345" }, null, -200, true))
                .Returns(new SingleUseMatchingRule(this.mockBucketRepo.Object));
            this.mockRuleService.Setup(m => m.CreateNewSingleUseRule(transferFundsData.ToLedger.BudgetBucket.Code, null, new[] { "FooTest12345" }, null, 200, true))
                .Returns(new SingleUseMatchingRule(this.mockBucketRepo.Object));

            this.subject.TransferFunds(this.testDataLedgerBook, transferFundsData, this.testDataEntryLine);

            this.mockRuleService.VerifyAll();
        }

        [TestMethod]
        public void TransferFunds_ShouldDecreaseChqBalance_GivenTransferFromChqSurplusToSavingsInsHome()
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
            this.subject.TransferFunds(this.testDataLedgerBook, transferDetails, this.testDataEntryLine);
            decimal afterBalance = this.testDataEntryLine.BankBalances.First(b => b.Account == StatementModelTestData.ChequeAccount).Balance
                                   + this.testDataEntryLine.BankBalanceAdjustments
                                       .Where(a => a.BankAccount == StatementModelTestData.ChequeAccount)
                                       .Sum(a => a.Amount);

            Assert.AreEqual(beforeBalance - transferDetails.TransferAmount, afterBalance);
        }

        [TestMethod]
        public void TransferFunds_ShouldDecreaseChqSurplus_GivenTransferFromChqSurplusToChqCarMtc()
        {
            var transferDetails = new TransferFundsCommand
            {
                FromLedger = this.surplusChqLedger,
                ToLedger = this.phNetChqLedger,
                TransferAmount = 22.00M,
                Narrative = "Testing 123"
            };

            decimal beforeBalance = this.testDataEntryLine.SurplusBalances.First(b => b.Account == StatementModelTestData.ChequeAccount).Balance;
            this.subject.TransferFunds(this.testDataLedgerBook, transferDetails, this.testDataEntryLine);
            decimal afterBalance = this.testDataEntryLine.SurplusBalances.First(b => b.Account == StatementModelTestData.ChequeAccount).Balance;

            Assert.AreEqual(beforeBalance - transferDetails.TransferAmount, afterBalance);
        }

        [TestMethod]
        public void TransferFunds_ShouldDecreaseChqSurplus_GivenTransferFromChqSurplusToSavingsInsHome()
        {
            var transferDetails = new TransferFundsCommand
            {
                FromLedger = this.surplusChqLedger,
                ToLedger = this.insHomeSavLedger,
                TransferAmount = 22.00M,
                Narrative = "Testing 123"
            };

            decimal beforeBalance = this.testDataEntryLine.SurplusBalances.First(b => b.Account == StatementModelTestData.ChequeAccount).Balance;
            this.subject.TransferFunds(this.testDataLedgerBook, transferDetails, this.testDataEntryLine);
            decimal afterBalance = this.testDataEntryLine.SurplusBalances.First(b => b.Account == StatementModelTestData.ChequeAccount).Balance;

            Assert.AreEqual(beforeBalance - transferDetails.TransferAmount, afterBalance);
        }

        [TestMethod]
        public void TransferFunds_ShouldIncreaseChqCarMtc_GivenTransferFromChqSurplusToChqCarMtc()
        {
            var transferDetails = new TransferFundsCommand
            {
                FromLedger = this.surplusChqLedger,
                ToLedger = this.phNetChqLedger,
                TransferAmount = 22.00M,
                Narrative = "Testing 123"
            };

            decimal beforeBalance = this.testDataEntryLine.Entries.First(e => e.LedgerBucket == this.phNetChqLedger).Balance;
            this.subject.TransferFunds(this.testDataLedgerBook, transferDetails, this.testDataEntryLine);
            decimal afterBalance = this.testDataEntryLine.Entries.First(e => e.LedgerBucket == this.phNetChqLedger).Balance;

            Assert.AreEqual(beforeBalance + transferDetails.TransferAmount, afterBalance);
        }

        [TestMethod]
        public void TransferFunds_ShouldIncreaseSavBalance_GivenTransferFromChqSurplusToSavingsInsHome()
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
            this.subject.TransferFunds(this.testDataLedgerBook, transferDetails, this.testDataEntryLine);
            decimal afterBalance = this.testDataEntryLine.BankBalances.First(b => b.Account == StatementModelTestData.SavingsAccount).Balance
                                   + this.testDataEntryLine.BankBalanceAdjustments
                                       .Where(a => a.BankAccount == StatementModelTestData.SavingsAccount)
                                       .Sum(a => a.Amount);

            Assert.AreEqual(beforeBalance + transferDetails.TransferAmount, afterBalance);
        }

        [TestMethod]
        public void TransferFunds_ShouldIncreaseSavInsHome_GivenTransferFromChqSurplusToSavingsInsHome()
        {
            var transferDetails = new TransferFundsCommand
            {
                FromLedger = this.surplusChqLedger,
                ToLedger = this.insHomeSavLedger,
                TransferAmount = 22.00M,
                Narrative = "Testing 123"
            };

            decimal beforeBalance = this.testDataEntryLine.Entries.First(e => e.LedgerBucket == this.insHomeSavLedger).Balance;
            this.subject.TransferFunds(this.testDataLedgerBook, transferDetails, this.testDataEntryLine);
            decimal afterBalance = this.testDataEntryLine.Entries.First(e => e.LedgerBucket == this.insHomeSavLedger).Balance;

            Assert.AreEqual(beforeBalance + transferDetails.TransferAmount, afterBalance);
        }

        [TestMethod]
        public void TransferFunds_ShouldNotCreateAutoMatchingRule_GivenTransferFromChqSurplusToChqHairCut()
        {
            var transferFundsData = new TransferFundsCommand
            {
                AutoMatchingReference = "FooTest12345",
                BankTransferRequired = false,
                FromLedger = LedgerBookTestData.SurplusLedger,
                Narrative = "Save excess for November",
                ToLedger = LedgerBookTestData.HairLedger,
                TransferAmount = 400M
            };

            var success = true;
            this.mockRuleService.Setup(m => m.CreateNewSingleUseRule(It.IsAny<string>(), null, new[] { "FooTest12345" }, null, It.IsAny<decimal>(), true))
                .Returns(new SingleUseMatchingRule(this.mockBucketRepo.Object))
                .Callback(() => success = false);

            this.subject.TransferFunds(this.testDataLedgerBook, transferFundsData, this.testDataEntryLine);

            Assert.IsTrue(success);
        }


        [TestMethod]
        public void TransferFunds_ShouldNotDecreaseChqBalance_GivenTransferFromChqSurplusToChqCarMtc()
        {
            var transferDetails = new TransferFundsCommand
            {
                FromLedger = this.surplusChqLedger,
                ToLedger = this.phNetChqLedger,
                TransferAmount = 22.00M,
                Narrative = "Testing 123"
            };

            decimal beforeBalance = this.testDataEntryLine.BankBalances.First(b => b.Account == StatementModelTestData.ChequeAccount).Balance
                                    + this.testDataEntryLine.BankBalanceAdjustments
                                        .Where(a => a.BankAccount == StatementModelTestData.ChequeAccount)
                                        .Sum(a => a.Amount);
            this.subject.TransferFunds(this.testDataLedgerBook, transferDetails, this.testDataEntryLine);
            decimal afterBalance = this.testDataEntryLine.BankBalances.First(b => b.Account == StatementModelTestData.ChequeAccount).Balance
                                   + this.testDataEntryLine.BankBalanceAdjustments
                                       .Where(a => a.BankAccount == StatementModelTestData.ChequeAccount)
                                       .Sum(a => a.Amount);

            Assert.AreEqual(beforeBalance, afterBalance);
        }

        [TestMethod]
        public void TransferFunds_ShouldNotIncreaseSavBalance_GivenTransferFromChqSurplusToChqCarMtc()
        {
            var transferDetails = new TransferFundsCommand
            {
                FromLedger = this.surplusChqLedger,
                ToLedger = this.phNetChqLedger,
                TransferAmount = 22.00M,
                Narrative = "Testing 123"
            };

            decimal beforeBalance = this.testDataEntryLine.BankBalances.First(b => b.Account == StatementModelTestData.SavingsAccount).Balance
                                    + this.testDataEntryLine.BankBalanceAdjustments
                                        .Where(a => a.BankAccount == StatementModelTestData.SavingsAccount)
                                        .Sum(a => a.Amount);
            this.subject.TransferFunds(this.testDataLedgerBook, transferDetails, this.testDataEntryLine);
            decimal afterBalance = this.testDataEntryLine.BankBalances.First(b => b.Account == StatementModelTestData.SavingsAccount).Balance
                                   + this.testDataEntryLine.BankBalanceAdjustments
                                       .Where(a => a.BankAccount == StatementModelTestData.SavingsAccount)
                                       .Sum(a => a.Amount);

            Assert.AreEqual(beforeBalance, afterBalance);
        }


        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TransferFunds_ShouldThrow_GivenInvalidTransferDetails()
        {
            this.subject.TransferFunds(this.testDataLedgerBook, new TransferFundsCommand(), new LedgerEntryLine());
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TransferFunds_ShouldThrow_GivenNullLedgerEntryLine()
        {
            this.subject.TransferFunds(this.testDataLedgerBook, new TransferFundsCommand(), null);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TransferFunds_ShouldThrow_GivenNullTransferDetails()
        {
            this.subject.TransferFunds(this.testDataLedgerBook, null, new LedgerEntryLine());
            Assert.Fail();
        }
    }
}