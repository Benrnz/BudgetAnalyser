using System;
using System.Linq;
using System.Text;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.UnitTest.Helper;
using BudgetAnalyser.UnitTest.TestData;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BudgetAnalyser.UnitTest.Ledger
{
    [TestClass]
    public class LedgerBookRepositoryTest
    {
        private const string DemoLedgerBookFileName = @"BudgetAnalyser.UnitTest.TestData.DemoLedgerBook.xml";
        private const string LoadFileName = @"BudgetAnalyser.UnitTest.TestData.LedgerBookRepositoryTest_Load_ShouldLoadTheXmlFile.xml";

        private BudgetBucket CarMtcBucket { get; set; }
        private BudgetBucket DoctorBucket { get; set; }
        private BudgetBucket HairBucket { get; set; }
        private BudgetBucket InsuranceHomeBucket { get; set; }
        private BudgetBucket PhoneBucket { get; set; }
        private BudgetBucket PowerBucket { get; set; }
        private BudgetBucket RatesBucket { get; set; }
        private BudgetBucket RegoBucket { get; set; }
        private BudgetBucket RentBucket { get; set; }
        private BudgetBucket WaterBucket { get; set; }

        [TestInitialize]
        public void TestInitialise()
        {
            AutoMapperConfigurationTest.AutoMapperConfiguration();
        }

        [TestMethod]
        public void Load_Output()
        {
            XamlOnDiskLedgerBookRepositoryTestHarness subject = ArrangeAndAct();
            LedgerBook book = subject.Load(LoadFileName);

            LedgerBookTestData.TestData2().Output();
            book.Output();
        }

        [TestMethod]
        public void Load_ShouldCreateBookThatIsValid()
        {
            XamlOnDiskLedgerBookRepositoryTestHarness subject = ArrangeAndAct();
            LedgerBook book = subject.Load(LoadFileName);
            var builder = new StringBuilder();
            Assert.IsTrue(book.Validate(builder), builder.ToString());
        }

        [TestMethod]
        public void Load_ShouldCreateBookWithFirstLineEqualBankBalances()
        {
            XamlOnDiskLedgerBookRepositoryTestHarness subject = ArrangeAndAct();
            LedgerBook book = subject.Load(LoadFileName);
            LedgerBook testData2 = LedgerBookTestData.TestData2();
            LedgerEntryLine line = book.DatedEntries.First();

            Assert.AreEqual(testData2.DatedEntries.First().TotalBankBalance, line.TotalBankBalance);
        }

        [TestMethod]
        public void SerialiseTestData2ToEnsureItMatches_Load_ShouldLoadTheXmlFile_xml()
        {
            var subject = new XamlOnDiskLedgerBookRepositoryTestHarness(new BasicMapper<LedgerBookDto, LedgerBook>(), new LedgerBookToDtoMapper());
            string serialisedData = string.Empty;
            subject.WriteToDiskOverride = (f, d) => serialisedData = d;
            subject.Save(LedgerBookTestData.TestData2());

            Console.WriteLine(serialisedData);
            Assert.IsTrue(serialisedData.Length > 100);
        }
        
        [TestMethod]
        public void Load_ShouldCreateBookWithFirstLineEqualSurplus()
        {
            XamlOnDiskLedgerBookRepositoryTestHarness subject = ArrangeAndAct();
            LedgerBook book = subject.Load(LoadFileName);
            book.Output();

            LedgerBook testData2 = LedgerBookTestData.TestData2();
            testData2.Output();

            LedgerEntryLine line = book.DatedEntries.First();

            Assert.AreEqual(testData2.DatedEntries.First().CalculatedSurplus, line.CalculatedSurplus);
        }

        [TestMethod]
        public void Load_ShouldCreateBookWithSameModifiedDate()
        {
            XamlOnDiskLedgerBookRepositoryTestHarness subject = ArrangeAndAct();
            LedgerBook book = subject.Load(LoadFileName);
            LedgerBook testData2 = LedgerBookTestData.TestData2();

            Assert.AreEqual(testData2.Modified, book.Modified);
        }

        [TestMethod]
        public void Load_ShouldCreateBookWithSameName()
        {
            XamlOnDiskLedgerBookRepositoryTestHarness subject = ArrangeAndAct();
            LedgerBook book = subject.Load(LoadFileName);
            LedgerBook testData2 = LedgerBookTestData.TestData2();

            Assert.AreEqual(testData2.Name, book.Name);
        }

        [TestMethod]
        public void Load_ShouldCreateBookWithSameNumberOfDatedEntries()
        {
            XamlOnDiskLedgerBookRepositoryTestHarness subject = ArrangeAndAct();
            LedgerBook book = subject.Load(LoadFileName);
            LedgerBook testData2 = LedgerBookTestData.TestData2();

            Assert.AreEqual(testData2.DatedEntries.Count(), book.DatedEntries.Count());
        }

        [TestMethod]
        public void Load_ShouldCreateBookWithSameNumberOfLedgers()
        {
            XamlOnDiskLedgerBookRepositoryTestHarness subject = ArrangeAndAct();
            LedgerBook book = subject.Load(LoadFileName);
            LedgerBook testData2 = LedgerBookTestData.TestData2();

            Assert.AreEqual(testData2.Ledgers.Count(), book.Ledgers.Count());
        }

        [TestMethod]
        public void Load_ShouldLoadTheXmlFile()
        {
            XamlOnDiskLedgerBookRepositoryTestHarness subject = ArrangeAndAct();
            LedgerBook book = subject.Load(LoadFileName);

            Assert.IsNotNull(book);
        }

        [TestMethod]
        public void MustBeAbleToLoadDemoLedgerBookFile()
        {
            XamlOnDiskLedgerBookRepositoryTestHarness subject = ArrangeAndAct();

            LedgerBook book = subject.Load(DemoLedgerBookFileName);

            Assert.IsNotNull(book);
        }

        [TestMethod]
        public void SavingAndLoadingShouldProduceTheSameCheckSum()
        {
            string serialisedData = string.Empty;
            {
                var subject = new XamlOnDiskLedgerBookRepositoryTestHarness(new BasicMapper<LedgerBookDto, LedgerBook>(), new LedgerBookToDtoMapper());
                subject.WriteToDiskOverride = (f, d) => serialisedData = d;
                subject.Save(LedgerBookTestData.TestData2());
            }

            LedgerBookDto bookDto;
            {
                var subject = new XamlOnDiskLedgerBookRepositoryTestHarness(new DtoToLedgerBookMapper(new FakeLogger(), new BucketBucketRepoAlwaysFind(), new InMemoryAccountTypeRepository()), new BasicMapper<LedgerBook, LedgerBookDto>());
                subject.FileExistsOverride = f => true;
                subject.LoadXamlAsStringOverride = f => serialisedData;
                subject.LoadXamlFromDiskFromEmbeddedResources = false;
                subject.Load("foo");
                bookDto = subject.LedgerBookDto;
            }

            var checksumPosition = serialisedData.IndexOf("CheckSum=\"", StringComparison.OrdinalIgnoreCase);
            var checksumLength = serialisedData.IndexOf('"', checksumPosition + 11) - checksumPosition;
            var serialisedCheckSum = serialisedData.Substring(checksumPosition+10, checksumLength-10);

            Assert.AreEqual(double.Parse(serialisedCheckSum), bookDto.Checksum);
        }

        [TestMethod]
        public void Save_ShouldSaveTheXmlFile()
        {
            string fileName = @"CompleteSmellyFoo.xml";

            XamlOnDiskLedgerBookRepositoryTestHarness subject = ArrangeAndAct();
            bool saved = false;
            subject.WriteToDiskOverride = (f, d) => { saved = true; };
            LedgerBook testData = LedgerBookTestData.TestData2();
            subject.Save(testData, fileName);
            Assert.IsTrue(saved);
        }

        private XamlOnDiskLedgerBookRepositoryTestHarness ArrangeAndAct()
        {
            RatesBucket = new SavedUpForExpenseBucket(TestDataConstants.RatesBucketCode, "Foo");
            CarMtcBucket = new SavedUpForExpenseBucket(TestDataConstants.CarMtcBucketCode, "Foo");
            RegoBucket = new SavedUpForExpenseBucket(TestDataConstants.RegoBucketCode, "Foo");
            HairBucket = new SavedUpForExpenseBucket(TestDataConstants.HairBucketCode, "Foo");
            PhoneBucket = new SpentMonthlyExpenseBucket(TestDataConstants.PhoneBucketCode, "Foo");
            PowerBucket = new SpentMonthlyExpenseBucket(TestDataConstants.PowerBucketCode, "Foo");
            WaterBucket = new SpentMonthlyExpenseBucket(TestDataConstants.WaterBucketCode, "Foo");
            InsuranceHomeBucket = new SpentMonthlyExpenseBucket(TestDataConstants.InsuranceHomeBucketCode, "Foo");
            DoctorBucket = new SavedUpForExpenseBucket(TestDataConstants.DoctorBucketCode, "Foo");
            RentBucket = new SpentMonthlyExpenseBucket(TestDataConstants.RentBucketCode, "Foo");

            var bucketRepositoryMock = new Mock<IBudgetBucketRepository>();
            bucketRepositoryMock.Setup(r => r.GetByCode(TestDataConstants.RatesBucketCode)).Returns(RatesBucket);
            bucketRepositoryMock.Setup(r => r.GetByCode(TestDataConstants.RegoBucketCode)).Returns(RegoBucket);
            bucketRepositoryMock.Setup(r => r.GetByCode(TestDataConstants.CarMtcBucketCode)).Returns(CarMtcBucket);
            bucketRepositoryMock.Setup(r => r.GetByCode(TestDataConstants.HairBucketCode)).Returns(HairBucket);
            bucketRepositoryMock.Setup(r => r.GetByCode(TestDataConstants.PhoneBucketCode)).Returns(PhoneBucket);
            bucketRepositoryMock.Setup(r => r.GetByCode(TestDataConstants.PowerBucketCode)).Returns(PowerBucket);
            bucketRepositoryMock.Setup(r => r.GetByCode(TestDataConstants.WaterBucketCode)).Returns(WaterBucket);
            bucketRepositoryMock.Setup(r => r.GetByCode(TestDataConstants.InsuranceHomeBucketCode)).Returns(InsuranceHomeBucket);
            bucketRepositoryMock.Setup(r => r.GetByCode(TestDataConstants.DoctorBucketCode)).Returns(DoctorBucket);
            bucketRepositoryMock.Setup(r => r.GetByCode(TestDataConstants.RentBucketCode)).Returns(RentBucket);

            var accountTypeRepoMock = new Mock<IAccountTypeRepository>();
            accountTypeRepoMock.Setup(a => a.GetByKey(StatementModelTestData.ChequeAccount.Name)).Returns(StatementModelTestData.ChequeAccount);

            var dataToDomainMapper = new DtoToLedgerBookMapper(new FakeLogger(), bucketRepositoryMock.Object, accountTypeRepoMock.Object);
            var subject = new XamlOnDiskLedgerBookRepositoryTestHarness(dataToDomainMapper, new LedgerBookToDtoMapper());

            return subject;
        }
    }
}