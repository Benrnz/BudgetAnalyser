#nullable disable
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Ledger;

public class DtoToLedgerBookMapperTest
{
    public DtoToLedgerBookMapperTest()
    {
        TestData = LedgerBookDtoTestData.TestData1();
    }

    private LedgerBookDto TestData { get; set; }

    [Fact]
    public void InvalidTransactionTypeShouldThrow()
    {
        var invalidTxn = new LedgerTransactionDto
        (
            TestDataConstants.ChequeAccountName,
            -12.45M,
            Date: new DateOnly(2013, 02, 02),
            Id: Guid.NewGuid(),
            Narrative: "Foo",
            TransactionType: "Foobar",
            AutoMatchingReference: null
        );
        var firstReconciliation = TestData.Reconciliations.First();
        var firstEntry = firstReconciliation.Entries.First();

        // Update the entry with the modified transaction
        var updatedTransactions = firstEntry.Transactions.ToList();
        updatedTransactions[0] = invalidTxn;
        var updatedEntry = firstEntry with { Transactions = updatedTransactions.ToArray() };

        // Update the reconciliation with the modified entry
        var updatedEntries = firstReconciliation.Entries.ToList();
        updatedEntries[0] = updatedEntry;
        var updatedReconciliation = firstReconciliation with { Entries = updatedEntries.ToArray() };

        // Update the TestData with the modified reconciliation
        var updatedReconciliations = TestData.Reconciliations.ToList();
        updatedReconciliations[0] = updatedReconciliation;
        TestData = TestData with { Reconciliations = updatedReconciliations.ToArray() };

        Should.Throw<DataFormatException>(() => ArrangeAndAct());
    }

    [Fact]
    public void NullTransactionTypeShouldThrow()
    {
        var invalidTxn = new LedgerTransactionDto
        (
            TestDataConstants.ChequeAccountName,
            -12.45M,
            Date: new DateOnly(2013, 02, 02),
            Id: Guid.NewGuid(),
            Narrative: "Foo",
            TransactionType: null,
            AutoMatchingReference: null
        );
        var firstReconciliation = TestData.Reconciliations.First();
        var firstEntry = firstReconciliation.Entries.First();
        var firstTransaction = firstEntry.Transactions.First();

        // Update the entry with the modified transaction
        var updatedTransactions = firstEntry.Transactions.ToList();
        updatedTransactions[0] = invalidTxn;
        var updatedEntry = firstEntry with { Transactions = updatedTransactions.ToArray() };

        // Update the reconciliation with the modified entry
        var updatedEntries = firstReconciliation.Entries.ToList();
        updatedEntries[0] = updatedEntry;
        var updatedReconciliation = firstReconciliation with { Entries = updatedEntries.ToArray() };

        // Update the TestData with the modified reconciliation
        var updatedReconciliations = TestData.Reconciliations.ToList();
        updatedReconciliations[0] = updatedReconciliation;
        TestData = TestData with { Reconciliations = updatedReconciliations.ToArray() };

        Should.Throw<CorruptedLedgerBookException>(() => ArrangeAndAct());
    }

    [Fact]
    public void NumberOfLedgerBookPropertiesShouldBe6()
    {
        var domainProperties = typeof(LedgerBook).CountProperties();
        domainProperties.ShouldBe(6);
    }

    [Fact]
    public void ShouldIgnoreAndContinueIfLedgerIsNotDeclared_GivenOneLedgerBucketIsMissing()
    {
        var ledgers = TestData.Ledgers.Skip(1).ToArray();
        var myTestData = TestData with { Ledgers = ledgers };
        TestData = myTestData;
        var model = ArrangeAndAct();

        // There should be three ledgers in the book because it is deemed invalid for there to be NO ledgers at all from the persistence file. If this occurs it is repopulated based on the
        // reconciliations and this will be persisted next save.
        model.Ledgers.Count().ShouldBe(2);
    }

    [Fact]
    public void ShouldMapCorrectNumberOfLedgers()
    {
        var result = ArrangeAndAct();

        result.Ledgers.Count().ShouldBe(3);
    }

    [Fact]
    public void ShouldMapCorrectNumberOfLineEntries()
    {
        var result = ArrangeAndAct();
        result.Reconciliations.First().Entries.Count().ShouldBe(TestData.Reconciliations.First().Entries.Count());
    }

    [Fact]
    public void ShouldMapCorrectNumberOfLineEntryTransactions()
    {
        var result = ArrangeAndAct();
        result.Reconciliations.First().Entries.First().Transactions.Count().ShouldBe(TestData.Reconciliations.First().Entries.First().Transactions.Count());
    }

    [Fact]
    public void ShouldMapCorrectNumberOfLines()
    {
        var result = ArrangeAndAct();
        result.Reconciliations.Count().ShouldBe(TestData.Reconciliations.Count());
    }

