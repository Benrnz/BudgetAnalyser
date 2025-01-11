using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;

namespace BudgetAnalyser.Engine.UnitTest.Ledger;

[TestClass]
public class LedgerTransactionToDtoMapperTest
{
    private static readonly Guid TransactionId = new("7F921750-4467-4EA4-81E6-3EFD466341C6");

    public LedgerTransactionToDtoMapperTest()
    {
        TestData = new CreditLedgerTransaction(new Guid("7F921750-4467-4EA4-81E6-3EFD466341C6")) { Amount = 123.99M, Narrative = "Foo bar." };
    }

    private LedgerTransactionDto Result { get; set; }
    private LedgerTransaction TestData { get; }

    [TestMethod]
    public void ShouldMapAmount()
    {
        Assert.AreEqual(123.99M, Result.Amount);
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
        Assert.AreEqual(typeof(CreditLedgerTransaction).FullName, Result.TransactionType);
    }

    [TestMethod]
    public void ShouldNotMapBankAccountForCreditLedgerTransaction()
    {
        Assert.IsNull(Result.Account);
    }

    [TestMethod]
    public void ShouldNotMapBankAccountForDebitLedgerTransaction()
    {
        Assert.IsNull(Result.Account);
    }

    [TestInitialize]
    public void TestInitialise()
    {
        var subject = new MapperLedgerTransactionToDto2(new LedgerTransactionFactory());
        Result = subject.ToDto(TestData);
    }
}
