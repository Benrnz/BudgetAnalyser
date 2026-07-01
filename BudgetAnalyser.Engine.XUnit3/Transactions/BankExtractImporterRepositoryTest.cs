using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Transactions;
using NSubstitute;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Transactions;

public class BankExtractImporterRepositoryTest
{
    private readonly IList<IBankExtractImporter> importers;

    public BankExtractImporterRepositoryTest()
    {
        this.importers =
        [
            Substitute.For<IBankExtractImporter>(),
            Substitute.For<IBankExtractImporter>()
        ];
    }

    [Fact]
    public void CtorShouldConstructGivenValidListOfImporters()
    {
        var subject = CreateSubject();

        subject.ShouldNotBeNull();
    }

    [Fact]
    public void CtorShouldThrowGivenEmptyListOfImporters()
    {
        Should.Throw<ArgumentException>(() => new BankExtractImporterRepository([]));
    }

    [Fact]
    public void CtorShouldThrowGivenNullListOfImporters()
    {
        Should.Throw<ArgumentNullException>(() => new BankExtractImporterRepository(null!));
    }

    [Fact]
    public async Task ImportShouldThrowGivenNoImportersCanImport()
    {
        foreach (var importer in this.importers)
        {
            importer.TasteTestAsync(Arg.Any<string>()).Returns(Task.FromResult(false));
        }

        await Should.ThrowAsync<DataFormatException>(async () => await CreateSubject().ImportAsync("Foo.bar", new ChequeAccount("Cheque")));
    }

    private BankExtractImporterRepository CreateSubject()
    {
        return new BankExtractImporterRepository(this.importers);
    }
}
