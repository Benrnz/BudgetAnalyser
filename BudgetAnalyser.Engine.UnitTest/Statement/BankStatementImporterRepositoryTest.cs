using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Statement;
using Moq;

namespace BudgetAnalyser.Engine.UnitTest.Statement;

[TestClass]
public class BankStatementImporterRepositoryTest
{
    private IList<Mock<IBankStatementImporter>> Importers { get; set; }
    private BankStatementImporterRepository Subject { get; set; }

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
        new BankStatementImporterRepository(new List<IBankStatementImporter>());
        Assert.Fail();
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void CtorShouldThrowGivenNullListOfImporters()
    {
        new BankStatementImporterRepository(null);
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
        Importers = new[] { new Mock<IBankStatementImporter>(), new Mock<IBankStatementImporter>() };
        Subject = CreateSubject();
    }

    private BankStatementImporterRepository CreateSubject()
    {
        return new BankStatementImporterRepository(Importers.Select(i => i.Object));
    }
}
