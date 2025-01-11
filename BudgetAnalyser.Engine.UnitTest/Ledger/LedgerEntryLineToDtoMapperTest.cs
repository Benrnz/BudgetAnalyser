using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;

namespace BudgetAnalyser.Engine.UnitTest.Ledger;

[TestClass]
public class LedgerEntryLineToDtoMapperTest
{
    private LedgerEntryLineDto Result { get; set; }

    private LedgerEntryLine TestData
    {
        get
        {
            var book = LedgerBookTestData.TestData4();
            return book.Reconciliations.First();
        }
    }

    [TestMethod]
    public void ShouldMapBankBalance()
    {
        Assert.AreEqual(2950M, Result.BankBalance);
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
        Assert.AreEqual(new DateTime(2013, 8, 15), Result.Date);
    }

    [TestMethod]
    public void ShouldMapEntries()
    {
        Assert.AreEqual(3, Result.Entries.Count());
    }

    [TestMethod]
    public void ShouldMapRemarks()
    {
        Assert.AreEqual("The quick brown fox jumped over the lazy dog", Result.Remarks);
    }

    [TestInitialize]
    public void TestInitialise()
    {
        var accountRepo = new InMemoryAccountTypeRepository();
        var subject = new MapperLedgerEntryLineToDto2(accountRepo, new LedgerBucketFactory(new BucketBucketRepoAlwaysFind(), accountRepo), new LedgerTransactionFactory());
        Result = subject.ToDto(TestData);
    }
}
