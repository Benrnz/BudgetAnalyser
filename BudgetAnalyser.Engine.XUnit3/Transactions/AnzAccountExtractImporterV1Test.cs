using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using NSubstitute;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Transactions;

public class AnzAccountExtractImporterV1Test
{
    private readonly BankImportUtilitiesTestHarness bankImportUtilities = new();
    private readonly IReaderWriterSelector readerWriterSelector = Substitute.For<IReaderWriterSelector>();

    [Fact]
    public void CtorShouldThrowGivenNullBankImportUtilities()
    {
        Should.Throw<ArgumentNullException>(() => new AnzAccountExtractImporterV1(null!, new FakeLogger(), this.readerWriterSelector));
    }

    [Fact]
    public void CtorShouldThrowGivenNullLogger()
    {
        Should.Throw<ArgumentNullException>(() => new AnzAccountExtractImporterV1(new BankImportUtilities(new FakeLogger()), null!, this.readerWriterSelector));
    }

    [Fact]
    public async Task LoadShouldParseAFileWithExtraColumns()
    {
        var subject = Arrange();
        subject.ReadLinesOverride = _ => AnzChequeCsvTestData.TestData2();
        var result = await subject.LoadAsync("foo.bar", TransactionsListModelTestData.ChequeAccount);

        result.DurationInMonths.ShouldBe(1);
        result.AllTransactions.Count().ShouldBe(7);
    }

    [Fact]
    public async Task LoadShouldParseAGoodFile()
    {
        var subject = Arrange();
        subject.ReadLinesOverride = _ => AnzChequeCsvTestData.TestData1();
        var result = await subject.LoadAsync("foo.bar", TransactionsListModelTestData.ChequeAccount);

        result.DurationInMonths.ShouldBe(1);
        result.AllTransactions.Count().ShouldBe(7);
    }

    [Fact]
    public async Task LoadShouldThrowGivenBadData()
    {
        var subject = Arrange();
        subject.ReadLinesOverride = _ => AnzChequeCsvTestData.BadTestData1();

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
    public async Task TasteTestShouldReturnFalseGivenTheAsbFormat()
    {
        var subject = Arrange();
        subject.ReadTextChunkOverride = _ => AsbChequeCsvTestData.FirstNineLines1();
        var result = await subject.TasteTestAsync(@"transumm.CSV");

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task TasteTestShouldReturnFalseGivenTheVisaFormat()
    {
        var subject = Arrange();
        subject.ReadTextChunkOverride = _ => AnzVisaCsvTestData.FirstTwoLines1();
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

    private AnzAccountExtractImporterV1TestHarness Arrange()
    {
        return new AnzAccountExtractImporterV1TestHarness(this.bankImportUtilities, this.readerWriterSelector);
    }
}
