using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;

namespace BudgetAnalyser.Engine.UnitTest.Ledger;

[TestClass]
public class DtoToLedgerEntryLineMapperTest
{
    private LedgerEntryLine Result { get; set; }

    private LedgerEntryLineDto TestData
    {
        get
        {
            var book = LedgerBookDtoTestData.TestData3();
            return book.Reconciliations.First();
        }
    }

    [TestMethod]
    public void ShouldMapBankBalanceAdjustments()
    {
        Assert.AreEqual(1, Result.BankBalanceAdjustments.Count());
    }

    [TestMethod]
    public void ShouldMapBankBalances()
    {
        Assert.AreEqual(2, Result.BankBalances.Count());
    }

    [TestMethod]
    public void ShouldMapDate()
    {
        Assert.AreEqual(new DateOnly(2014, 2, 20), Result.Date);
    }

    [TestMethod]
    public void ShouldMapEntries()
    {
        Assert.AreEqual(3, Result.Entries.Count());
    }

    [TestMethod]
    public void ShouldMapLedgerBalance()
    {
        Assert.AreEqual(1902.44M, Result.LedgerBalance);
    }

    [TestMethod]
    public void ShouldMapRemarks()
    {
        Assert.AreEqual("Lorem ipsum dolor. Mit solo darte.", Result.Remarks);
    }

    [TestMethod]
    public void ShouldMapSurplus()
    {
        Assert.AreEqual(1498.99M, Result.CalculatedSurplus);
    }

    [TestMethod]
    public void ShouldMapTotalBalanceAdjustments()
    {
        Assert.AreEqual(-100.01M, Result.TotalBalanceAdjustments);
    }

    [TestMethod]
    public void ShouldMapTotalBankBalance()
    {
        Assert.AreEqual(2002.45M, Result.TotalBankBalance);
    }

    [TestMethod]
    public void ShouldSetIsNewToFalse()
    {
        Assert.IsFalse(Result.IsNew);
    }

    [TestInitialize]
    public void TestInitialise()
    {
        var accountRepo = new InMemoryAccountTypeRepository();
        var subject = new MapperLedgerEntryLineToDto2(accountRepo, new LedgerBucketFactory(new BucketBucketRepoAlwaysFind(), accountRepo), new LedgerTransactionFactory());
        Result = subject.ToModel(TestData);
    }
}
