using System;
using System.Linq;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.UnitTest.TestData;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BudgetAnalyser.UnitTest.Ledger
{
    [TestClass]
    public class DtoToLedgerBookMapperTest
    {
        private BudgetBucket CarMtcBucket { get; set; }
        private BudgetBucket RatesBucket { get; set; }
        private BudgetBucket RegoBucket { get; set; }
        private LedgerBookDto TestData { get; set; }

        [TestMethod]
        [ExpectedException(typeof(FileFormatException))]
        public void InvalidTransactionTypeShouldThrow()
        {
            TestData.DatedEntries.First().Entries.Last().Transactions.First().TransactionType = "Foobar";
            ArrangeAndAct();
        }

        [TestMethod]
        [ExpectedException(typeof(FileFormatException))]
        public void NullTransactionTypeShouldThrow()
        {
            TestData.DatedEntries.First().Entries.Last().Transactions.First().TransactionType = null;
            ArrangeAndAct();
        }

        [TestMethod]
        [Description("A test designed to break when new propperties are added to the LedgerBook. This is a trigger to update the mappers.")]
        public void NumberOfLedgerBookPropertiesShouldBe5()
        {
            int domainProperties = typeof(LedgerBook).CountProperties();
            Assert.AreEqual(5, domainProperties);
        }

        [TestMethod]
        public void ShouldMapCorrectNumberOfLineEntries()
        {
            LedgerBook result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.First().Entries.Count, result.DatedEntries.First().Entries.Count());
        }

        [TestMethod]
        public void ShouldMapCorrectNumberOfLineEntryTransactions()
        {
            LedgerBook result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.First().Entries.First().Transactions.Count, result.DatedEntries.First().Entries.First().Transactions.Count());
        }

        [TestMethod]
        public void ShouldMapCorrectNumberOfLines()
        {
            LedgerBook result = ArrangeAndAct();
            Assert.AreEqual(TestData.DatedEntries.Count, result.DatedEntries.Count());
        }

        [TestMethod]
        public void ShouldMapFileName()
        {
            LedgerBook result = ArrangeAndAct();
            Assert.AreEqual(TestData.FileName, result.FileName);
            Assert.IsNotNull(result.FileName);
        }

        [TestMethod]
        public void ShouldMapLineBalanceAdjustments()
        {
            TestData = DataLedgerBookTestData.TestData2();
            LedgerBook result = ArrangeAndAct();
            LedgerEntryLine subject = result.DatedEntries.First();

            Assert.AreEqual(TestData.DatedEntries.First().BankBalanceAdjustments.Sum(a => a.Credit - a.Debit), subject.TotalBalanceAdjustments);
            Assert.AreNotEqual(0, subject.BankBalanceAdjustments.Count());
        }

        [TestMethod]
        public void ShouldMapLineBankBalance()
        {
            LedgerBook result = ArrangeAndAct();
            LedgerEntryLine subject = result.DatedEntries.First();
            Assert.AreEqual(TestData.DatedEntries.First().BankBalance, subject.TotalBankBalance);
            Assert.AreNotEqual(0, subject.TotalBankBalance);
        }

        [TestMethod]
        public void ShouldMapLineDate()
        {
            LedgerBook result = ArrangeAndAct();
            DateTime subject = result.DatedEntries.First().Date;
            Assert.AreEqual(TestData.DatedEntries.First().Date, subject);
            Assert.AreNotEqual(DateTime.MinValue, subject);
        }

        [TestMethod]
        public void ShouldMapLineEntryBucketCode()
        {
            LedgerBook result = ArrangeAndAct();
            string subject = result.DatedEntries.First().Entries.First().LedgerColumn.BudgetBucket.Code;
            Assert.AreEqual(TestData.DatedEntries.First().Entries.First().BucketCode, subject);
            Assert.IsNotNull(subject);
        }

        [TestMethod]
        public void ShouldMapLineEntryTransactionCredit()
        {
            LedgerBook result = ArrangeAndAct();
            decimal subject = result.DatedEntries.First().Entries.First().Transactions.First().Credit;
            Assert.AreEqual(TestData.DatedEntries.First().Entries.First().Transactions.First().Credit, subject);
            Assert.AreNotEqual(0, subject);
        }

        [TestMethod]
        public void ShouldMapLineEntryTransactionDebit()
        {
            LedgerBook result = ArrangeAndAct();
            decimal subject = result.DatedEntries.First().Entries.First().Transactions.First().Debit;
            Assert.AreEqual(TestData.DatedEntries.First().Entries.First().Transactions.First().Debit, subject);
            Assert.AreEqual(0, subject);
        }

        [TestMethod]
        public void ShouldMapLineEntryTransactionId()
        {
            LedgerBook result = ArrangeAndAct();
            Guid subject = result.DatedEntries.First().Entries.First().Transactions.First().Id;
            Assert.AreEqual(TestData.DatedEntries.First().Entries.First().Transactions.First().Id, subject);
            Assert.AreNotEqual(Guid.Empty, subject);
        }

        [TestMethod]
        public void ShouldMapLineEntryTransactionNarrative()
        {
            LedgerBook result = ArrangeAndAct();
            string subject = result.DatedEntries.First().Entries.First().Transactions.First().Narrative;
            Assert.AreEqual(TestData.DatedEntries.First().Entries.First().Transactions.First().Narrative, subject);
            Assert.IsNotNull(subject);
        }

        [TestMethod]
        public void ShouldMapLineEntryTransactionType()
        {
            LedgerBook result = ArrangeAndAct();
            string subject = result.DatedEntries.First().Entries.First().Transactions.First().GetType().FullName;
            Assert.AreEqual(TestData.DatedEntries.First().Entries.First().Transactions.First().TransactionType, subject);
            Assert.IsNotNull(subject);
        }

        [TestMethod]
        public void ShouldMapLineRemarks()
        {
            LedgerBook result = ArrangeAndAct();
            string subject = result.DatedEntries.First().Remarks;
            Assert.AreEqual(TestData.DatedEntries.First().Remarks, subject);
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
            TestData = DataLedgerBookTestData.TestData1();
        }

        private LedgerBook ArrangeAndAct()
        {
            RatesBucket = new SavedUpForExpenseBucket(TestDataConstants.RatesBucketCode, "Foo");
            CarMtcBucket = new SavedUpForExpenseBucket(TestDataConstants.CarMtcBucketCode, "Foo");
            RegoBucket = new SavedUpForExpenseBucket(TestDataConstants.RegoBucketCode, "Foo");

            var bucketRepositoryMock = new Mock<IBudgetBucketRepository>();
            bucketRepositoryMock.Setup(r => r.GetByCode(TestDataConstants.RatesBucketCode)).Returns(RatesBucket);
            bucketRepositoryMock.Setup(r => r.GetByCode(TestDataConstants.RegoBucketCode)).Returns(RegoBucket);
            bucketRepositoryMock.Setup(r => r.GetByCode(TestDataConstants.CarMtcBucketCode)).Returns(CarMtcBucket);

            var accountTypeRepoMock = new Mock<IAccountTypeRepository>();
            accountTypeRepoMock.Setup(a => a.GetByKey(StatementModelTestData.ChequeAccount.Name)).Returns(StatementModelTestData.ChequeAccount);

            var mapper = new DtoToLedgerBookMapper(new FakeLogger(), bucketRepositoryMock.Object, accountTypeRepoMock.Object);
            return mapper.Map(TestData);
        }
    }
}