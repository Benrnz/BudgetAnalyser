using System;
using System.Linq;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Ledger
{
    [TestClass]
    public class LedgerBookToDataLedgerBookMapperTest
    {
        private LedgerBook TestData { get; set; }

        [TestInitialize]
        public void TestInitialise()
        {
            this.TestData = LedgerBookTestData.TestData2();
        }

        [TestMethod]
        [Description("A test designed to break when new propperties are added to the DataLedgerBook. This is a trigger to update the mappers.")]
        public void NumberOfDataLedgerBookPropertiesShouldBe5()
        {
            var dataProperties = CountProperties(typeof (DataLedgerBook));
            Assert.AreEqual(5, dataProperties);
        }

        [TestMethod]
        [Description("A test designed to break when new propperties are added to the DataLedgerEntryLine. This is a trigger to update the mappers.")]
        public void NumberOfDataLedgerLinePropertiesShouldBe6()
        {
            var dataProperties = CountProperties(typeof(DataLedgerEntryLine));
            Assert.AreEqual(6, dataProperties);
        }

        [TestMethod]
        [Description("A test designed to break when new propperties are added to the DataLedgerEntry. This is a trigger to update the mappers.")]
        public void NumberOfDataLedgerPropertiesShouldBe3()
        {
            var dataProperties = CountProperties(typeof(DataLedgerEntry));
            Assert.AreEqual(3, dataProperties);
        }

        [TestMethod]
        [Description("A test designed to break when new propperties are added to the DataLedgerTransaction. This is a trigger to update the mappers.")]
        public void NumberOfDataLedgerTransactionPropertiesShouldBe5()
        {
            var dataProperties = CountProperties(typeof(DataLedgerTransaction));
            Assert.AreEqual(6, dataProperties);
        }

        [TestMethod]
        public void FilenameShouldBeMapped()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(this.TestData.FileName, result.FileName);
        }

        [TestMethod]
        public void ModifiedDateShouldBeMapped()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(this.TestData.Modified, result.Modified);
        }

        [TestMethod]
        public void NameShouldBeMapped()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(this.TestData.Name, result.Name);
        }

        [TestMethod]
        public void DatedEntriesShouldHaveSameCount()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(this.TestData.DatedEntries.Count(), result.DatedEntries.Count());
        }

        [TestMethod]
        public void FirstDatedEntryShouldHaveSameBankBalance()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(this.TestData.DatedEntries.First().TotalBankBalance, result.DatedEntries.First().BankBalance);
        }

        [TestMethod]
        public void FirstDatedEntryShouldHaveSameDate()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(this.TestData.DatedEntries.First().Date, result.DatedEntries.First().Date);
        }

        [TestMethod]
        public void FirstDatedEntryShouldHaveSameRemarks()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(this.TestData.DatedEntries.First().Remarks, result.DatedEntries.First().Remarks);
        }

        [TestMethod]
        public void FirstDatedEntryShouldHaveSameNumberOfBalanceAdjustments()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(this.TestData.DatedEntries.First().BankBalanceAdjustments.Count(), result.DatedEntries.First().BankBalanceAdjustments.Count());
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstBalanceAdjustmentShouldHaveSameCredit()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(this.TestData.DatedEntries.First().BankBalanceAdjustments.First().Credit, 
                result.DatedEntries.First().BankBalanceAdjustments.First().Credit);
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstBalanceAdjustmentShouldHaveSameDebit()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(this.TestData.DatedEntries.First().BankBalanceAdjustments.First().Debit,
                result.DatedEntries.First().BankBalanceAdjustments.First().Debit);
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstBalanceAdjustmentShouldHaveSameId()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(this.TestData.DatedEntries.First().BankBalanceAdjustments.First().Id,
                result.DatedEntries.First().BankBalanceAdjustments.First().Id);
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstBalanceAdjustmentShouldHaveSameNarrative()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(this.TestData.DatedEntries.First().BankBalanceAdjustments.First().Narrative,
                result.DatedEntries.First().BankBalanceAdjustments.First().Narrative);
        }

        [TestMethod]
        public void FirstDatedEntryShouldHaveSameNumberOfEntries()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(this.TestData.DatedEntries.First().Entries.Count(),
                result.DatedEntries.First().Entries.Count());
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstEntryShouldHaveSameBalance()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(this.TestData.DatedEntries.First().Entries.First().Balance,
                result.DatedEntries.First().Entries.First().Balance);
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstEntryShouldHaveSameBucketCode()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(this.TestData.DatedEntries.First().Entries.First().LedgerColumn.BudgetBucket.Code,
                result.DatedEntries.First().Entries.First().BucketCode);
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstEntryShouldHaveSameNumberOfTransactions()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(this.TestData.DatedEntries.First().Entries.First().Transactions.Count(),
                result.DatedEntries.First().Entries.First().Transactions.Count());
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstEntryWithFirstTransactionsShouldHaveSameCredit()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(this.TestData.DatedEntries.First().Entries.First().Transactions.First().Credit,
                result.DatedEntries.First().Entries.First().Transactions.First().Credit);
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstEntryWithFirstTransactionsShouldHaveSameDebit()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(this.TestData.DatedEntries.First().Entries.First().Transactions.First().Debit,
                result.DatedEntries.First().Entries.First().Transactions.First().Debit);
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstEntryWithFirstTransactionsShouldHaveSameNarrative()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(this.TestData.DatedEntries.First().Entries.First().Transactions.First().Narrative,
                result.DatedEntries.First().Entries.First().Transactions.First().Narrative);
        }

        private DataLedgerBook ArrangeAndAct()
        {
            var mapper = new LedgerDomainToDataMapper();
            return mapper.Map(this.TestData);
        }

        private int CountProperties(Type type)
        {
            var properties = type.GetProperties();
            properties.ToList().ForEach(p => Console.WriteLine(p.Name));
            return properties.Length;
        }
    }
}
