#nullable disable
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Ledger;

public class DtoToLedgerBookMapperTest2
{
    public DtoToLedgerBookMapperTest2()
    {
        var accountRepo = new InMemoryAccountTypeRepository();
        var bucketRepo = new BudgetBucketRepoAlwaysFind();
        var subject = new MapperLedgerBookToDto2(bucketRepo, accountRepo, new LedgerBucketFactory(bucketRepo, accountRepo), new LedgerTransactionFactory(), new DebugLogger());
        Result = subject.ToModel(TestData);
    }

    private LedgerBook Result { get; }
    private LedgerBookDto TestData => LedgerBookDtoTestData.TestData3();

    [Fact]
    public void ShouldMapFileName()
    {
        Result.StorageKey.ShouldBe(@"C:\Folder\FooBook3.xml");
    }

    [Fact]
    public void ShouldMapModified()
    {
        Result.Modified.ShouldBe(new DateTime(new DateOnly(2013, 12, 14), TimeOnly.MinValue, DateTimeKind.Utc));
    }

    [Fact]
    public void ShouldMapName()
    {
        Result.Name.ShouldBe("Test Budget Ledger Book 3");
    }

    [Fact]
    public void ShouldMapReconciliations()
    {
        Result.Reconciliations.Count().ShouldBe(3);
    }
}
