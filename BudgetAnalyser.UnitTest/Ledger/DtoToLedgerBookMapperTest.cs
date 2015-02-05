using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Ledger
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
            catch (AutoMapperMappingException ex)
            {
                if (ex.InnerException is DataFormatException)
                {
                    return;
                }
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
            catch (AutoMapperMappingException ex)
            {
                if (ex.InnerException is ArgumentNullException)
                {
                    return;
                }
            }

            Assert.Fail();
        }

        [TestMethod]
        [Description("A test designed to break when new propperties are added to the LedgerBook. This is a trigger to update the mappers.")]
        public void NumberOfLedgerBookPropertiesShouldBe5()
        {
            int domainProperties = typeof(LedgerBook).CountProperties();
            Assert.AreEqual(5, domainProperties);
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
            Assert.AreEqual(TestData.FileName, result.FileName);
            Assert.IsNotNull(result.FileName);
        }

        [TestMethod]
        public void ShouldMapLedgerBucketAndDefaultToChequeIfNull()
        {
            TestData.Ledgers.Clear();
            LedgerBook result = ArrangeAndAct();
            Assert.IsFalse(result.Reconciliations.SelectMany(e => e.Entries).Any(e => e.LedgerBucket == null));
        }

        [TestMethod]
        public void ShouldMapLedgerBucketsAutoPopulateLedgersCollection()
        {
            TestData.Ledgers.Clear();
            LedgerBook result = ArrangeAndAct();
            foreach (LedgerBucket ledger in result.Ledgers)
            {
                Console.WriteLine("Ledger Column: {0} {1} {2}", ledger.BudgetBucket.Code, ledger.StoredInAccount.Name, ledger.GetHashCode());
            }

            Assert.AreEqual(3, result.Ledgers.Count());
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
            var mapper = new DtoToLedgerBookMapper(new InMemoryAccountTypeRepository());
            return mapper.Map(TestData);
        }
    }
}