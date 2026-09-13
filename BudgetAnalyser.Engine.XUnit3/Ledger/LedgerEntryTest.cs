#nullable disable
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.XUnit.TestData;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Ledger;

public class LedgerEntryTest
{
    private readonly List<LedgerTransaction> testTransactions =
    [
        new BudgetCreditLedgerTransaction { Amount = 200 },
        new CreditLedgerTransaction { Amount = -50 },
        new CreditLedgerTransaction { Amount = -30 }
    ];

    private LedgerEntry subject;

    public LedgerEntryTest()
    {
        this.subject = new LedgerEntry(true) { Balance = 120, LedgerBucket = LedgerBookTestData.PowerLedger };
        this.subject.SetTransactionsForReconciliation(this.testTransactions);
    }

    [Fact]
    public void AddTransactionShouldEffectEntryBalance()
    {
        var newTransaction = new CreditLedgerTransaction { Amount = -100 };
        this.testTransactions.Add(newTransaction);

        this.subject = new LedgerEntry(this.testTransactions) { LedgerBucket = LedgerBookTestData.PowerLedger };

        this.subject.Balance.ShouldBe(20M);
    }

    [Fact]
    public void AddTransactionShouldEffectEntryNetAmount()
    {
        var newTransaction = new CreditLedgerTransaction { Amount = -100 };
        this.testTransactions.Add(newTransaction);

        this.subject = new LedgerEntry(this.testTransactions) { LedgerBucket = LedgerBookTestData.PowerLedger };

        this.subject.NetAmount.ShouldBe(20M);
    }

    [Fact]
    public void RemoveTransactionShouldEffectEntryNetAmount()
    {
        var txn = this.subject.Transactions.First(t => t is CreditLedgerTransaction);
        this.subject.RemoveTransaction(txn.Id);

        this.subject.NetAmount.ShouldBe(170M);
    }

    [Fact]
    public void RemoveTransactionShouldNotEffectEntryBalance()
    {
        this.subject.RemoveTransaction(this.subject.Transactions.First(t => t is CreditLedgerTransaction).Id);

        // The balance cannot be simply set inside the Ledger Line, it must be recalc'ed at the ledger book level.
        this.subject.Balance.ShouldBe(120M);
    }
}
