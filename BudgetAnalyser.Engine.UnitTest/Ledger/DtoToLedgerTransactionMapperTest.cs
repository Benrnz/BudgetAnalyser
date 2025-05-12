using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;

namespace BudgetAnalyser.Engine.UnitTest.Ledger;

[TestClass]
public class DtoToLedgerTransactionMapperTest
{
    private static readonly Guid TransactionId = new("7F921750-4467-4EA4-81E6-3EFD466341C6");

    private LedgerTransaction Result { get; set; }

    private LedgerTransactionDto TestData { get; } = new(
        Id: TransactionId,
        Amount: -123.99M,
        Narrative: "Foo bar.",
        TransactionType: typeof(CreditLedgerTransaction).FullName,
        AutoMatchingReference: null,
        Date: null,
        Account: null);

    [TestMethod]
    public void ShouldMapAmount()
    {
        Assert.AreEqual(-123.99M, Result.Amount);
    }

    [TestMethod]
    public void ShouldMapId()
    {
        Assert.AreEqual(TransactionId, Result.Id);
    }

    [TestMethod]
    public void ShouldMapNarrative()
    {
        Assert.AreEqual("Foo bar.", Result.Narrative);
    }

    [TestMethod]
    public void ShouldMapTransactionType()
    {
        Assert.IsInstanceOfType(Result, typeof(CreditLedgerTransaction));
    }

    [TestInitialize]
    public void TestInitialise()
    {
        var subject = new MapperLedgerTransactionToDto2(new LedgerTransactionFactory());
        Result = subject.ToModel(TestData);
    }
}
