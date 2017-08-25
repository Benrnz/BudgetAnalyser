using System.Linq;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.Ledger.Reconciliation;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BudgetAnalyser.Engine.UnitTest.Ledger
{
    [TestClass]
    public class LedgerBookToDtoMapperTest
    {
        private LedgerBook TestData { get; set; }

        [TestMethod]
        public void FilenameShouldBeMapped()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.StorageKey, result.StorageKey);
        }

        [TestMethod]
        public void FirstDatedEntryShouldHaveSameBankBalance()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.Reconciliations.First().TotalBankBalance, result.Reconciliations.First().BankBalance);
        }

        [TestMethod]
        public void FirstDatedEntryShouldHaveSameDate()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.Reconciliations.First().Date, result.Reconciliations.First().Date);
        }

        [TestMethod]
        public void FirstDatedEntryShouldHaveSameNumberOfBalanceAdjustments()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.Reconciliations.First().BankBalanceAdjustments.Count(), result.Reconciliations.First().BankBalanceAdjustments.Count());
        }

        [TestMethod]
        public void FirstDatedEntryShouldHaveSameNumberOfEntries()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(
                TestData.Reconciliations.First().Entries.Count(),
                result.Reconciliations.First().Entries.Count());
        }

        [TestMethod]
        public void FirstDatedEntryShouldHaveSameRemarks()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.Reconciliations.First().Remarks, result.Reconciliations.First().Remarks);
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstBalanceAdjustmentShouldHaveSameCredit()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(
                TestData.Reconciliations.First().BankBalanceAdjustments.First().Amount,
                result.Reconciliations.First().BankBalanceAdjustments.First().Amount);
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstBalanceAdjustmentShouldHaveSameDebit()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(
                TestData.Reconciliations.First().BankBalanceAdjustments.First().Amount,
                result.Reconciliations.First().BankBalanceAdjustments.First().Amount);
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstBalanceAdjustmentShouldHaveSameId()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(
                TestData.Reconciliations.First().BankBalanceAdjustments.First().Id,
                result.Reconciliations.First().BankBalanceAdjustments.First().Id);
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstBalanceAdjustmentShouldHaveSameNarrative()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(
                TestData.Reconciliations.First().BankBalanceAdjustments.First().Narrative,
                result.Reconciliations.First().BankBalanceAdjustments.First().Narrative);
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstEntryShouldHaveSameBalance()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(
                TestData.Reconciliations.First().Entries.First().Balance,
                result.Reconciliations.First().Entries.First().Balance);
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstEntryShouldHaveSameBucketCode()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(
                TestData.Reconciliations.First().Entries.First().LedgerBucket.BudgetBucket.Code,
                result.Reconciliations.First().Entries.First().BucketCode);
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstEntryShouldHaveSameNumberOfTransactions()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(
                TestData.Reconciliations.First().Entries.First().Transactions.Count(),
                result.Reconciliations.First().Entries.First().Transactions.Count());
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstEntryWithFirstTransactionsShouldHaveSameAmount()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(
                TestData.Reconciliations.First().Entries.First().Transactions.First().Amount,
                result.Reconciliations.First().Entries.First().Transactions.First().Amount);
        }

        [TestMethod]
        public void FirstDatedEntryWithFirstEntryWithFirstTransactionsShouldHaveSameNarrative()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(
                TestData.Reconciliations.First().Entries.First().Transactions.First().Narrative,
                result.Reconciliations.First().Entries.First().Transactions.First().Narrative);
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
        public void NumberOfDataLedgerBookPropertiesShouldBe7()
        {
            int dataProperties = typeof(LedgerBookDto).CountProperties();
            Assert.AreEqual(7, dataProperties);
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
        public void NumberOfDataLedgerPropertiesShouldBe4()
        {
            int dataProperties = typeof(LedgerEntryDto).CountProperties();
            Assert.AreEqual(4, dataProperties);
        }

        [TestMethod]
        [Description("A test designed to break when new propperties are added to the LedgerTransactionDto. This is a trigger to update the mappers.")]
        public void NumberOfDataLedgerTransactionPropertiesShouldBe7()
        {
            int dataProperties = typeof(LedgerTransactionDto).CountProperties();
            Assert.AreEqual(7, dataProperties);
        }

        [TestMethod]
        public void ReconciliationsShouldHaveSameCount()
        {
            LedgerBookDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.Reconciliations.Count(), result.Reconciliations.Count());
        }

        [TestInitialize]
        public void TestInitialise()
        {
            TestData = LedgerBookTestData.TestData2();
        }

        private LedgerBookDto ArrangeAndAct()
        {
            var bucketRepo = new BucketBucketRepoAlwaysFind();
            var accountRepo = new InMemoryAccountTypeRepository();
            var mapper = new Mapper_LedgerBookDto_LedgerBook(bucketRepo, accountRepo, new LedgerBucketFactory(bucketRepo, accountRepo), new LedgerTransactionFactory(), new Mock<IReconciliationBuilder>().Object);
            return mapper.ToDto(TestData);
        }
    }
}