using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Moq;

namespace BudgetAnalyser.Engine.UnitTest.Transactions
{
    [TestClass]
    public class AsbAccountExtractImporterV1Test
    {
        private Mock<IReaderWriterSelector> mockReaderWriterSelector;
        private BankImportUtilitiesTestHarness BankImportUtilities { get; set; }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullBankImportUtilities()
        {
            new AsbAccountExtractImporterV1(null, new FakeLogger(), this.mockReaderWriterSelector.Object);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullLogger()
        {
            new AsbAccountExtractImporterV1(new BankImportUtilities(new FakeLogger()), null, this.mockReaderWriterSelector.Object);
            Assert.Fail();
        }

        [TestMethod]
        public async Task LoadShouldParseAGoodFile()
        {
            var subject = Arrange();
            subject.ReadLinesOverride = f => AsbChequeCsvTestData.TestData1();
            var result = await subject.LoadAsync("foo.bar", TransactionsListModelTestData.ChequeAccount);

            Assert.AreEqual(1, result.DurationInMonths);
            Assert.AreEqual(7, result.AllTransactions.Count());
        }

        [TestMethod]
        public async Task LoadShouldParseAFileWithExtraColumns()
        {
            var subject = Arrange();
            subject.ReadLinesOverride = f => AsbChequeCsvTestData.TestData2();
            var result = await subject.LoadAsync("foo.bar", TransactionsListModelTestData.ChequeAccount);

            Assert.AreEqual(1, result.DurationInMonths);
            Assert.AreEqual(7, result.AllTransactions.Count());
        }

        [TestMethod]
        public async Task LoadShouldParseAGoodFileAndOutputIt()
        {
            var subject = Arrange();
            subject.ReadLinesOverride = f => AsbChequeCsvTestData.TestData1();
            var result = await subject.LoadAsync("foo.bar", TransactionsListModelTestData.ChequeAccount);

            Console.WriteLine("Date        Type             Description    Amount    ");
            foreach (var txn in result.AllTransactions)
            {
                Console.WriteLine($"{txn.Date:dd-MMM-yy} {txn.TransactionType,10} {txn.Description,12} {txn.Amount,10}");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(UnexpectedIndexException))]
        public async Task LoadShouldThrowGivenBadData()
        {
            var subject = Arrange();
            subject.ReadLinesOverride = filename => AsbChequeCsvTestData.BadTestData1();
            await subject.LoadAsync("foo.bar", TransactionsListModelTestData.ChequeAccount);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task LoadShouldThrowIfFileNotFound()
        {
            var subject = Arrange();
            BankImportUtilities.AbortIfFileDoesntExistOverride = s => { throw new FileNotFoundException(); };
            await subject.LoadAsync("foo.bar", TransactionsListModelTestData.ChequeAccount);
            Assert.Fail();
        }

        [TestMethod]
        public async Task TasteTestShouldReturnFalseGivenABadFile()
        {
            var subject = Arrange();
            subject.ReadTextChunkOverride = file => "lkjpoisjg809wutwuoipsahf98qyfg0w9ashgpiosxnhbvoiyxcu8o9ui9paso,spotiw93th98sh8,35345345,353453534521,lkhsldhlsk,shgjkshj,sgsjdgsd";
            var result = await subject.TasteTestAsync(@"transumm.CSV");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task TasteTestShouldReturnFalseGivenAnEmptyTasteTestResponse()
        {
            var subject = Arrange();
            subject.ReadTextChunkOverride = file => string.Empty;
            var result = await subject.TasteTestAsync(@"transumm.CSV");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task TasteTestShouldReturnFalseGivenNullTasteTestResponse()
        {
            var subject = Arrange();
            subject.ReadTextChunkOverride = file => null;
            var result = await subject.TasteTestAsync(@"transumm.CSV");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task TasteTestShouldReturnFalseGivenTheAnzChequeFormat()
        {
            var subject = Arrange();
            subject.ReadTextChunkOverride = file => AnzChequeCsvTestData.FirstTwoLines1(); // Anz format given to ASB parser
            var result = await subject.TasteTestAsync(@"transumm.CSV");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task TasteTestShouldReturnFalseGivenTheWestpacFormat()
        {
            var subject = Arrange();
            subject.ReadTextChunkOverride = file => WestpacChequeCsvTestData.FirstTwoLines1();
            var result = await subject.TasteTestAsync(@"transumm.CSV");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task TasteTestShouldReturnTrueGivenAGoodFile()
        {
            var subject = Arrange();
            var result = await subject.TasteTestAsync(@"transumm.CSV");
            Assert.IsTrue(result);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            BankImportUtilities = new BankImportUtilitiesTestHarness();
            this.mockReaderWriterSelector = new Mock<IReaderWriterSelector>();
        }

        private AsbAccountExtractImporterV1TestHarness Arrange()
        {
            return new AsbAccountExtractImporterV1TestHarness(BankImportUtilities, this.mockReaderWriterSelector.Object);
        }
    }
}
