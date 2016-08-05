using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.UnitTest.Helper;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rees.TangyFruitMapper;

namespace BudgetAnalyser.Engine.UnitTest.Ledger
{
    [TestClass]
    public class LedgerBookRepositoryTest
    {
        private const string LoadFileName = @"BudgetAnalyser.Engine.UnitTest.TestData.LedgerBookRepositoryTest_Load_ShouldLoadTheXmlFile.xml";

        private IDtoMapper<LedgerBookDto, LedgerBook> mapper;
        private Mock<IReaderWriterSelector> mockReaderWriterSelector;
        private Mock<IFileReaderWriter> mockReaderWriter;

        [TestMethod]
        public async Task DemoBookFileChecksumShouldNotChangeWhenLoadAndSave()
        {
            double fileChecksum = 0;
            XamlOnDiskLedgerBookRepository subject = ArrangeAndAct();
            LedgerBookDto predeserialiseDto = null;
            //subject.DtoDeserialised += (s, e) =>
            //{
            //    fileChecksum = subject.LedgerBookDto.Checksum;
            //    subject.LedgerBookDto.Checksum = -1;
            //    predeserialiseDto = subject.LedgerBookDto;
            //};
            LedgerBookDto reserialisedDto = null;
            //subject.SaveDtoToDiskOverride = bookDto => reserialisedDto = bookDto;
            LedgerBook book = await subject.LoadAsync(TestDataConstants.DemoLedgerBookFileName, false);
            predeserialiseDto.Output(true);

            await subject.SaveAsync(book, book.StorageKey, false);

            reserialisedDto.Output(true);

            Assert.AreEqual(fileChecksum, reserialisedDto.Checksum);
        }

        [TestMethod]
        public async Task LedgerBookTestData2ShouldHaveACheckSumOf8435()
        {
            string serialisedData = string.Empty;
            var subject = ArrangeAndAct();
            //subject.WriteToDiskOverride = (f, d) => serialisedData = d;
            await subject.SaveAsync(LedgerBookTestData.TestData2(), "Foo.xml", false);

            int checksumPosition = serialisedData.IndexOf("CheckSum=\"", StringComparison.OrdinalIgnoreCase);
            int checksumLength = serialisedData.IndexOf('"', checksumPosition + 11) - checksumPosition;
            string serialisedCheckSum = serialisedData.Substring(checksumPosition + 10, checksumLength - 10);

            Assert.AreEqual(8435.06, double.Parse(serialisedCheckSum));
        }

        [TestMethod]
        public async Task Load_Output()
        {
            XamlOnDiskLedgerBookRepository subject = ArrangeAndAct();
            LedgerBook book = await subject.LoadAsync(LoadFileName, false);

            // Visual compare these two - should be the same
            LedgerBookTestData.TestData2().Output();

            book.Output();
        }

        [TestMethod]
        public async Task Load_ShouldCreateBookThatIsValid()
        {
            XamlOnDiskLedgerBookRepository subject = ArrangeAndAct();
            LedgerBook book = await subject.LoadAsync(LoadFileName, false);
            var builder = new StringBuilder();
            Assert.IsTrue(book.Validate(builder), builder.ToString());
        }

        [TestMethod]
        public async Task Load_ShouldCreateBookWithFirstLineEqualBankBalances()
        {
            XamlOnDiskLedgerBookRepository subject = ArrangeAndAct();
            LedgerBook book = await subject.LoadAsync(LoadFileName, false);
            LedgerBook testData2 = LedgerBookTestData.TestData2();
            LedgerEntryLine line = book.Reconciliations.First();

            Assert.AreEqual(testData2.Reconciliations.First().TotalBankBalance, line.TotalBankBalance);
        }

        [TestMethod]
        public async Task Load_ShouldCreateBookWithFirstLineEqualSurplus()
        {
            XamlOnDiskLedgerBookRepository subject = ArrangeAndAct();
            LedgerBook book = await subject.LoadAsync(LoadFileName, false);
            book.Output();

            LedgerBook testData2 = LedgerBookTestData.TestData2();
            testData2.Output();

            LedgerEntryLine line = book.Reconciliations.First();

            Assert.AreEqual(testData2.Reconciliations.First().CalculatedSurplus, line.CalculatedSurplus);
        }

        [TestMethod]
        public async Task Load_ShouldCreateBookWithSameModifiedDate()
        {
            XamlOnDiskLedgerBookRepository subject = ArrangeAndAct();
            LedgerBook book = await subject.LoadAsync(LoadFileName, false);
            LedgerBook testData2 = LedgerBookTestData.TestData2();

            Assert.AreEqual(testData2.Modified, book.Modified);
        }

        [TestMethod]
        public async Task Load_ShouldCreateBookWithSameName()
        {
            XamlOnDiskLedgerBookRepository subject = ArrangeAndAct();
            LedgerBook book = await subject.LoadAsync(LoadFileName, false);
            LedgerBook testData2 = LedgerBookTestData.TestData2();

            Assert.AreEqual(testData2.Name, book.Name);
        }

