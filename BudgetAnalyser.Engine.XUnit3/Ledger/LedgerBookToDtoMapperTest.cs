#nullable disable
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Ledger;

public class LedgerBookToDtoMapperTest
{
    public LedgerBookToDtoMapperTest()
    {
        TestData = LedgerBookTestData.TestData2();
    }

    private LedgerBook TestData { get; }

    [Fact]
    public void FilenameShouldBeMapped()
    {
        var result = ArrangeAndAct();
        result.StorageKey.ShouldBe(TestData.StorageKey);
    }

    [Fact]
    public void FirstDatedEntryShouldHaveSameBankBalance()
    {
        var result = ArrangeAndAct();
        result.Reconciliations.First().BankBalance.ShouldBe(TestData.Reconciliations.First().TotalBankBalance);
    }

    [Fact]
    public void FirstDatedEntryShouldHaveSameDate()
    {
        var result = ArrangeAndAct();
        result.Reconciliations.First().Date.ShouldBe(TestData.Reconciliations.First().Date);
    }

    [Fact]
    public void FirstDatedEntryShouldHaveSameNumberOfBalanceAdjustments()
    {
        var result = ArrangeAndAct();
        result.Reconciliations.First().BankBalanceAdjustments.Count().ShouldBe(TestData.Reconciliations.First().BankBalanceAdjustments.Count());
    }

    [Fact]
    public void FirstDatedEntryShouldHaveSameNumberOfEntries()
    {
        var result = ArrangeAndAct();
        result.Reconciliations.First().Entries.Count().ShouldBe(TestData.Reconciliations.First().Entries.Count());
    }

    [Fact]
    public void FirstDatedEntryShouldHaveSameRemarks()
    {
        var result = ArrangeAndAct();
        result.Reconciliations.First().Remarks.ShouldBe(TestData.Reconciliations.First().Remarks);
    }

    [Fact]
    public void FirstDatedEntryWithFirstBalanceAdjustmentShouldHaveSameCredit()
    {
        var result = ArrangeAndAct();
        result.Reconciliations.First().BankBalanceAdjustments.First().Amount.ShouldBe(TestData.Reconciliations.First().BankBalanceAdjustments.First().Amount);
    }

    [Fact]
    public void FirstDatedEntryWithFirstBalanceAdjustmentShouldHaveSameDebit()
    {
        var result = ArrangeAndAct();
        result.Reconciliations.First().BankBalanceAdjustments.First().Amount.ShouldBe(TestData.Reconciliations.First().BankBalanceAdjustments.First().Amount);
    }

    [Fact]
    public void FirstDatedEntryWithFirstBalanceAdjustmentShouldHaveSameId()
    {
        var result = ArrangeAndAct();
        result.Reconciliations.First().BankBalanceAdjustments.First().Id.ShouldBe(TestData.Reconciliations.First().BankBalanceAdjustments.First().Id);
    }

    [Fact]
    public void FirstDatedEntryWithFirstBalanceAdjustmentShouldHaveSameNarrative()
    {
        var result = ArrangeAndAct();
        result.Reconciliations.First().BankBalanceAdjustments.First().Narrative.ShouldBe(TestData.Reconciliations.First().BankBalanceAdjustments.First().Narrative);
    }

    [Fact]
    public void FirstDatedEntryWithFirstEntryShouldHaveSameBalance()
    {
        var result = ArrangeAndAct();
        result.Reconciliations.First().Entries.First().Balance.ShouldBe(TestData.Reconciliations.First().Entries.First().Balance);
    }

    [Fact]
    public void FirstDatedEntryWithFirstEntryShouldHaveSameBucketCode()
    {
        var result = ArrangeAndAct();
        result.Reconciliations.First().Entries.First().BucketCode.ShouldBe(TestData.Reconciliations.First().Entries.First().LedgerBucket.BudgetBucket.Code);
    }

    [Fact]
    public void FirstDatedEntryWithFirstEntryShouldHaveSameNumberOfTransactions()
    {
        var result = ArrangeAndAct();
        result.Reconciliations.First().Entries.First().Transactions.Count().ShouldBe(TestData.Reconciliations.First().Entries.First().Transactions.Count());
    }

    [Fact]
    public void FirstDatedEntryWithFirstEntryWithFirstTransactionsShouldHaveSameAmount()
    {
        var result = ArrangeAndAct();
        result.Reconciliations.First().Entries.First().Transactions.First().Amount.ShouldBe(TestData.Reconciliations.First().Entries.First().Transactions.First().Amount);
    }

    [Fact]
    public void FirstDatedEntryWithFirstEntryWithFirstTransactionsShouldHaveSameNarrative()
    {
        var result = ArrangeAndAct();
        result.Reconciliations.First().Entries.First().Transactions.First().Narrative.ShouldBe(TestData.Reconciliations.First().Entries.First().Transactions.First().Narrative);
    }

    [Fact]
    public void ModifiedDateShouldBeMapped()
    {
        var result = ArrangeAndAct();
        result.Modified.ShouldBe(TestData.Modified);
    }

    [Fact]
    public void NameShouldBeMapped()
    {
        var result = ArrangeAndAct();
        result.Name.ShouldBe(TestData.Name);
    }

    [Fact]
    public void NumberOfDataLedgerBookPropertiesShouldBe7()
    {
        var dataProperties = typeof(LedgerBookDto).CountProperties();
        dataProperties.ShouldBe(7);
    }

    [Fact]
    public void NumberOfDataLedgerLinePropertiesShouldBe6()
    {
        var dataProperties = typeof(LedgerEntryLineDto).CountProperties();
        dataProperties.ShouldBe(6);
    }

    [Fact]
    public void NumberOfDataLedgerPropertiesShouldBe4()
    {
        var dataProperties = typeof(LedgerEntryDto).CountProperties();
        dataProperties.ShouldBe(4);
    }

    [Fact]
    public void NumberOfDataLedgerTransactionPropertiesShouldBe7()
    {
        var dataProperties = typeof(LedgerTransactionDto).CountProperties();
        dataProperties.ShouldBe(7);
    }

    [Fact]
    public void ReconciliationsShouldHaveSameCount()
    {
        var result = ArrangeAndAct();
        result.Reconciliations.Count().ShouldBe(TestData.Reconciliations.Count());
    }

    private LedgerBookDto ArrangeAndAct()
    {
        var bucketRepo = new BudgetBucketRepoAlwaysFind();
        var accountRepo = new InMemoryAccountTypeRepository();
        var mapper = new MapperLedgerBookToDto2(bucketRepo, accountRepo, new LedgerBucketFactory(bucketRepo, accountRepo), new LedgerTransactionFactory(), new DebugLogger());
        return mapper.ToDto(TestData);
    }
}
