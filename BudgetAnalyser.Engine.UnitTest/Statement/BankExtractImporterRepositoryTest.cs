using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Statement;
using Moq;

namespace BudgetAnalyser.Engine.UnitTest.Statement;

[TestClass]
public class BankExtractImporterRepositoryTest
{
    private IList<Mock<IBankExtractImporter>> Importers { get; set; }
    private BankExtractImporterRepository Subject { get; set; }

    [TestMethod]
    public void CtorShouldConstructGivenValidListOfImporters()
    {
        var subject = CreateSubject();
        Assert.IsNotNull(subject);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void CtorShouldThrowGivenEmptyListOfImporters()
    {
        new BankExtractImporterRepository(new List<IBankExtractImporter>());
        Assert.Fail();
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void CtorShouldThrowGivenNullListOfImporters()
    {
        new BankExtractImporterRepository(null);
        Assert.Fail();
    }

    [TestMethod]
    [ExpectedException(typeof(DataFormatException))]
    public async Task ImportShouldThrowGivenNoImportersCanImport()
    {
        var model = await Subject.ImportAsync("Foo.bar", new ChequeAccount("Cheque"));
    }

    [TestInitialize]
    public void TestInitialise()
    {
        Importers = new[] { new Mock<IBankExtractImporter>(), new Mock<IBankExtractImporter>() };
        Subject = CreateSubject();
    }

    private BankExtractImporterRepository CreateSubject()
    {
        return new BankExtractImporterRepository(Importers.Select(i => i.Object));
    }
}
