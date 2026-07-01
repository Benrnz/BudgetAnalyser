#nullable disable
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Ledger;

public class LedgerBookToDtoMapperTest2
{
    public LedgerBookToDtoMapperTest2()
    {
        var accountRepo = new InMemoryAccountTypeRepository();
        var bucketRepo = new BudgetBucketRepoAlwaysFind();
        var subject = new MapperLedgerBookToDto2(bucketRepo, accountRepo, new LedgerBucketFactory(bucketRepo, accountRepo), new LedgerTransactionFactory(), new DebugLogger());
        Result = subject.ToDto(TestData);
    }

    private LedgerBookDto Result { get; }
    private LedgerBook TestData => LedgerBookTestData.TestData4();

    [Fact]
    public void ShouldMapFileName()
    {
        Result.StorageKey.ShouldBe("C:\\Folder\\book1.xml");
    }

    [Fact]
    public void ShouldMapModified()
    {
        Result.Modified.ShouldBe(new DateTime(2013, 12, 16));
    }

    [Fact]
    public void ShouldMapName()
    {
        Result.Name.ShouldBe("Test Data 4 Book");
    }

    [Fact]
    public void ShouldMapReconciliations()
    {
        Result.Reconciliations.Count().ShouldBe(3);
    }
}
