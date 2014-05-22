using System.Linq;
using System.Text;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.UnitTest.Helper;
using BudgetAnalyser.UnitTest.TestData;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BudgetAnalyser.UnitTest
{
    [TestClass]
    public class LedgerBookRepositoryTest
    {
        private const string LoadFileName = @"BudgetAnalyser.UnitTest.TestData.LedgerBookRepositoryTest_Load_ShouldLoadTheXmlFile.xml";

        private BudgetBucket CarMtcBucket { get; set; }
        private BudgetBucket HairBucket { get; set; }
        private BudgetBucket PhoneBucket { get; set; }
        private BudgetBucket PowerBucket { get; set; }
        private BudgetBucket RatesBucket { get; set; }
        private BudgetBucket RegoBucket { get; set; }

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
        public void Load_ShouldCreateBookWithFirstLineEqualSurplus()
        {
            XamlOnDiskLedgerBookRepositoryTestHarness subject = ArrangeAndAct();
            LedgerBook book = subject.Load(LoadFileName);
            LedgerBook testData2 = LedgerBookTestData.TestData2();
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
        public void Save_ShouldSaveTheXmlFile()
        {
            string fileName = @"CompleteSmellyFoo.xml";

            XamlOnDiskLedgerBookRepositoryTestHarness subject = ArrangeAndAct();
            bool saved = false;
            subject.SaveXamlFileToDiskMock = book => { saved = true; };
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

            var bucketRepositoryMock = new Mock<IBudgetBucketRepository>();
            bucketRepositoryMock.Setup(r => r.GetByCode(TestDataConstants.RatesBucketCode)).Returns(RatesBucket);
            bucketRepositoryMock.Setup(r => r.GetByCode(TestDataConstants.RegoBucketCode)).Returns(RegoBucket);
            bucketRepositoryMock.Setup(r => r.GetByCode(TestDataConstants.CarMtcBucketCode)).Returns(CarMtcBucket);
            bucketRepositoryMock.Setup(r => r.GetByCode(TestDataConstants.HairBucketCode)).Returns(HairBucket);
            bucketRepositoryMock.Setup(r => r.GetByCode(TestDataConstants.PhoneBucketCode)).Returns(PhoneBucket);
            bucketRepositoryMock.Setup(r => r.GetByCode(TestDataConstants.PowerBucketCode)).Returns(PowerBucket);

            var dataToDomainMapper = new LedgerDataToDomainMapper(bucketRepositoryMock.Object, new FakeLogger());
            var subject = new XamlOnDiskLedgerBookRepositoryTestHarness(dataToDomainMapper, new LedgerDomainToDataMapper());

            return subject;
        }
    }
}