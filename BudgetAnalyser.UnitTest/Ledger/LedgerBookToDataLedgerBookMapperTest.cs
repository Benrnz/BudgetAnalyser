using System;
using System.Linq;
using System.Reflection;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Ledger
{
    [TestClass]
    public class LedgerBookToDataLedgerBookMapperTest
    {
        private LedgerBook TestData { get; set; }

        [TestMethod]
        public void DatedEntriesShouldHaveSameCount()
        {
            DataLedgerBook result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.Count(), result.DatedEntries.Count());
        }

        [TestMethod]
        public void FilenameShouldBeMapped()
        {
            DataLedgerBook result = ArrangeAndAct();
            Assert.AreEqual(TestData.FileName, result.FileName);
        }

        [TestMethod]
        public void FirstDatedEntryShouldHaveSameBankBalance()
        {
            DataLedgerBook result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.First().TotalBankBalance, result.DatedEntries.First().BankBalance);
        }

        [TestMethod]
        public void FirstDatedEntryShouldHaveSameDate()
        {
            DataLedgerBook result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.First().Date, result.DatedEntries.First().Date);
        }

        [TestMethod]
        public void FirstDatedEntryShouldHaveSameNumberOfBalanceAdjustments()
        {
            DataLedgerBook result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.First().BankBalanceAdjustments.Count(), result.DatedEntries.First().BankBalanceAdjustments.Count());
        }

        [TestMethod]
        public void FirstDatedEntryShouldHaveSameNumberOfEntries()
        {
            DataLedgerBook result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.First().Entries.Count(),
                result.DatedEntries.First().Entries.Count());
        }

        [TestMethod]
        public void FirstDatedEntryShouldHaveSameRemarks()
        {
            DataLedgerBook result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.First().Remarks, result.DatedEntries.First().Remarks);
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstBalanceAdjustmentShouldHaveSameCredit()
        {
            DataLedgerBook result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.First().BankBalanceAdjustments.First().Credit,
                result.DatedEntries.First().BankBalanceAdjustments.First().Credit);
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstBalanceAdjustmentShouldHaveSameDebit()
        {
            DataLedgerBook result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.First().BankBalanceAdjustments.First().Debit,
                result.DatedEntries.First().BankBalanceAdjustments.First().Debit);
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstBalanceAdjustmentShouldHaveSameId()
        {
            DataLedgerBook result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.First().BankBalanceAdjustments.First().Id,
                result.DatedEntries.First().BankBalanceAdjustments.First().Id);
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstBalanceAdjustmentShouldHaveSameNarrative()
        {
            DataLedgerBook result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.First().BankBalanceAdjustments.First().Narrative,
                result.DatedEntries.First().BankBalanceAdjustments.First().Narrative);
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstEntryShouldHaveSameBalance()
        {
            DataLedgerBook result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.First().Entries.First().Balance,
                result.DatedEntries.First().Entries.First().Balance);
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstEntryShouldHaveSameBucketCode()
        {
            DataLedgerBook result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.First().Entries.First().LedgerColumn.BudgetBucket.Code,
                result.DatedEntries.First().Entries.First().BucketCode);
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstEntryShouldHaveSameNumberOfTransactions()
        {
            DataLedgerBook result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.First().Entries.First().Transactions.Count(),
                result.DatedEntries.First().Entries.First().Transactions.Count());
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstEntryWithFirstTransactionsShouldHaveSameCredit()
        {
            DataLedgerBook result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.First().Entries.First().Transactions.First().Credit,
                result.DatedEntries.First().Entries.First().Transactions.First().Credit);
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstEntryWithFirstTransactionsShouldHaveSameDebit()
        {
            DataLedgerBook result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.First().Entries.First().Transactions.First().Debit,
                result.DatedEntries.First().Entries.First().Transactions.First().Debit);
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstEntryWithFirstTransactionsShouldHaveSameNarrative()
        {
            DataLedgerBook result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.First().Entries.First().Transactions.First().Narrative,
                result.DatedEntries.First().Entries.First().Transactions.First().Narrative);
        }

        [TestMethod]
        public void ModifiedDateShouldBeMapped()
        {
            DataLedgerBook result = ArrangeAndAct();
            Assert.AreEqual(TestData.Modified, result.Modified);
        }

        [TestMethod]
        public void NameShouldBeMapped()
        {
            DataLedgerBook result = ArrangeAndAct();
            Assert.AreEqual(TestData.Name, result.Name);
        }

        [TestMethod]
        [Description("A test designed to break when new propperties are added to the DataLedgerBook. This is a trigger to update the mappers.")]
        public void NumberOfDataLedgerBookPropertiesShouldBe5()
        {
            int dataProperties = typeof(DataLedgerBook).CountProperties();
            Assert.AreEqual(5, dataProperties);
        }

        [TestMethod]
        [Description("A test designed to break when new propperties are added to the DataLedgerEntryLine. This is a trigger to update the mappers.")]
        public void NumberOfDataLedgerLinePropertiesShouldBe6()
        {
            int dataProperties = typeof(DataLedgerEntryLine).CountProperties();
            Assert.AreEqual(6, dataProperties);
        }

        [TestMethod]
        [Description("A test designed to break when new propperties are added to the DataLedgerEntry. This is a trigger to update the mappers.")]
        public void NumberOfDataLedgerPropertiesShouldBe3()
        {
            int dataProperties = typeof(DataLedgerEntry).CountProperties();
            Assert.AreEqual(3, dataProperties);
        }

        [TestMethod]
        [Description("A test designed to break when new propperties are added to the DataLedgerTransaction. This is a trigger to update the mappers.")]
        public void NumberOfDataLedgerTransactionPropertiesShouldBe5()
        {
            int dataProperties = typeof(DataLedgerTransaction).CountProperties();
            Assert.AreEqual(6, dataProperties);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            TestData = LedgerBookTestData.TestData2();
        }

        private DataLedgerBook ArrangeAndAct()
        {
            var mapper = new LedgerDomainToDataMapper();
            return mapper.Map(TestData);
        }
    }
}