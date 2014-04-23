using System.IO;
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
        // TODO this is shite:
        private const string LoadFileName = @"C:\Foo\TestData\LedgerBookRepositoryTest_Load_ShouldLoadTheXmlFile.xml";

        private BudgetBucket CarMtcBucket { get; set; }
        private BudgetBucket RatesBucket { get; set; }
        private BudgetBucket RegoBucket { get; set; }
        private BudgetBucket HairBucket { get; set; }
        private BudgetBucket PhoneBucket { get; set; }
        private BudgetBucket PowerBucket { get; set; }

        [TestMethod]
        public void Save_ShouldSaveTheXmlFile()
        {
            var fileName = @"C:\Foo\TestData\LedgerBookRepositoryTest_Save_ShouldSaveTheXmlFile.xml";
            File.Delete(fileName);

            var subject = ArrangeAndAct();
            var testData = LedgerBookTestData.TestData2();
            subject.Save(testData, fileName);

            Assert.IsTrue(File.Exists(fileName));
        }

        [TestMethod]
        public void Save_ShouldSaveTheXmlFile3()
        {
            var fileName = @"C:\Foo\TestData\LedgerBookRepositoryTest_Save_ShouldSaveTheXmlFile3.xml";
            File.Delete(fileName);

            var subject = ArrangeAndAct();
            var testData = LedgerBookTestData.TestData3();
            subject.Save(testData, fileName);

            Assert.IsTrue(File.Exists(fileName));
        }

        [TestMethod]
        public void Load_ShouldLoadTheXmlFile()
        {
            var subject = ArrangeAndAct();
            var book = subject.Load(LoadFileName );

            Assert.IsNotNull(book);
        }

        [TestMethod]
        public void Load_ShouldCreateBookWithSameNumberOfLedgers()
        {
            var subject = ArrangeAndAct();
            var book = subject.Load(LoadFileName);
            var testData2 = LedgerBookTestData.TestData2();

            Assert.AreEqual(testData2.Ledgers.Count(), book.Ledgers.Count());
        }

        [TestMethod]
        public void Load_ShouldCreateBookWithSameNumberOfDatedEntries()
        {
            var subject = ArrangeAndAct();
            var book = subject.Load(LoadFileName);
            var testData2 = LedgerBookTestData.TestData2();

            Assert.AreEqual(testData2.DatedEntries.Count(), book.DatedEntries.Count());
        }

        [TestMethod]
        public void Load_ShouldCreateBookWithSameName()
        {
            var subject = ArrangeAndAct();
            var book = subject.Load(LoadFileName);
            var testData2 = LedgerBookTestData.TestData2();

            Assert.AreEqual(testData2.Name, book.Name);
        }

        [TestMethod]
        public void Load_ShouldCreateBookWithSameModifiedDate()
        {
            var subject = ArrangeAndAct();
            var book = subject.Load(LoadFileName);
            var testData2 = LedgerBookTestData.TestData2();

            Assert.AreEqual(testData2.Modified, book.Modified);
        }

        [TestMethod]
        public void Load_ShouldCreateBookThatIsValid()
        {
            var subject = ArrangeAndAct();
            var book = subject.Load(LoadFileName);
            var builder = new StringBuilder();
            Assert.IsTrue(book.Validate(builder), builder.ToString());
        }

        [TestMethod]
        public void Load_ShouldCreateBookWithFirstLineEqualBankBalances()
        {
            var subject = ArrangeAndAct();
            var book = subject.Load(LoadFileName);
            var testData2 = LedgerBookTestData.TestData2();
            var line = book.DatedEntries.First();

            Assert.AreEqual(testData2.DatedEntries.First().BankBalance, line.BankBalance);
        }

        [TestMethod]
        public void Load_ShouldCreateBookWithFirstLineEqualSurplus()
        {
            var subject = ArrangeAndAct();
            var book = subject.Load(LoadFileName);
            var testData2 = LedgerBookTestData.TestData2();
            var line = book.DatedEntries.First();

            Assert.AreEqual(testData2.DatedEntries.First().CalculatedSurplus, line.CalculatedSurplus);
        }

        [TestMethod]
        public void Load_Output()
        {
            var subject = ArrangeAndAct();
            var book = subject.Load(LoadFileName);

            LedgerBookTestData.TestData2().Output();
            book.Output();
        }

        private XamlOnDiskLedgerBookRepository ArrangeAndAct()
        {
            RatesBucket = new SavedUpForExpense(TestDataConstants.RatesBucketCode, "Foo");
            CarMtcBucket = new SavedUpForExpense(TestDataConstants.CarMtcBucketCode, "Foo");
            RegoBucket = new SavedUpForExpense(TestDataConstants.RegoBucketCode, "Foo");
            HairBucket = new SavedUpForExpense(TestDataConstants.HairBucketCode, "Foo");
            PhoneBucket = new SpentMonthlyExpense(TestDataConstants.PhoneBucketCode, "Foo");
            PowerBucket = new SpentMonthlyExpense(TestDataConstants.PowerBucketCode, "Foo");

            var bucketRepositoryMock = new Mock<IBudgetBucketRepository>();
            bucketRepositoryMock.Setup(r => r.GetByCode(TestDataConstants.RatesBucketCode)).Returns(RatesBucket);
            bucketRepositoryMock.Setup(r => r.GetByCode(TestDataConstants.RegoBucketCode)).Returns(RegoBucket);
            bucketRepositoryMock.Setup(r => r.GetByCode(TestDataConstants.CarMtcBucketCode)).Returns(CarMtcBucket);
            bucketRepositoryMock.Setup(r => r.GetByCode(TestDataConstants.HairBucketCode)).Returns(HairBucket);
            bucketRepositoryMock.Setup(r => r.GetByCode(TestDataConstants.PhoneBucketCode)).Returns(PhoneBucket);
            bucketRepositoryMock.Setup(r => r.GetByCode(TestDataConstants.PowerBucketCode)).Returns(PowerBucket);

            var dataToDomainMapper = new LedgerDataToDomainMapper(bucketRepositoryMock.Object, new FakeLogger());
            var subject = new XamlOnDiskLedgerBookRepository(dataToDomainMapper, new LedgerDomainToDataMapper());

            return subject;
        }
    }
}
