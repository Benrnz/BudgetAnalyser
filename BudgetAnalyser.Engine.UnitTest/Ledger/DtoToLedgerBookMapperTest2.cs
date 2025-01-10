using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.Ledger.Data.Data2;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;

namespace BudgetAnalyser.Engine.UnitTest.Ledger;

[TestClass]
public class DtoToLedgerBookMapperTest2
{
    private LedgerBook Result { get; set; }
    private LedgerBookDto TestData => LedgerBookDtoTestData.TestData3();

    [TestMethod]
    public void ShouldMapFileName()
    {
        Assert.AreEqual(@"C:\Folder\FooBook3.xml", Result.StorageKey);
    }

    [TestMethod]
    public void ShouldMapModified()
    {
        Assert.AreEqual(new DateTime(2013, 12, 14), Result.Modified);
    }

    [TestMethod]
    public void ShouldMapName()
    {
        Assert.AreEqual("Test Budget Ledger Book 3", Result.Name);
    }

    [TestMethod]
    public void ShouldMapReconciliations()
    {
        Assert.AreEqual(3, Result.Reconciliations.Count());
    }

    [TestInitialize]
    public void TestInitialise()
    {
        var accountRepo = new InMemoryAccountTypeRepository();
        var bucketRepo = new BucketBucketRepoAlwaysFind();
        var subject = new MapperLedgerBookToDto2(bucketRepo, accountRepo, new LedgerBucketFactory(bucketRepo, accountRepo), new LedgerTransactionFactory());
        Result = subject.ToModel(TestData);
    }
}
