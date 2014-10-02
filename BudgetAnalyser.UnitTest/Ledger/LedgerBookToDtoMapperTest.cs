using System.Linq;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Ledger
{
    [TestClass]
    public class LedgerBookToDtoMapperTest
    {
        private LedgerBook TestData { get; set; }

        [TestMethod]
        public void DatedEntriesShouldHaveSameCount()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.Count(), result.DatedEntries.Count());
        }

        [TestMethod]
        public void FilenameShouldBeMapped()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.FileName, result.FileName);
        }

        [TestMethod]
        public void FirstDatedEntryShouldHaveSameBankBalance()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.First().TotalBankBalance, result.DatedEntries.First().BankBalance);
        }

        [TestMethod]
        public void FirstDatedEntryShouldHaveSameDate()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.First().Date, result.DatedEntries.First().Date);
        }

        [TestMethod]
        public void FirstDatedEntryShouldHaveSameNumberOfBalanceAdjustments()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.First().BankBalanceAdjustments.Count(), result.DatedEntries.First().BankBalanceAdjustments.Count());
        }

        [TestMethod]
        public void FirstDatedEntryShouldHaveSameNumberOfEntries()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.First().Entries.Count(),
                result.DatedEntries.First().Entries.Count());
        }

        [TestMethod]
        public void FirstDatedEntryShouldHaveSameRemarks()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.First().Remarks, result.DatedEntries.First().Remarks);
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstBalanceAdjustmentShouldHaveSameCredit()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.First().BankBalanceAdjustments.First().Credit,
                result.DatedEntries.First().BankBalanceAdjustments.First().Credit);
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstBalanceAdjustmentShouldHaveSameDebit()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.First().BankBalanceAdjustments.First().Debit,
                result.DatedEntries.First().BankBalanceAdjustments.First().Debit);
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstBalanceAdjustmentShouldHaveSameId()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.First().BankBalanceAdjustments.First().Id,
                result.DatedEntries.First().BankBalanceAdjustments.First().Id);
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstBalanceAdjustmentShouldHaveSameNarrative()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.First().BankBalanceAdjustments.First().Narrative,
                result.DatedEntries.First().BankBalanceAdjustments.First().Narrative);
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstEntryShouldHaveSameBalance()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.First().Entries.First().Balance,
                result.DatedEntries.First().Entries.First().Balance);
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstEntryShouldHaveSameBucketCode()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.First().Entries.First().LedgerColumn.BudgetBucket.Code,
                result.DatedEntries.First().Entries.First().BucketCode);
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstEntryShouldHaveSameNumberOfTransactions()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.First().Entries.First().Transactions.Count(),
                result.DatedEntries.First().Entries.First().Transactions.Count());
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstEntryWithFirstTransactionsShouldHaveSameCredit()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.First().Entries.First().Transactions.First().Credit,
                result.DatedEntries.First().Entries.First().Transactions.First().Credit);
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstEntryWithFirstTransactionsShouldHaveSameDebit()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.First().Entries.First().Transactions.First().Debit,
                result.DatedEntries.First().Entries.First().Transactions.First().Debit);
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstEntryWithFirstTransactionsShouldHaveSameNarrative()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.First().Entries.First().Transactions.First().Narrative,
                result.DatedEntries.First().Entries.First().Transactions.First().Narrative);
        }

        [TestMethod]
        public void ModifiedDateShouldBeMapped()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.Modified, result.Modified);
        }

        [TestMethod]
        public void NameShouldBeMapped()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.Name, result.Name);
        }

        [TestMethod]
        [Description("A test designed to break when new propperties are added to the LedgerBookDto. This is a trigger to update the mappers.")]
        public void NumberOfDataLedgerBookPropertiesShouldBe6()
        {
            int dataProperties = typeof(LedgerBookDto).CountProperties();
            Assert.AreEqual(6, dataProperties);
        }

        [TestMethod]
        [Description("A test designed to break when new propperties are added to the LedgerEntryLineDto. This is a trigger to update the mappers.")]
        public void NumberOfDataLedgerLinePropertiesShouldBe6()
        {
            int dataProperties = typeof(LedgerEntryLineDto).CountProperties();
            Assert.AreEqual(6, dataProperties);
        }

        [TestMethod]
        [Description("A test designed to break when new propperties are added to the LedgerEntryDto. This is a trigger to update the mappers.")]
        public void NumberOfDataLedgerPropertiesShouldBe3()
        {
            int dataProperties = typeof(LedgerEntryDto).CountProperties();
            Assert.AreEqual(3, dataProperties);
        }

        [TestMethod]
        [Description("A test designed to break when new propperties are added to the LedgerTransactionDto. This is a trigger to update the mappers.")]
        public void NumberOfDataLedgerTransactionPropertiesShouldBe5()
        {
            int dataProperties = typeof(LedgerTransactionDto).CountProperties();
            Assert.AreEqual(6, dataProperties);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            
            TestData = LedgerBookTestData.TestData2();
        }

        private LedgerBookDto ArrangeAndAct()
        {
            var mapper = new LedgerBookToDtoMapper();
            return mapper.Map(TestData);
        }
    }
}