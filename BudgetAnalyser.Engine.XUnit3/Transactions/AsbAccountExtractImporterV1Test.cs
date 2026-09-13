using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using NSubstitute;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Transactions;

public class AsbAccountExtractImporterV1Test
{
    private readonly BankImportUtilitiesTestHarness bankImportUtilities = new();
    private readonly IReaderWriterSelector readerWriterSelector = Substitute.For<IReaderWriterSelector>();

    [Fact]
    public void CtorShouldThrowGivenNullBankImportUtilities()
    {
        Should.Throw<ArgumentNullException>(() => new AsbAccountExtractImporterV1(null!, new FakeLogger(), this.readerWriterSelector));
    }

    [Fact]
    public void CtorShouldThrowGivenNullLogger()
    {
        Should.Throw<ArgumentNullException>(() => new AsbAccountExtractImporterV1(new BankImportUtilities(new FakeLogger()), null!, this.readerWriterSelector));
    }

    [Fact]
    public async Task LoadShouldParseAFileWithExtraColumns()
    {
        var subject = Arrange();
        subject.ReadLinesOverride = _ => AsbChequeCsvTestData.TestData2();
        var result = await subject.LoadAsync("foo.bar", TransactionsListModelTestData.ChequeAccount);

        result.DurationInMonths.ShouldBe(1);
        result.AllTransactions.Count().ShouldBe(7);
    }

    [Fact]
    public async Task LoadShouldParseAGoodFile()
    {
        var subject = Arrange();
        subject.ReadLinesOverride = _ => AsbChequeCsvTestData.TestData1();
        var result = await subject.LoadAsync("foo.bar", TransactionsListModelTestData.ChequeAccount);

        result.DurationInMonths.ShouldBe(1);
        result.AllTransactions.Count().ShouldBe(7);
    }

    [Fact]
    public async Task LoadShouldParseAGoodFileAndOutputIt()
    {
        var subject = Arrange();
        subject.ReadLinesOverride = _ => AsbChequeCsvTestData.TestData1();
        var result = await subject.LoadAsync("foo.bar", TransactionsListModelTestData.ChequeAccount);

        Console.WriteLine("Date        Type             Description    Amount    ");
        foreach (var txn in result.AllTransactions)
        {
            Console.WriteLine($"{txn.Date:dd-MMM-yy} {txn.TransactionType,10} {txn.Description,12} {txn.Amount,10}");
        }
    }

    [Fact]
    public async Task LoadShouldThrowGivenBadData()
    {
        var subject = Arrange();
        subject.ReadLinesOverride = _ => AsbChequeCsvTestData.BadTestData1();

        await Should.ThrowAsync<UnexpectedIndexException>(async () => await subject.LoadAsync("foo.bar", TransactionsListModelTestData.ChequeAccount));
    }

    [Fact]
    public async Task LoadShouldThrowIfFileNotFound()
    {
        var subject = Arrange();
        this.bankImportUtilities.AbortIfFileDoesntExistOverride = _ => throw new FileNotFoundException();

        await Should.ThrowAsync<KeyNotFoundException>(async () => await subject.LoadAsync("foo.bar", TransactionsListModelTestData.ChequeAccount));
    }

    [Fact]
    public async Task TasteTestShouldReturnFalseGivenABadFile()
    {
        var subject = Arrange();
        subject.ReadTextChunkOverride = _ => "lkjpoisjg809wutwuoipsahf98qyfg0w9ashgpiosxnhbvoiyxcu8o9ui9paso,spotiw93th98sh8,35345345,353453534521,lkhsldhlsk,shgjkshj,sgsjdgsd";
        var result = await subject.TasteTestAsync(@"transumm.CSV");

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task TasteTestShouldReturnFalseGivenAnEmptyTasteTestResponse()
    {
        var subject = Arrange();
        subject.ReadTextChunkOverride = _ => string.Empty;
        var result = await subject.TasteTestAsync(@"transumm.CSV");

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task TasteTestShouldReturnFalseGivenNullTasteTestResponse()
    {
        var subject = Arrange();
        subject.ReadTextChunkOverride = _ => null!;
        var result = await subject.TasteTestAsync(@"transumm.CSV");

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task TasteTestShouldReturnFalseGivenTheAnzChequeFormat()
    {
        var subject = Arrange();
        subject.ReadTextChunkOverride = _ => AnzChequeCsvTestData.FirstTwoLines1();
        var result = await subject.TasteTestAsync(@"transumm.CSV");

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task TasteTestShouldReturnFalseGivenTheWestpacFormat()
    {
        var subject = Arrange();
        subject.ReadTextChunkOverride = _ => WestpacChequeCsvTestData.FirstTwoLines1();
        var result = await subject.TasteTestAsync(@"transumm.CSV");

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task TasteTestShouldReturnTrueGivenAGoodFile()
    {
        var subject = Arrange();
        var result = await subject.TasteTestAsync(@"transumm.CSV");

        result.ShouldBeTrue();
    }

    private AsbAccountExtractImporterV1TestHarness Arrange()
    {
        return new AsbAccountExtractImporterV1TestHarness(this.bankImportUtilities, this.readerWriterSelector);
    }
}
