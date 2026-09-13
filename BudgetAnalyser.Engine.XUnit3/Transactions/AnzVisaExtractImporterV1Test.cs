using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using NSubstitute;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Transactions;

public class AnzVisaExtractImporterV1Test
{
    private readonly BankImportUtilitiesTestHarness bankImportUtilities = new();
    private readonly IReaderWriterSelector readerWriterSelector = Substitute.For<IReaderWriterSelector>();

    [Fact]
    public void CtorShouldThrowGivenNullBankImportUtilities()
    {
        Should.Throw<ArgumentNullException>(() => new AnzVisaExtractImporterV1(null!, new FakeLogger(), this.readerWriterSelector));
    }

    [Fact]
    public void CtorShouldThrowGivenNullLogger()
    {
        Should.Throw<ArgumentNullException>(() => new AnzVisaExtractImporterV1(new BankImportUtilities(new FakeLogger()), null!, this.readerWriterSelector));
    }

    [Fact]
    public async Task LoadShouldParseAFileWithExtraColumns()
    {
        var subject = Arrange();
        subject.ReadLinesOverride = _ => AnzVisaCsvTestData.TestData2();
        var result = await subject.LoadAsync("foo.bar", TransactionsListModelTestData.VisaAccount);

        result.DurationInMonths.ShouldBe(1);
        result.AllTransactions.Count().ShouldBe(13);
    }

    [Fact]
    public async Task LoadShouldParseAGoodFile()
    {
        var subject = Arrange();
        subject.ReadLinesOverride = _ => AnzVisaCsvTestData.TestData1();
        var result = await subject.LoadAsync("foo.bar", TransactionsListModelTestData.VisaAccount);

        result.DurationInMonths.ShouldBe(1);
        result.AllTransactions.Count().ShouldBe(13);
    }

    [Fact]
    public async Task LoadShouldThrowGivenBadData()
    {
        var subject = Arrange();
        subject.ReadLinesOverride = _ => AnzChequeCsvTestData.BadTestData1();

        await Should.ThrowAsync<InvalidDataException>(async () => await subject.LoadAsync("foo.bar", TransactionsListModelTestData.VisaAccount));
    }

    [Fact]
    public async Task LoadShouldThrowIfFileNotFound()
    {
        var subject = Arrange();
        this.bankImportUtilities.AbortIfFileDoesntExistOverride = _ => throw new FileNotFoundException();

        await Should.ThrowAsync<KeyNotFoundException>(async () => await subject.LoadAsync("foo.bar", TransactionsListModelTestData.VisaAccount));
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
    public async Task TasteTestShouldReturnFalseGivenTheChequeFormat()
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

    private AnzVisaExtractImporterV1TestHarness Arrange()
    {
        return new AnzVisaExtractImporterV1TestHarness(this.bankImportUtilities, this.readerWriterSelector);
    }
}
