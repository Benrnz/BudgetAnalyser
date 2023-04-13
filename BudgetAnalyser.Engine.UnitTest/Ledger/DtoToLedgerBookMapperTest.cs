using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Ledger
{
    [TestClass]
    public class DtoToLedgerBookMapperTest
    {
        private LedgerBookDto TestData { get; set; }

        [TestMethod]
        public void InvalidTransactionTypeShouldThrow()
        {
            try
            {
                TestData.Reconciliations.First().Entries.Last().Transactions.First().TransactionType = "Foobar";
                ArrangeAndAct();
            }
            catch (DataFormatException)
            {
                return;
            }

            Assert.Fail();
        }

        [TestMethod]
        public void NullTransactionTypeShouldThrow()
        {
            try
            {
                TestData.Reconciliations.First().Entries.Last().Transactions.First().TransactionType = null;
                ArrangeAndAct();
            }
            catch (ArgumentNullException)
            {
                    return;
            }

            Assert.Fail();
        }

        [TestMethod]
        [Description("A test designed to break when new propperties are added to the LedgerBook. This is a trigger to update the mappers.")]
        public void NumberOfLedgerBookPropertiesShouldBe6()
        {
            int domainProperties = typeof(LedgerBook).CountProperties();
            Assert.AreEqual(6, domainProperties);
        }

        [TestMethod]
        public void ShouldMapCorrectNumberOfLedgers()
        {
            LedgerBook result = ArrangeAndAct();

            Assert.AreEqual(3, result.Ledgers.Count());
        }

        [TestMethod]
        public void ShouldMapCorrectNumberOfLineEntries()
        {
            LedgerBook result = ArrangeAndAct();
            Assert.AreEqual(TestData.Reconciliations.First().Entries.Count, result.Reconciliations.First().Entries.Count());
        }

        [TestMethod]
        public void ShouldMapCorrectNumberOfLineEntryTransactions()
        {
            LedgerBook result = ArrangeAndAct();
            Assert.AreEqual(TestData.Reconciliations.First().Entries.First().Transactions.Count, result.Reconciliations.First().Entries.First().Transactions.Count());
        }

        [TestMethod]
        public void ShouldMapCorrectNumberOfLines()
        {
            LedgerBook result = ArrangeAndAct();
            Assert.AreEqual(TestData.Reconciliations.Count, result.Reconciliations.Count());
        }

        [TestMethod]
        public void ShouldMapFileName()
        {
            LedgerBook result = ArrangeAndAct();
            Assert.AreEqual(TestData.StorageKey, result.StorageKey);
            Assert.IsNotNull(result.StorageKey);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ShouldThrow_GivenLedgerBucketListIsEmptyInDto()
        {
            TestData.Ledgers.Clear();
            ArrangeAndAct();
            Assert.Fail();
        }

        [TestMethod]
        public void ShouldMapLedgerBucketsOnLedgerEntriesWithAccountNotNull()
        {
            LedgerBook result = ArrangeAndAct();
            Assert.IsFalse(result.Reconciliations.SelectMany(e => e.Entries).Any(e => e.LedgerBucket == null));
        }

        [TestMethod]
        public void ShouldMapLedgerBucketsWithNoDuplicateInstances()
        {
            LedgerBook result = ArrangeAndAct();
            IEnumerable<LedgerBucket> ledgerBuckets = result.Reconciliations
                .SelectMany(e => e.Entries)
                .Select(e => e.LedgerBucket)
                .Union(result.Ledgers)
                .Distinct();

            Assert.AreEqual(3, ledgerBuckets.Count());
        }

        [TestMethod]
        public void ShouldMapLineBalanceAdjustments()
        {
            TestData = LedgerBookDtoTestData.TestData2();
            LedgerBook result = ArrangeAndAct();
            LedgerEntryLine subject = result.Reconciliations.First();

            Assert.AreEqual(TestData.Reconciliations.First().BankBalanceAdjustments.Sum(a => a.Amount), subject.TotalBalanceAdjustments);
            Assert.AreNotEqual(0, subject.BankBalanceAdjustments.Count());
        }

        [TestMethod]
        public void ShouldMapLineBankBalance()
        {
            LedgerBook result = ArrangeAndAct();
            LedgerEntryLine subject = result.Reconciliations.First();
            Assert.AreEqual(TestData.Reconciliations.First().BankBalance, subject.TotalBankBalance);
            Assert.AreNotEqual(0, subject.TotalBankBalance);
        }

        [TestMethod]
        public void ShouldMapLineDate()
        {
            LedgerBook result = ArrangeAndAct();
            DateTime subject = result.Reconciliations.First().Date;
            Assert.AreEqual(TestData.Reconciliations.First().Date, subject);
            Assert.AreNotEqual(DateTime.MinValue, subject);
        }

        [TestMethod]
        public void ShouldMapLineEntryBucketCode()
        {
            LedgerBook result = ArrangeAndAct();
            string subject = result.Reconciliations.First().Entries.First().LedgerBucket.BudgetBucket.Code;
            Assert.AreEqual(TestData.Reconciliations.First().Entries.First().BucketCode, subject);
            Assert.IsNotNull(subject);
        }

        [TestMethod]
        public void ShouldMapLineEntryTransactionAmount()
        {
            LedgerBook result = ArrangeAndAct();
            decimal subject = result.Reconciliations.First().Entries.First().Transactions.First().Amount;
            Assert.AreEqual(TestData.Reconciliations.First().Entries.First().Transactions.First().Amount, subject);
            Assert.AreNotEqual(0, subject);
        }

        [TestMethod]
        public void ShouldMapLineEntryTransactionId()
        {
            LedgerBook result = ArrangeAndAct();
            Guid subject = result.Reconciliations.First().Entries.First().Transactions.First().Id;
            Assert.AreEqual(TestData.Reconciliations.First().Entries.First().Transactions.First().Id, subject);
            Assert.AreNotEqual(Guid.Empty, subject);
        }

        [TestMethod]
        public void ShouldMapLineEntryTransactionNarrative()
        {
            LedgerBook result = ArrangeAndAct();
            string subject = result.Reconciliations.First().Entries.First().Transactions.First().Narrative;
            Assert.AreEqual(TestData.Reconciliations.First().Entries.First().Transactions.First().Narrative, subject);
            Assert.IsNotNull(subject);
        }

        [TestMethod]
        public void ShouldMapLineEntryTransactionType()
        {
            LedgerBook result = ArrangeAndAct();
            string subject = result.Reconciliations.First().Entries.First().Transactions.First().GetType().FullName;
            Assert.AreEqual(TestData.Reconciliations.First().Entries.First().Transactions.First().TransactionType, subject);
            Assert.IsNotNull(subject);
        }

        [TestMethod]
        public void ShouldMapLineRemarks()
        {
            LedgerBook result = ArrangeAndAct();
            string subject = result.Reconciliations.First().Remarks;
            Assert.AreEqual(TestData.Reconciliations.First().Remarks, subject);
            Assert.IsNotNull(subject);
        }

        [TestMethod]
        public void ShouldMapModifiedDate()
        {
            LedgerBook result = ArrangeAndAct();
            Assert.AreEqual(TestData.Modified, result.Modified);
            Assert.AreNotEqual(DateTime.MinValue, result.Modified);
        }

        [TestMethod]
        public void ShouldMapName()
        {
            LedgerBook result = ArrangeAndAct();
            Assert.AreEqual(TestData.Name, result.Name);
            Assert.IsNotNull(result.Name);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            TestData = LedgerBookDtoTestData.TestData1();
        }

        private LedgerBook ArrangeAndAct()
        {
            var bucketRepo = new BucketBucketRepoAlwaysFind();
            var accountRepo = new InMemoryAccountTypeRepository();
            var mapper = new Mapper_LedgerBookDto_LedgerBook(bucketRepo, accountRepo, new LedgerBucketFactory(bucketRepo, accountRepo), new LedgerTransactionFactory());
            return mapper.ToModel(TestData);
        }
    }
}