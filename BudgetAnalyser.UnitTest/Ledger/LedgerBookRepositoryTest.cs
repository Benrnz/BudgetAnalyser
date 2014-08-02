using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using BudgetAnalyser.Engine;
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
        private const string DemoLedgerBookFileName = @"BudgetAnalyser.UnitTest.TestData.DemoLedgerBook.xml";
        private const string LoadFileName = @"BudgetAnalyser.UnitTest.TestData.LedgerBookRepositoryTest_Load_ShouldLoadTheXmlFile.xml";

        [TestMethod]
        public void DemoBookFileChecksumShouldNotChangeWhenLoadAndSave()
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
            LedgerBook book = subject.Load(DemoLedgerBookFileName);
            predeserialiseDto.Output(true);

            subject.Save(book);

            reserialisedDto.Output(true);

            Assert.AreEqual(fileChecksum, reserialisedDto.Checksum);
        }

        [TestMethod]
        public void LedgerBookTestData2ShouldHaveACheckSumOf8435()
        {
            string serialisedData = string.Empty;
            {
                var subject = new XamlOnDiskLedgerBookRepositoryTestHarness(new BasicMapper<LedgerBookDto, LedgerBook>(), new LedgerBookToDtoMapper());
                subject.WriteToDiskOverride = (f, d) => serialisedData = d;
                subject.Save(LedgerBookTestData.TestData2());
            }

            int checksumPosition = serialisedData.IndexOf("CheckSum=\"", StringComparison.OrdinalIgnoreCase);
            int checksumLength = serialisedData.IndexOf('"', checksumPosition + 11) - checksumPosition;
            string serialisedCheckSum = serialisedData.Substring(checksumPosition + 10, checksumLength - 10);

            Assert.AreEqual(8435.06, double.Parse(serialisedCheckSum));
        }

        [TestMethod]
        public void Load_Output()
        {
            XamlOnDiskLedgerBookRepositoryTestHarness subject = ArrangeAndAct();
            LedgerBook book = subject.Load(LoadFileName);

            // Visual compare these two - should be the same
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
            book.Output(true);
            Assert.IsNotNull(book);
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

        [TestMethod]
        public void SavingAndLoadingShouldProduceTheSameCheckSum()
        {
            string serialisedData = string.Empty;
            {
                var subject = new XamlOnDiskLedgerBookRepositoryTestHarness(new BasicMapper<LedgerBookDto, LedgerBook>(), new LedgerBookToDtoMapper());
                subject.WriteToDiskOverride = (f, d) => serialisedData = d;
                subject.Save(LedgerBookTestData.TestData2());
            }

            Debug.WriteLine("Saved / Serialised Xml:");
            Debug.WriteLine(serialisedData);

            LedgerBookDto bookDto;
            {
                var subject = new XamlOnDiskLedgerBookRepositoryTestHarness(new DtoToLedgerBookMapper(), new BasicMapper<LedgerBook, LedgerBookDto>());
                subject.FileExistsOverride = f => true;
                subject.LoadXamlAsStringOverride = f => serialisedData;
                subject.LoadXamlFromDiskFromEmbeddedResources = false;
                subject.Load("foo");
                bookDto = subject.LedgerBookDto;
            }

            int checksumPosition = serialisedData.IndexOf("CheckSum=\"", StringComparison.OrdinalIgnoreCase);
            int checksumLength = serialisedData.IndexOf('"', checksumPosition + 11) - checksumPosition;
            string serialisedCheckSum = serialisedData.Substring(checksumPosition + 10, checksumLength - 10);

            Assert.AreEqual(double.Parse(serialisedCheckSum), bookDto.Checksum);
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

        [TestInitialize]
        public void TestInitialise()
        {
            AutoMapperConfigurationTest.AutoMapperConfiguration();
        }

        private XamlOnDiskLedgerBookRepositoryTestHarness ArrangeAndAct()
        {
            return new XamlOnDiskLedgerBookRepositoryTestHarness(new DtoToLedgerBookMapper(), new LedgerBookToDtoMapper());
        }
    }
}