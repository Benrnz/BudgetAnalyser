using BudgetAnalyser.Engine.Ledger.Reconciliation;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.UnitTest.TestData;
using Rees.UnitTestUtilities;

namespace BudgetAnalyser.Engine.UnitTest.Ledger;

[TestClass]
public class ReconciliationBuilderNarrativeTest
{
    private const string BillPaymentTransactionText = "Bill Payment";
    private const string TestDescriptionText = "My description here 123.";
    private Transaction testTransaction;

    [TestMethod]
    public void ExtractNarrative_EmptyDescEmptyRefWithTxnType_ShouldReturnTxnType()
    {
        this.testTransaction.Description = string.Empty;
        this.testTransaction.Reference1 = string.Empty;
        this.testTransaction.Reference2 = string.Empty;
        this.testTransaction.Reference3 = string.Empty;

        var result = ExtractNarrative();

        Assert.AreEqual(BillPaymentTransactionText, result);
    }

    [TestMethod]
    public void ExtractNarrative_NoDescNoRefWithTxnType_ShouldReturnTxnType()
    {
        var result = ExtractNarrative();

        Assert.AreEqual(BillPaymentTransactionText, result);
    }

    [TestMethod]
    public void ExtractNarrative_ShouldUseDescIfNotNull()
    {
        this.testTransaction = new Transaction { Description = "Test Description1" };

        var result = ExtractNarrative();

        Assert.AreEqual("Test Description1", result);
    }

    [TestMethod]
    public void ExtractNarrative_ShouldUseNoDescriptionIfNull()
    {
        this.testTransaction = new Transaction { Description = null, Reference1 = "Ref123", Reference2 = "Ref456", Reference3 = "Ref789" };

        var result = ExtractNarrative();

        Assert.AreEqual("Ref123; Ref456; Ref789", result);
    }

    [TestMethod]
    public void ExtractNarrative_ShouldUseRef1IfNotNull()
    {
        this.testTransaction = new Transaction { Description = "Test Description1", Reference1 = "Ref123" };

        var result = ExtractNarrative();

        Assert.AreEqual("Test Description1; Ref123", result);
    }

    [TestMethod]
    public void ExtractNarrative_ShouldUseRef2IfNotNull()
    {
        this.testTransaction = new Transaction { Description = "Test Description1", Reference1 = "Ref123", Reference2 = "Ref456" };

        var result = ExtractNarrative();

        Assert.AreEqual("Test Description1; Ref123; Ref456", result);
    }

    [TestMethod]
    public void ExtractNarrative_ShouldUseRef3IfNotNull()
    {
        this.testTransaction = new Transaction { Description = "Test Description1", Reference1 = "Ref123", Reference2 = "Ref456", Reference3 = "Ref789" };

        var result = ExtractNarrative();

        Assert.AreEqual("Test Description1; Ref123; Ref456; Ref789", result);
    }

    [TestMethod]
    public void ExtractNarrative_ShouldUseTxnTypeIfEverythingIsNull()
    {
        this.testTransaction = new Transaction
        {
            Description = null!,
            Reference1 = null!,
            Reference2 = null!,
            Reference3 = null!,
            TransactionType = new NamedTransaction("Foo")
        };

        var result = ExtractNarrative();

        Assert.AreEqual("Foo", result);
    }

    [TestMethod]
    public void ExtractNarrative_WithDescEmptyRefWithType()
    {
        this.testTransaction.Description = TestDescriptionText;
        this.testTransaction.Reference1 = string.Empty;
        this.testTransaction.Reference2 = string.Empty;
        this.testTransaction.Reference3 = string.Empty;

        var result = ExtractNarrative();

        Assert.AreEqual(TestDescriptionText, result);
    }

    [TestMethod]
    public void ExtractNarrative_WithDescMixedRefWithType()
    {
        this.testTransaction.Description = TestDescriptionText;
        this.testTransaction.Reference1 = null!;
        this.testTransaction.Reference2 = TestDescriptionText;
        this.testTransaction.Reference3 = string.Empty;

        var result = ExtractNarrative();

        Assert.AreEqual($"{TestDescriptionText}; {TestDescriptionText}", result);
    }

    [TestMethod]
    public void ExtractNarrative_WithDescOneRefWithType()
    {
        this.testTransaction.Description = TestDescriptionText;
        this.testTransaction.Reference1 = TestDescriptionText;
        this.testTransaction.Reference2 = string.Empty;
        this.testTransaction.Reference3 = string.Empty;

        var result = ExtractNarrative();

        Assert.AreEqual($"{TestDescriptionText}; {TestDescriptionText}", result);
    }

    [TestInitialize]
    public void TestInitialise()
    {
        this.testTransaction = new Transaction
        {
            Account = LedgerBookTestData.ChequeAccount,
            Amount = 123.45M,
            BudgetBucket = BudgetBucketTestData.BudgetModelTestData1Buckets.First(),
            Date = new DateOnly(2024, 12, 9),
            Id = Guid.NewGuid(),
            TransactionType = StatementModelTestData.TransactionType /* Bill Payment */
        };
    }

    private string ExtractNarrative()
    {
        return PrivateAccessor.InvokeStaticFunction<string>(typeof(ReconciliationBuilder), "ExtractNarrative", this.testTransaction);
    }
}