        [TestMethod]
        public async Task Load_ShouldCreateBookWithSameNumberOfLedgers()
        {
            XamlOnDiskLedgerBookRepository subject = ArrangeAndAct();
            LedgerBook book = await subject.LoadAsync(LoadFileName, false);
            LedgerBook testData2 = LedgerBookTestData.TestData2();

            Assert.AreEqual(testData2.Ledgers.Count(), book.Ledgers.Count());
        }

        [TestMethod]
        public async Task Load_ShouldCreateBookWithSameNumberOfReconciliations()
        {
            XamlOnDiskLedgerBookRepository subject = ArrangeAndAct();
            LedgerBook book = await subject.LoadAsync(LoadFileName, false);
            LedgerBook testData2 = LedgerBookTestData.TestData2();

            Assert.AreEqual(testData2.Reconciliations.Count(), book.Reconciliations.Count());
        }

        [TestMethod]
        public async Task Load_ShouldLoadTheXmlFile()
        {
            XamlOnDiskLedgerBookRepository subject = ArrangeAndAct();
            LedgerBook book = await subject.LoadAsync(LoadFileName, false);

            Assert.IsNotNull(book);
        }

        [TestMethod]
        public async Task MustBeAbleToLoadDemoLedgerBookFile()
        {
            XamlOnDiskLedgerBookRepository subject = ArrangeAndAct();

            LedgerBook book = await subject.LoadAsync(TestDataConstants.DemoLedgerBookFileName, false);
            book.Output(true);
            Assert.IsNotNull(book);
        }

        [TestMethod]
        public async Task Save_ShouldSaveTheXmlFile()
        {
            var fileName = @"CompleteSmellyFoo.xml";

            XamlOnDiskLedgerBookRepository subject = ArrangeAndAct();
            var saved = false;
            // subject.WriteToDiskOverride = (f, d) => { saved = true; };
            LedgerBook testData = LedgerBookTestData.TestData2();
            await subject.SaveAsync(testData, fileName, false);
            Assert.IsTrue(saved);
        }

        [TestMethod]
        public async Task SavingAndLoadingShouldProduceTheSameCheckSum()
        {
            string serialisedData = string.Empty;
            var subject1 = ArrangeAndAct();
            // subject.WriteToDiskOverride = (f, d) => serialisedData = d;
            await subject1.SaveAsync(LedgerBookTestData.TestData2(), "Foo2.xml", false);

            Debug.WriteLine("Saved / Serialised Xml:");
            Debug.WriteLine(serialisedData);

            LedgerBookDto bookDto;
            var subject2 = ArrangeAndAct();
            //subject2.FileExistsOverride = f => true;
            //subject2.LoadXamlAsStringOverride = f => serialisedData;
            //subject2.LoadXamlFromDiskFromEmbeddedResources = false;
            await subject2.LoadAsync("foo", false);
            //bookDto = subject2.LedgerBookDto;

            int checksumPosition = serialisedData.IndexOf("CheckSum=\"", StringComparison.OrdinalIgnoreCase);
            int checksumLength = serialisedData.IndexOf('"', checksumPosition + 11) - checksumPosition;
            string serialisedCheckSum = serialisedData.Substring(checksumPosition + 10, checksumLength - 10);

            Assert.Fail();
            //Assert.AreEqual(double.Parse(serialisedCheckSum), bookDto.Checksum);
        }

        [TestMethod]
        public async Task SerialiseTestData2ToEnsureItMatches_Load_ShouldLoadTheXmlFile_xml()
        {
            var subject = ArrangeAndAct();
            string serialisedData = string.Empty;
            // subject.WriteToDiskOverride = (f, d) => serialisedData = d;
            await subject.SaveAsync(LedgerBookTestData.TestData2(), "Leonard Nimoy.xml", false);

            Console.WriteLine(serialisedData);

            Assert.IsTrue(serialisedData.Length > 100);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            var accountRepo = new InMemoryAccountTypeRepository();
            var bucketRepo = new BucketBucketRepoAlwaysFind();
            this.mapper = new Mapper_LedgerBookDto_LedgerBook(bucketRepo, accountRepo, new LedgerBucketFactory(bucketRepo, accountRepo), new LedgerTransactionFactory(), new Mock<IReconciliationBuilder>().Object);
            this.mockReaderWriterSelector = new Mock<IReaderWriterSelector>();
            this.mockReaderWriter = new Mock<IFileReaderWriter>();
            this.mockReaderWriterSelector.Setup(m => m.SelectReaderWriter(It.IsAny<bool>())).Returns(this.mockReaderWriter.Object);
        }

        private XamlOnDiskLedgerBookRepository ArrangeAndAct()
        {
            return new XamlOnDiskLedgerBookRepository(
                this.mapper, 
                new BankImportUtilitiesTestHarness(), 
                new LedgerBookFactory(new ReconciliationBuilder(new FakeLogger())),
                this.mockReaderWriterSelector.Object);
        }
    }
}