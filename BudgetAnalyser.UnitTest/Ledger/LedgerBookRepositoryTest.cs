using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.UnitTest.Helper;
using BudgetAnalyser.UnitTest.TestData;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Ledger
{
    [TestClass]
    public class LedgerBookRepositoryTest
    {
        private const string LoadFileName = @"BudgetAnalyser.UnitTest.TestData.LedgerBookRepositoryTest_Load_ShouldLoadTheXmlFile.xml";

        [TestMethod]
        public async Task DemoBookFileChecksumShouldNotChangeWhenLoadAndSave()
        {
            double fileChecksum = 0;
            XamlOnDiskLedgerBookRepositoryTestHarness subject = ArrangeAndAct();
            LedgerBookDto predeserialiseDto = null;
            subject.DtoDeserialised += (s, e) =>
            {
                fileChecksum = subject.LedgerBookDto.Checksum;
                subject.LedgerBookDto.Checksum = -1;
                predeserialiseDto = subject.LedgerBookDto;
            };
            LedgerBookDto reserialisedDto = null;
            subject.SaveDtoToDiskOverride = bookDto => reserialisedDto = bookDto;
            LedgerBook book = await subject.LoadAsync(TestDataConstants.DemoLedgerBookFileName);
            predeserialiseDto.Output(true);

            await subject.SaveAsync(book, book.FileName);

            reserialisedDto.Output(true);

            Assert.AreEqual(fileChecksum, reserialisedDto.Checksum);
        }

        [TestMethod]
        public async Task LedgerBookTestData2ShouldHaveACheckSumOf8435()
        {
            string serialisedData = string.Empty;
            {
                var subject = new XamlOnDiskLedgerBookRepositoryTestHarness(new BasicMapperFake<LedgerBookDto, LedgerBook>(), new LedgerBookToDtoMapper());
                subject.WriteToDiskOverride = (f, d) => serialisedData = d;
                await subject.SaveAsync(LedgerBookTestData.TestData2(), "Foo.xml");
            }

            int checksumPosition = serialisedData.IndexOf("CheckSum=\"", StringComparison.OrdinalIgnoreCase);
            int checksumLength = serialisedData.IndexOf('"', checksumPosition + 11) - checksumPosition;
            string serialisedCheckSum = serialisedData.Substring(checksumPosition + 10, checksumLength - 10);

            Assert.AreEqual(8435.06, double.Parse(serialisedCheckSum));
        }

        [TestMethod]
        public async Task Load_Output()
        {
            XamlOnDiskLedgerBookRepositoryTestHarness subject = ArrangeAndAct();
            LedgerBook book = await subject.LoadAsync(LoadFileName);

            // Visual compare these two - should be the same
            LedgerBookTestData.TestData2().Output();

            book.Output();
        }

        [TestMethod]
        public async Task Load_ShouldCreateBookThatIsValid()
        {
            XamlOnDiskLedgerBookRepositoryTestHarness subject = ArrangeAndAct();
            LedgerBook book = await subject.LoadAsync(LoadFileName);
            var builder = new StringBuilder();
            Assert.IsTrue(book.Validate(builder), builder.ToString());
        }

        [TestMethod]
        public async Task Load_ShouldCreateBookWithFirstLineEqualBankBalances()
        {
            XamlOnDiskLedgerBookRepositoryTestHarness subject = ArrangeAndAct();
            LedgerBook book = await subject.LoadAsync(LoadFileName);
            LedgerBook testData2 = LedgerBookTestData.TestData2();
            LedgerEntryLine line = book.Reconciliations.First();

            Assert.AreEqual(testData2.Reconciliations.First().TotalBankBalance, line.TotalBankBalance);
        }

        [TestMethod]
        public async Task Load_ShouldCreateBookWithFirstLineEqualSurplus()
        {
            XamlOnDiskLedgerBookRepositoryTestHarness subject = ArrangeAndAct();
            LedgerBook book = await subject.LoadAsync(LoadFileName);
            book.Output();

            LedgerBook testData2 = LedgerBookTestData.TestData2();
            testData2.Output();

            LedgerEntryLine line = book.Reconciliations.First();

            Assert.AreEqual(testData2.Reconciliations.First().CalculatedSurplus, line.CalculatedSurplus);
        }

        [TestMethod]
        public async Task Load_ShouldCreateBookWithSameModifiedDate()
        {
            XamlOnDiskLedgerBookRepositoryTestHarness subject = ArrangeAndAct();
            LedgerBook book = await subject.LoadAsync(LoadFileName);
            LedgerBook testData2 = LedgerBookTestData.TestData2();

            Assert.AreEqual(testData2.Modified, book.Modified);
        }

        [TestMethod]
        public async Task Load_ShouldCreateBookWithSameName()
        {
            XamlOnDiskLedgerBookRepositoryTestHarness subject = ArrangeAndAct();
            LedgerBook book = await subject.LoadAsync(LoadFileName);
            LedgerBook testData2 = LedgerBookTestData.TestData2();

            Assert.AreEqual(testData2.Name, book.Name);
        }

        [TestMethod]
        public async Task Load_ShouldCreateBookWithSameNumberOfLedgers()
        {
            XamlOnDiskLedgerBookRepositoryTestHarness subject = ArrangeAndAct();
            LedgerBook book = await subject.LoadAsync(LoadFileName);
            LedgerBook testData2 = LedgerBookTestData.TestData2();

            Assert.AreEqual(testData2.Ledgers.Count(), book.Ledgers.Count());
        }

        [TestMethod]
        public async Task Load_ShouldCreateBookWithSameNumberOfReconciliations()
        {
            XamlOnDiskLedgerBookRepositoryTestHarness subject = ArrangeAndAct();
            LedgerBook book = await subject.LoadAsync(LoadFileName);
            LedgerBook testData2 = LedgerBookTestData.TestData2();

            Assert.AreEqual(testData2.Reconciliations.Count(), book.Reconciliations.Count());
        }

        [TestMethod]
        public async Task Load_ShouldLoadTheXmlFile()
        {
            XamlOnDiskLedgerBookRepositoryTestHarness subject = ArrangeAndAct();
            LedgerBook book = await subject.LoadAsync(LoadFileName);

            Assert.IsNotNull(book);
        }

        [TestMethod]
        public async Task MustBeAbleToLoadDemoLedgerBookFile()
        {
            XamlOnDiskLedgerBookRepositoryTestHarness subject = ArrangeAndAct();

            LedgerBook book = await subject.LoadAsync(TestDataConstants.DemoLedgerBookFileName);
            book.Output(true);
            Assert.IsNotNull(book);
        }

        [TestMethod]
        public async Task Save_ShouldSaveTheXmlFile()
        {
            var fileName = @"CompleteSmellyFoo.xml";

            XamlOnDiskLedgerBookRepositoryTestHarness subject = ArrangeAndAct();
            var saved = false;
            subject.WriteToDiskOverride = (f, d) => { saved = true; };
            LedgerBook testData = LedgerBookTestData.TestData2();
            await subject.SaveAsync(testData, fileName);
            Assert.IsTrue(saved);
        }

        [TestMethod]
        public async Task SavingAndLoadingShouldProduceTheSameCheckSum()
        {
            string serialisedData = string.Empty;
            {
                var subject = new XamlOnDiskLedgerBookRepositoryTestHarness(new BasicMapperFake<LedgerBookDto, LedgerBook>(), new LedgerBookToDtoMapper());
                subject.WriteToDiskOverride = (f, d) => serialisedData = d;
                await subject.SaveAsync(LedgerBookTestData.TestData2(), "Foo2.xml");
            }

            Debug.WriteLine("Saved / Serialised Xml:");
            Debug.WriteLine(serialisedData);

            LedgerBookDto bookDto;
            {
                var subject = new XamlOnDiskLedgerBookRepositoryTestHarness(new DtoToLedgerBookMapper(new InMemoryAccountTypeRepository()), new BasicMapperFake<LedgerBook, LedgerBookDto>());
                subject.FileExistsOverride = f => true;
                subject.LoadXamlAsStringOverride = f => serialisedData;
                subject.LoadXamlFromDiskFromEmbeddedResources = false;
                await subject.LoadAsync("foo");
                bookDto = subject.LedgerBookDto;
            }

            int checksumPosition = serialisedData.IndexOf("CheckSum=\"", StringComparison.OrdinalIgnoreCase);
            int checksumLength = serialisedData.IndexOf('"', checksumPosition + 11) - checksumPosition;
            string serialisedCheckSum = serialisedData.Substring(checksumPosition + 10, checksumLength - 10);

            Assert.AreEqual(double.Parse(serialisedCheckSum), bookDto.Checksum);
        }

        [TestMethod]
        public async Task SerialiseTestData2ToEnsureItMatches_Load_ShouldLoadTheXmlFile_xml()
        {
            var subject = new XamlOnDiskLedgerBookRepositoryTestHarness(new BasicMapperFake<LedgerBookDto, LedgerBook>(), new LedgerBookToDtoMapper());
            string serialisedData = string.Empty;
            subject.WriteToDiskOverride = (f, d) => serialisedData = d;
            await subject.SaveAsync(LedgerBookTestData.TestData2(), "Leonard Nimoy.xml");

            Console.WriteLine(serialisedData);

            Assert.IsTrue(serialisedData.Length > 100);
        }

        [TestInitialize]
        public void TestInitialise()
        {
        }

        private XamlOnDiskLedgerBookRepositoryTestHarness ArrangeAndAct()
        {
            return new XamlOnDiskLedgerBookRepositoryTestHarness(new DtoToLedgerBookMapper(new InMemoryAccountTypeRepository()), new LedgerBookToDtoMapper());
        }
    }
}