    [Fact]
    public void ShouldMapFileName()
    {
        var result = ArrangeAndAct();
        result.StorageKey.ShouldBe(TestData.StorageKey);
        result.StorageKey.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldMapLedgerBucketsOnLedgerEntriesWithAccountNotNull()
    {
        var result = ArrangeAndAct();
        result.Reconciliations.SelectMany(e => e.Entries).Any(e => e.LedgerBucket is null).ShouldBeFalse();
    }

    [Fact]
    public void ShouldMapLedgerBucketsWithNoDuplicateInstances()
    {
        var result = ArrangeAndAct();
        var ledgerBuckets = result.Reconciliations
            .SelectMany(e => e.Entries)
            .Select(e => e.LedgerBucket)
            .Union(result.Ledgers)
            .Distinct();

        ledgerBuckets.Count().ShouldBe(3);
    }

    [Fact]
    public void ShouldMapLineBalanceAdjustments()
    {
        TestData = LedgerBookDtoTestData.TestData2();
        var result = ArrangeAndAct();
        var subject = result.Reconciliations.First();

        subject.TotalBalanceAdjustments.ShouldBe(TestData.Reconciliations.First().BankBalanceAdjustments.Sum(a => a.Amount));
        subject.BankBalanceAdjustments.Count().ShouldNotBe(0);
    }

    [Fact]
    public void ShouldMapLineBankBalance()
    {
        var result = ArrangeAndAct();
        var subject = result.Reconciliations.First();
        subject.TotalBankBalance.ShouldBe(TestData.Reconciliations.First().BankBalance);
        subject.TotalBankBalance.ShouldNotBe(0);
    }

    [Fact]
    public void ShouldMapLineDate()
    {
        var result = ArrangeAndAct();
        var subject = result.Reconciliations.First().Date;
        subject.ShouldBe(TestData.Reconciliations.First().Date);
        subject.ShouldNotBe(DateOnly.MinValue);
    }

    [Fact]
    public void ShouldMapLineEntryBucketCode()
    {
        var result = ArrangeAndAct();
        var subject = result.Reconciliations.First().Entries.First().LedgerBucket.BudgetBucket.Code;
        subject.ShouldBe(TestData.Reconciliations.First().Entries.First().BucketCode);
        subject.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldMapLineEntryTransactionAmount()
    {
        var result = ArrangeAndAct();
        var subject = result.Reconciliations.First().Entries.First().Transactions.First().Amount;
        subject.ShouldBe(TestData.Reconciliations.First().Entries.First().Transactions.First().Amount);
        subject.ShouldNotBe(0);
    }

    [Fact]
    public void ShouldMapLineEntryTransactionId()
    {
        var result = ArrangeAndAct();
        var subject = result.Reconciliations.First().Entries.First().Transactions.First().Id;
        subject.ShouldBe(TestData.Reconciliations.First().Entries.First().Transactions.First().Id);
        subject.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void ShouldMapLineEntryTransactionNarrative()
    {
        var result = ArrangeAndAct();
        var subject = result.Reconciliations.First().Entries.First().Transactions.First().Narrative;
        subject.ShouldBe(TestData.Reconciliations.First().Entries.First().Transactions.First().Narrative);
        subject.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldMapLineEntryTransactionType()
    {
        var result = ArrangeAndAct();
        var subject = result.Reconciliations.First().Entries.First().Transactions.First().GetType().FullName;
        subject.ShouldBe(TestData.Reconciliations.First().Entries.First().Transactions.First().TransactionType);
        subject.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldMapLineRemarks()
    {
        var result = ArrangeAndAct();
        var subject = result.Reconciliations.First().Remarks;
        subject.ShouldBe(TestData.Reconciliations.First().Remarks);
        subject.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldMapModifiedDate()
    {
        var result = ArrangeAndAct();
        result.Modified.ShouldBe(TestData.Modified);
        result.Modified.ShouldNotBe(DateTime.MinValue);
    }

    [Fact]
    public void ShouldMapName()
    {
        var result = ArrangeAndAct();
        result.Name.ShouldBe(TestData.Name);
        result.Name.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldRepopulateLedgerCollectionFromReconciliations_GivenDtoContainsNoLedgers()
    {
        var myTestData = TestData with { Ledgers = [] };
        TestData = myTestData;
        var model = ArrangeAndAct();

        // There should be three ledgers in the book because it is deemed invalid for there to be NO ledgers at all from the persistence file. If this occurs it is repopulated based on the
        // reconciliations and this will be persisted next save.
        model.Ledgers.Count().ShouldBe(3);
    }

    private LedgerBook ArrangeAndAct()
    {
        var bucketRepo = new BudgetBucketRepoAlwaysFind();
        var accountRepo = new InMemoryAccountTypeRepository();
        var mapper = new MapperLedgerBookToDto2(bucketRepo, accountRepo, new LedgerBucketFactory(bucketRepo, accountRepo), new LedgerTransactionFactory(), new DebugLogger());
        return mapper.ToModel(TestData);
    }
}
