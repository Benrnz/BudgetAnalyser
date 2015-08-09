using System;
using System.Linq;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.UnitTest.Helper;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Ledger
{
    // ReSharper disable InconsistentNaming

    [TestClass]
    public class LedgerBook_DataTest
    {
        [TestMethod]
        public void UsingTestData1_FirstLineBankBalanceEqualsLedgerBalance()
        {
            LedgerBook result = ArrangeAndAct();
            LedgerEntryLine subject = result.Reconciliations.First();
            Assert.AreEqual(subject.LedgerBalance, subject.TotalBankBalance);
        }

        [TestMethod]
        public void UsingTestData1_FirstLineHairEntryShouldHaveBalance120()
        {
            LedgerBook result = ArrangeAndAct();
            LedgerEntry subject = result.Reconciliations.First().Entries.First(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode);
            Assert.AreEqual(120, subject.Balance);
        }

        [TestMethod]
        public void UsingTestData1_FirstLineHasNoAdjustments()
        {
            LedgerBook result = ArrangeAndAct();
            Assert.AreEqual(0, result.Reconciliations.First().BankBalanceAdjustments.Count());
        }

        [TestMethod]
        public void UsingTestData1_FirstLineShouldHave3Entries()
        {
            LedgerBook result = ArrangeAndAct();
            LedgerEntryLine subject = result.Reconciliations.First();
            Assert.AreEqual(3, subject.Entries.Count());
        }

        [TestMethod]
        public void UsingTestData1_FirstLineShouldHaveBankBalance2950()
        {
            LedgerBook result = ArrangeAndAct();
            LedgerEntryLine subject = result.Reconciliations.First();
            Assert.AreEqual(2950, subject.TotalBankBalance);
        }

        [TestMethod]
        public void UsingTestData1_FirstLineShouldHaveDate20130815()
        {
            LedgerBook result = ArrangeAndAct();
            LedgerEntryLine subject = result.Reconciliations.First();
            Assert.AreEqual(new DateTime(2013, 08, 15), subject.Date);
        }

        [TestMethod]
        public void UsingTestData1_FirstLineShouldHaveSurplusOf2712()
        {
            LedgerBook result = ArrangeAndAct();
            LedgerEntryLine subject = result.Reconciliations.First();
            Assert.AreEqual(2712.97M, subject.CalculatedSurplus);
        }

        [TestMethod]
        public void UsingTestData1_LedgerCountShouldBe3()
        {
            LedgerBook result = ArrangeAndAct();
            Assert.AreEqual(3, result.Ledgers.Count());
        }

        [TestMethod]
        public void UsingTestData1_OutputDataInTabularForm()
        {
            LedgerBook result = ArrangeAndAct();
            result.Output();
        }

        [TestMethod]
        public void UsingTestData1_ShouldBeHairLedger()
        {
            LedgerBook result = ArrangeAndAct();
            Assert.IsNotNull(result.Ledgers.FirstOrDefault(l => l.BudgetBucket.Code == TestDataConstants.HairBucketCode));
        }

        [TestMethod]
        public void UsingTestData1_ShouldBePhoneLedger()
        {
            LedgerBook result = ArrangeAndAct();
            Assert.IsNotNull(result.Ledgers.FirstOrDefault(l => l.BudgetBucket.Code == TestDataConstants.PhoneBucketCode));
        }

        [TestMethod]
        public void UsingTestData1_ShouldBePowerLedger()
        {
            LedgerBook result = ArrangeAndAct();
            Assert.IsNotNull(result.Ledgers.FirstOrDefault(l => l.BudgetBucket.Code == TestDataConstants.PowerBucketCode));
        }

        [TestMethod]
        public void UsingTestData1_ShouldHave3Lines()
        {
            LedgerBook result = ArrangeAndAct();
            Assert.AreEqual(3, result.Reconciliations.Count());
        }

        private LedgerBook ArrangeAndAct()
        {
            return LedgerBookTestData.TestData1();
        }
    }

    // ReSharper restore InconsistentNaming
}