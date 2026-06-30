#nullable disable
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.XUnit.TestData;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Ledger;

public class SavedUpForLedgerReconciliationBehaviourTest
{
    private const decimal OpeningBalance = 100M;
    private readonly DateOnly reconciliationDate = new(2013, 9, 20);
    private readonly SavedUpForLedger subject2;

    public SavedUpForLedgerReconciliationBehaviourTest()
    {
        this.subject2 = new SavedUpForLedger { BudgetBucket = TransactionsListModelTestData.CarMtcBucket, StoredInAccount = TransactionsListModelTestData.ChequeAccount };
    }

    [Fact]
    public void ShouldAddCompensatingTransaction_GivenClosingBalanceLessThanBudgetAmount()
    {
        var testInput = new List<LedgerTransaction>
        {
            new BudgetCreditLedgerTransaction { Amount = 150, Date = new DateOnly(2013, 9, 11), Narrative = "Budget Amount" },
            new CreditLedgerTransaction { Amount = -200, Date = new DateOnly(2013, 9, 11) }
        };

        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

        result.ShouldBeTrue();
        testInput.Count().ShouldBe(3);
    }

    [Fact]
    public void ShouldAddCompensatingTransaction_GivenNoBudgetAmountAndClosingBalanceLessThanZero()
    {
        var testInput = new List<LedgerTransaction> { new CreditLedgerTransaction { Amount = -200, Date = new DateOnly(2013, 9, 11) } };

        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

        result.ShouldBeTrue();
        testInput.Count.ShouldBe(2);
    }

    [Fact]
    public void ShouldNotAddCompensatingTransaction_GivenClosingBalanceGreaterThanBudgetAmount()
    {
        var testInput = new List<LedgerTransaction>
        {
            new BudgetCreditLedgerTransaction { Amount = 150, Date = new DateOnly(2013, 9, 11), Narrative = "Budget Amount" },
            new CreditLedgerTransaction { Amount = -20, Date = new DateOnly(2013, 9, 11) }
        };
        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

        result.ShouldBeFalse();
        testInput.Count().ShouldBe(2);
    }

    [Fact]
    public void ShouldNotAddCompensatingTransaction_GivenNoBudgetAmountAndClosingBalanceGreaterThanZero()
    {
        var testInput = new List<LedgerTransaction> { new CreditLedgerTransaction { Amount = -20, Date = new DateOnly(2013, 9, 11) } };
        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

        result.ShouldBeFalse();
        testInput.Count().ShouldBe(1);
    }

    [Fact]
    public void ShouldNotSupplementPositiveBalance_GivenClosingBalanceGreaterThanBudgetAmount()
    {
        var testInput = new List<LedgerTransaction>
        {
            new BudgetCreditLedgerTransaction { Amount = 150, Date = new DateOnly(2013, 9, 11), Narrative = "Budget Amount" },
            new CreditLedgerTransaction { Amount = -20, Date = new DateOnly(2013, 9, 11) }
        };

        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

        result.ShouldBeFalse();
        (OpeningBalance + testInput.Sum(t => t.Amount)).ShouldBe(230M);
    }

    [Fact]
    public void ShouldSupplementOverdrawnBalance_GivenClosingBalanceLessThanBudgetAmount()
    {
        var testInput = new List<LedgerTransaction>
        {
            new BudgetCreditLedgerTransaction { Amount = 150, Date = new DateOnly(2013, 9, 11), Narrative = "Budget Amount" },
            new CreditLedgerTransaction { Amount = -200, Date = new DateOnly(2013, 9, 11) }
        };
        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

        result.ShouldBeTrue();
        (OpeningBalance + testInput.Sum(t => t.Amount)).ShouldBe(150);
    }
}
