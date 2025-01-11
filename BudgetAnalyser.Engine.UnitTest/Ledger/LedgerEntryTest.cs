using BudgetAnalyser.Engine.Ledger;

namespace BudgetAnalyser.Engine.UnitTest.Ledger;

[TestClass]
public class LedgerEntryTest
{
    private readonly List<LedgerTransaction> testTransactions =
    [
        new BudgetCreditLedgerTransaction { Amount = 200 },
        new CreditLedgerTransaction { Amount = -50 },
        new CreditLedgerTransaction { Amount = -30 }
    ];

    private LedgerEntry subject;

    [TestMethod]
    public void AddTransactionShouldEffectEntryBalance()
    {
        var newTransaction = new CreditLedgerTransaction { Amount = -100 };
        this.testTransactions.Add(newTransaction);

        this.subject = new LedgerEntry(this.testTransactions);

        Assert.AreEqual(20M, this.subject.Balance);
    }

    [TestMethod]
    public void AddTransactionShouldEffectEntryNetAmount()
    {
        var newTransaction = new CreditLedgerTransaction { Amount = -100 };
        this.testTransactions.Add(newTransaction);

        this.subject = new LedgerEntry(this.testTransactions);

        Assert.AreEqual(20M, this.subject.NetAmount);
    }

    [TestMethod]
    public void RemoveTransactionShouldEffectEntryNetAmount()
    {
        var txn = this.subject.Transactions.First(t => t is CreditLedgerTransaction);
        this.subject.RemoveTransaction(txn.Id);

        Assert.AreEqual(170M, this.subject.NetAmount);
    }

    [TestMethod]
    public void RemoveTransactionShouldNotEffectEntryBalance()
    {
        this.subject.RemoveTransaction(this.subject.Transactions.First(t => t is CreditLedgerTransaction).Id);

        // The balance cannot be simply set inside the Ledger Line, it must be recalc'ed at the ledger book level.
        Assert.AreEqual(120M, this.subject.Balance);
    }

    [TestInitialize]
    public void TestInitialise()
    {
        this.subject = new LedgerEntry(true) { Balance = 120 };
        this.subject.SetTransactionsForReconciliation(this.testTransactions);
    }
}
