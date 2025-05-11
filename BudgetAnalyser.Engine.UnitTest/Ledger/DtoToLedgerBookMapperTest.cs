using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;

namespace BudgetAnalyser.Engine.UnitTest.Ledger;

[TestClass]
public class DtoToLedgerBookMapperTest
{
    private LedgerBookDto TestData { get; set; }

    [TestMethod]
    [ExpectedException(typeof(DataFormatException))]
    public void InvalidTransactionTypeShouldThrow()
    {
        var invalidTxn = new LedgerTransactionDto
        {
            Account = TestDataConstants.ChequeAccountName,
            Amount = -12.45M,
            Date = new DateOnly(2013, 02, 02),
            Id = Guid.NewGuid(),
            Narrative = "Foo",
            TransactionType = "Foobar"
        };
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

        ArrangeAndAct();
    }

    [TestMethod]
    [ExpectedException(typeof(CorruptedLedgerBookException))]
    public void NullTransactionTypeShouldThrow()
    {
        var invalidTxn = new LedgerTransactionDto
        {
            Account = TestDataConstants.ChequeAccountName,
            Amount = -12.45M,
            Date = new DateOnly(2013, 02, 02),
            Id = Guid.NewGuid(),
            Narrative = "Foo",
            TransactionType = null
        };
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

        ArrangeAndAct();
    }

    [TestMethod]
    [Description("A test designed to break when new propperties are added to the LedgerBook. This is a trigger to update the mappers.")]
    public void NumberOfLedgerBookPropertiesShouldBe6()
    {
        var domainProperties = typeof(LedgerBook).CountProperties();
        Assert.AreEqual(6, domainProperties);
    }

    [TestMethod]
    public void ShouldIgnoreAndContinueIfLedgerIsNotDeclared_GivenOneLedgerBucketIsMissing()
    {
        var ledgers = TestData.Ledgers.Skip(1).ToArray();
        var myTestData = TestData with { Ledgers = ledgers };
        TestData = myTestData;
        var model = ArrangeAndAct();

        // There should be three ledgers in the book because it is deemed invalid for there to be NO ledgers at all from the persistence file. If this occurs it is repopulated based on the
        // reconciliations and this will be persisted next save.
        Assert.AreEqual(2, model.Ledgers.Count());
    }

    [TestMethod]
    public void ShouldMapCorrectNumberOfLedgers()
    {
        var result = ArrangeAndAct();

        Assert.AreEqual(3, result.Ledgers.Count());
    }

