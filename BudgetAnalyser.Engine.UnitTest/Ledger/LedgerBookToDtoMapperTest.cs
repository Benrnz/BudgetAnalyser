using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;

namespace BudgetAnalyser.Engine.UnitTest.Ledger;

[TestClass]
public class LedgerBookToDtoMapperTest
{
    private LedgerBook TestData { get; set; }

    [TestMethod]
    public void FilenameShouldBeMapped()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(TestData.StorageKey, result.StorageKey);
    }

    [TestMethod]
    public void FirstDatedEntryShouldHaveSameBankBalance()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(TestData.Reconciliations.First().TotalBankBalance, result.Reconciliations.First().BankBalance);
    }

    [TestMethod]
    public void FirstDatedEntryShouldHaveSameDate()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(TestData.Reconciliations.First().Date, result.Reconciliations.First().Date);
    }

    [TestMethod]
    public void FirstDatedEntryShouldHaveSameNumberOfBalanceAdjustments()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(TestData.Reconciliations.First().BankBalanceAdjustments.Count(), result.Reconciliations.First().BankBalanceAdjustments.Count());
    }

    [TestMethod]
    public void FirstDatedEntryShouldHaveSameNumberOfEntries()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(
            TestData.Reconciliations.First().Entries.Count(),
            result.Reconciliations.First().Entries.Count());
    }

    [TestMethod]
    public void FirstDatedEntryShouldHaveSameRemarks()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(TestData.Reconciliations.First().Remarks, result.Reconciliations.First().Remarks);
    }

    [TestMethod]
    public void FirstDatedEntryWithFirstBalanceAdjustmentShouldHaveSameCredit()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(
            TestData.Reconciliations.First().BankBalanceAdjustments.First().Amount,
            result.Reconciliations.First().BankBalanceAdjustments.First().Amount);
    }

    [TestMethod]
    public void FirstDatedEntryWithFirstBalanceAdjustmentShouldHaveSameDebit()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(
            TestData.Reconciliations.First().BankBalanceAdjustments.First().Amount,
            result.Reconciliations.First().BankBalanceAdjustments.First().Amount);
    }

    [TestMethod]
    public void FirstDatedEntryWithFirstBalanceAdjustmentShouldHaveSameId()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(
            TestData.Reconciliations.First().BankBalanceAdjustments.First().Id,
            result.Reconciliations.First().BankBalanceAdjustments.First().Id);
    }

    [TestMethod]
    public void FirstDatedEntryWithFirstBalanceAdjustmentShouldHaveSameNarrative()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(
            TestData.Reconciliations.First().BankBalanceAdjustments.First().Narrative,
            result.Reconciliations.First().BankBalanceAdjustments.First().Narrative);
    }

    [TestMethod]
    public void FirstDatedEntryWithFirstEntryShouldHaveSameBalance()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(
            TestData.Reconciliations.First().Entries.First().Balance,
            result.Reconciliations.First().Entries.First().Balance);
    }

    [TestMethod]
    public void FirstDatedEntryWithFirstEntryShouldHaveSameBucketCode()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(
            TestData.Reconciliations.First().Entries.First().LedgerBucket.BudgetBucket.Code,
            result.Reconciliations.First().Entries.First().BucketCode);
    }

    [TestMethod]
    public void FirstDatedEntryWithFirstEntryShouldHaveSameNumberOfTransactions()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(
            TestData.Reconciliations.First().Entries.First().Transactions.Count(),
            result.Reconciliations.First().Entries.First().Transactions.Count());
    }

    [TestMethod]
    public void FirstDatedEntryWithFirstEntryWithFirstTransactionsShouldHaveSameAmount()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(
            TestData.Reconciliations.First().Entries.First().Transactions.First().Amount,
            result.Reconciliations.First().Entries.First().Transactions.First().Amount);
    }

    [TestMethod]
    public void FirstDatedEntryWithFirstEntryWithFirstTransactionsShouldHaveSameNarrative()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(
            TestData.Reconciliations.First().Entries.First().Transactions.First().Narrative,
            result.Reconciliations.First().Entries.First().Transactions.First().Narrative);
    }

    [TestMethod]
    public void ModifiedDateShouldBeMapped()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(TestData.Modified, result.Modified);
    }

    [TestMethod]
    public void NameShouldBeMapped()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(TestData.Name, result.Name);
    }

    [TestMethod]
    [Description("A test designed to break when new propperties are added to the LedgerBookDto. This is a trigger to update the mappers.")]
    public void NumberOfDataLedgerBookPropertiesShouldBe7()
    {
        var dataProperties = typeof(LedgerBookDto).CountProperties();
        Assert.AreEqual(7, dataProperties);
    }

    [TestMethod]
    [Description("A test designed to break when new propperties are added to the LedgerEntryLineDto. This is a trigger to update the mappers.")]
    public void NumberOfDataLedgerLinePropertiesShouldBe6()
    {
        var dataProperties = typeof(LedgerEntryLineDto).CountProperties();
        Assert.AreEqual(6, dataProperties);
    }

    [TestMethod]
    [Description("A test designed to break when new propperties are added to the LedgerEntryDto. This is a trigger to update the mappers.")]
    public void NumberOfDataLedgerPropertiesShouldBe4()
    {
        var dataProperties = typeof(LedgerEntryDto).CountProperties();
        Assert.AreEqual(4, dataProperties);
    }

    [TestMethod]
    [Description("A test designed to break when new propperties are added to the LedgerTransactionDto. This is a trigger to update the mappers.")]
    public void NumberOfDataLedgerTransactionPropertiesShouldBe7()
    {
        var dataProperties = typeof(LedgerTransactionDto).CountProperties();
        Assert.AreEqual(7, dataProperties);
    }

    [TestMethod]
    public void ReconciliationsShouldHaveSameCount()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(TestData.Reconciliations.Count(), result.Reconciliations.Count());
    }

    [TestInitialize]
    public void TestInitialise()
    {
        TestData = LedgerBookTestData.TestData2();
    }

    private LedgerBookDto ArrangeAndAct()
    {
        var bucketRepo = new BucketBucketRepoAlwaysFind();
        var accountRepo = new InMemoryAccountTypeRepository();
        var mapper = new MapperLedgerBookToDto2(bucketRepo, accountRepo, new LedgerBucketFactory(bucketRepo, accountRepo), new LedgerTransactionFactory());
        return mapper.ToDto(TestData);
    }
}
