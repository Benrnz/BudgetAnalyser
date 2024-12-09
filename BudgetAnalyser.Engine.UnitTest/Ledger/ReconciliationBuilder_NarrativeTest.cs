using BudgetAnalyser.Engine.Ledger.Reconciliation;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.UnitTest.TestData;
using Rees.UnitTestUtilities;

namespace BudgetAnalyser.Engine.UnitTest.Ledger;

[TestClass]
public class ReconciliationBuilder_NarrativeTest
{
    private const string? NoDescriptionText = "[No Description]";
    private const string? BillPaymentTransactionText = "Bill Payment";
    private const string TestDescriptionText = "My description here 123.";
    private Transaction testTransaction; 
    
    [TestInitialize]
    public void TestInitialise()
    {
        this.testTransaction = new Transaction
        {
            Account = LedgerBookTestData.ChequeAccount,
            Amount = 123.45M,
            BudgetBucket = BudgetBucketTestData.BudgetModelTestData1Buckets.First(),
            Date = new DateTime(2024, 12, 9),
            Id = Guid.NewGuid(),
            TransactionType = StatementModelTestData.TransactionType, /* Bill Payment */
        };
    }
    
    [TestMethod]
    public void ExtractNarrative_NoDescNoRefWithTxnType_ShouldReturnTxnType()
    {
        var result = ExtractNarrative();

        Assert.AreEqual(BillPaymentTransactionText, result);
    }
    
    [TestMethod]
    public void ExtractNarrative_NoDescNoRefNoType_ShouldReturnNoDescription()
    {
        this.testTransaction.TransactionType = null;
        var result = ExtractNarrative();

        Assert.AreEqual(NoDescriptionText, result);
    }

    [TestMethod]
    public void ExtractNarrative_EmptyDescEmptyRefWithTxnType_ShouldReturnTxnType()
    {
        this.testTransaction.Description = string.Empty;
        this.testTransaction.Reference1 =  string.Empty;
        this.testTransaction.Reference2 =  string.Empty;
        this.testTransaction.Reference3 =  string.Empty;
        
        var result = ExtractNarrative();

        Assert.AreEqual(BillPaymentTransactionText, result);
    }

    [TestMethod]
    public void ExtractNarrative_EmptyDescEmptyRefNoType_ShouldReturnNoDescription()
    {
        this.testTransaction.TransactionType = null;
        this.testTransaction.Description = string.Empty;
        this.testTransaction.Reference1 =  string.Empty;
        this.testTransaction.Reference2 =  string.Empty;
        this.testTransaction.Reference3 =  string.Empty;
        
        var result = ExtractNarrative();

        Assert.AreEqual(NoDescriptionText, result);
    }

    [TestMethod]
    public void ExtractNarrative_WithDescEmptyRefWithType()
    {
        this.testTransaction.Description = TestDescriptionText;
        this.testTransaction.Reference1 =  string.Empty;
        this.testTransaction.Reference2 =  string.Empty;
        this.testTransaction.Reference3 =  string.Empty;
        
        var result = ExtractNarrative();

        Assert.AreEqual(TestDescriptionText, result);
    }

    [TestMethod]
    public void ExtractNarrative_WithDescOneRefWithType()
    {
        this.testTransaction.Description = TestDescriptionText;
        this.testTransaction.Reference1 =  TestDescriptionText;
        this.testTransaction.Reference2 =  string.Empty;
        this.testTransaction.Reference3 =  string.Empty;
        
        var result = ExtractNarrative();

        Assert.AreEqual($"{TestDescriptionText}; {TestDescriptionText}", result);
    }

    private string? ExtractNarrative()
    {
        return PrivateAccessor.InvokeStaticFunction<string>(typeof(ReconciliationBuilder), "ExtractNarrative", this.testTransaction);
    }
}