    [TestMethod]
    public void ShouldMapCorrectNumberOfLineEntries()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(TestData.Reconciliations.First().Entries.Count(), result.Reconciliations.First().Entries.Count());
    }

    [TestMethod]
    public void ShouldMapCorrectNumberOfLineEntryTransactions()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(TestData.Reconciliations.First().Entries.First().Transactions.Count(), result.Reconciliations.First().Entries.First().Transactions.Count());
    }

    [TestMethod]
    public void ShouldMapCorrectNumberOfLines()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(TestData.Reconciliations.Count(), result.Reconciliations.Count());
    }

    [TestMethod]
    public void ShouldMapFileName()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(TestData.StorageKey, result.StorageKey);
        Assert.IsNotNull(result.StorageKey);
    }

    [TestMethod]
    public void ShouldMapLedgerBucketsOnLedgerEntriesWithAccountNotNull()
    {
        var result = ArrangeAndAct();
        Assert.IsFalse(result.Reconciliations.SelectMany(e => e.Entries).Any(e => e.LedgerBucket is null));
    }

    [TestMethod]
    public void ShouldMapLedgerBucketsWithNoDuplicateInstances()
    {
        var result = ArrangeAndAct();
        var ledgerBuckets = result.Reconciliations
            .SelectMany(e => e.Entries)
            .Select(e => e.LedgerBucket)
            .Union(result.Ledgers)
            .Distinct();

        Assert.AreEqual(3, ledgerBuckets.Count());
    }

    [TestMethod]
    public void ShouldMapLineBalanceAdjustments()
    {
        TestData = LedgerBookDtoTestData.TestData2();
        var result = ArrangeAndAct();
        var subject = result.Reconciliations.First();

        Assert.AreEqual(TestData.Reconciliations.First().BankBalanceAdjustments.Sum(a => a.Amount), subject.TotalBalanceAdjustments);
        Assert.AreNotEqual(0, subject.BankBalanceAdjustments.Count());
    }

    [TestMethod]
    public void ShouldMapLineBankBalance()
    {
        var result = ArrangeAndAct();
        var subject = result.Reconciliations.First();
        Assert.AreEqual(TestData.Reconciliations.First().BankBalance, subject.TotalBankBalance);
        Assert.AreNotEqual(0, subject.TotalBankBalance);
    }

    [TestMethod]
    public void ShouldMapLineDate()
    {
        var result = ArrangeAndAct();
        var subject = result.Reconciliations.First().Date;
        Assert.AreEqual(TestData.Reconciliations.First().Date, subject);
        Assert.AreNotEqual(DateOnly.MinValue, subject);
    }

    [TestMethod]
    public void ShouldMapLineEntryBucketCode()
    {
        var result = ArrangeAndAct();
        var subject = result.Reconciliations.First().Entries.First().LedgerBucket.BudgetBucket.Code;
        Assert.AreEqual(TestData.Reconciliations.First().Entries.First().BucketCode, subject);
        Assert.IsNotNull(subject);
    }

    [TestMethod]
    public void ShouldMapLineEntryTransactionAmount()
    {
        var result = ArrangeAndAct();
        var subject = result.Reconciliations.First().Entries.First().Transactions.First().Amount;
        Assert.AreEqual(TestData.Reconciliations.First().Entries.First().Transactions.First().Amount, subject);
        Assert.AreNotEqual(0, subject);
    }

    [TestMethod]
    public void ShouldMapLineEntryTransactionId()
    {
        var result = ArrangeAndAct();
        var subject = result.Reconciliations.First().Entries.First().Transactions.First().Id;
        Assert.AreEqual(TestData.Reconciliations.First().Entries.First().Transactions.First().Id, subject);
        Assert.AreNotEqual(Guid.Empty, subject);
    }

    [TestMethod]
    public void ShouldMapLineEntryTransactionNarrative()
    {
        var result = ArrangeAndAct();
        var subject = result.Reconciliations.First().Entries.First().Transactions.First().Narrative;
        Assert.AreEqual(TestData.Reconciliations.First().Entries.First().Transactions.First().Narrative, subject);
        Assert.IsNotNull(subject);
    }

    [TestMethod]
    public void ShouldMapLineEntryTransactionType()
    {
        var result = ArrangeAndAct();
        var subject = result.Reconciliations.First().Entries.First().Transactions.First().GetType().FullName;
        Assert.AreEqual(TestData.Reconciliations.First().Entries.First().Transactions.First().TransactionType, subject);
        Assert.IsNotNull(subject);
    }

    [TestMethod]
    public void ShouldMapLineRemarks()
    {
        var result = ArrangeAndAct();
        var subject = result.Reconciliations.First().Remarks;
        Assert.AreEqual(TestData.Reconciliations.First().Remarks, subject);
        Assert.IsNotNull(subject);
    }

    [TestMethod]
    public void ShouldMapModifiedDate()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(TestData.Modified, result.Modified);
        Assert.AreNotEqual(DateTime.MinValue, result.Modified);
    }

    [TestMethod]
    public void ShouldMapName()
    {
        var result = ArrangeAndAct();
        Assert.AreEqual(TestData.Name, result.Name);
        Assert.IsNotNull(result.Name);
    }

    [TestMethod]
    public void ShouldRepopulateLedgerCollectionFromReconciliations_GivenDtoContainsNoLedgers()
    {
        var myTestData = TestData with { Ledgers = [] };
        TestData = myTestData;
        var model = ArrangeAndAct();

        // There should be three ledgers in the book because it is deemed invalid for there to be NO ledgers at all from the persistence file. If this occurs it is repopulated based on the
        // reconciliations and this will be persisted next save.
        Assert.AreEqual(3, model.Ledgers.Count());
    }

    [TestInitialize]
    public void TestInitialise()
    {
        TestData = LedgerBookDtoTestData.TestData1();
    }

    private LedgerBook ArrangeAndAct()
    {
        var bucketRepo = new BucketBucketRepoAlwaysFind();
        var accountRepo = new InMemoryAccountTypeRepository();
        var mapper = new MapperLedgerBookToDto2(bucketRepo, accountRepo, new LedgerBucketFactory(bucketRepo, accountRepo), new LedgerTransactionFactory(), new DebugLogger());
        return mapper.ToModel(TestData);
    }
}
