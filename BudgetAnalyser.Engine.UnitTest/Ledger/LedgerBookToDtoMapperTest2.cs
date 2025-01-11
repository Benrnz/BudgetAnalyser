using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;

namespace BudgetAnalyser.Engine.UnitTest.Ledger;

[TestClass]
public class LedgerBookToDtoMapperTest2
{
    private LedgerBookDto Result { get; set; }
    private LedgerBook TestData => LedgerBookTestData.TestData4();

    [TestMethod]
    public void ShouldMapFileName()
    {
        Assert.AreEqual("C:\\Folder\\book1.xml", Result.StorageKey);
    }

    [TestMethod]
    public void ShouldMapModified()
    {
        Assert.AreEqual(new DateTime(2013, 12, 16), Result.Modified);
    }

    [TestMethod]
    public void ShouldMapName()
    {
        Assert.AreEqual("Test Data 4 Book", Result.Name);
    }

    [TestMethod]
    public void ShouldMapReconciliations()
    {
        Assert.AreEqual(3, Result.Reconciliations.Count);
    }

    [TestInitialize]
    public void TestInitialise()
    {
        var accountRepo = new InMemoryAccountTypeRepository();
        var bucketRepo = new BucketBucketRepoAlwaysFind();
        var subject = new MapperLedgerBookToDto2(bucketRepo, accountRepo, new LedgerBucketFactory(bucketRepo, accountRepo), new LedgerTransactionFactory());
        Result = subject.ToDto(TestData);
    }
}
