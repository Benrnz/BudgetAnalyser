#nullable disable
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.XUnit.Helpers;
using BudgetAnalyser.Engine.XUnit.TestData;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Ledger;

/// <summary>
///     These tests resulted from investigating Issue 83.
/// </summary>
public class SpentMonthlyLedgerReconciliationBehaviourTest
{
    /*
    Test Cases:
    ===========
    Opening Balance   Budget   Closing Balance   Action
    1) 0              0        0                 None
    2) 1              0        0                 Add supplement txn
    3) 0              1        0                 Add supplement txn
    4) 1              1        0                 Add supplement txn
    5) 0              0        1                 Add remove excess txn
    6) 1              0        1                 None
    7) 0              1        1                 None
    8) 1              1        1                 None

    1's and 0's only indicate difference of values ie whether values are equal to greater than, or less than.
    0's do not indicate the absence of a record.

    */
    private const decimal OpeningBalance = 125M;
    private readonly DateOnly reconciliationDate;
    private readonly LedgerEntry subject;
    private readonly SpentPerPeriodLedger subject2;

    public SpentMonthlyLedgerReconciliationBehaviourTest()
    {
        this.reconciliationDate = new DateOnly(2013, 9, 20);

        this.subject2 = new SpentPerPeriodLedger { BudgetBucket = TransactionsListModelTestData.PowerBucket, StoredInAccount = TransactionsListModelTestData.ChequeAccount };
        this.subject = new LedgerEntry(true) { LedgerBucket = LedgerBookTestData.PowerLedger, Balance = OpeningBalance };
    }

    [Fact]
    public void ShouldAddCompensatingTransaction_GivenOpeningBalanceEqualsClosingBalanceAndNoBudgetAmount()
    {
        this.subject.Balance = 1;
        Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
        var testInput = new List<LedgerTransaction>
        {
            new CreditLedgerTransaction { Amount = 1, Date = new DateOnly(2013, 9, 11) }, new CreditLedgerTransaction { Amount = -1, Date = new DateOnly(2013, 9, 11) }
        };
        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

        result.ShouldBeTrue();
        testInput.Count().ShouldBe(3);
    }

    [Fact]
    public void ShouldCreateRemoveExcessTransaction_GivenClosingBalanceIsGreaterThanBudgetAmountAndOpeningBalance()
    {
        this.subject.Balance = 0;
        Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
        var testInput = new List<LedgerTransaction>
        {
            new BudgetCreditLedgerTransaction { Amount = 0M, Date = this.reconciliationDate, Narrative = "Budget Amount" },
            new CreditLedgerTransaction { Amount = 1M, Date = new DateOnly(2013, 9, 11) }
        };
        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

        result.ShouldBeTrue();
        testInput.Count().ShouldBe(3);
    }

    [Fact]
    public void ShouldCreateRemoveExcessTransaction_GivenClosingBalanceIsGreaterThanOpeningBalanceAndNoBudgetAmount()
    {
        this.subject.Balance = 0;
        Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
        var testInput = new List<LedgerTransaction> { new CreditLedgerTransaction { Amount = 1M, Date = new DateOnly(2013, 9, 11) } };
        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

        result.ShouldBeTrue();
        testInput.Count().ShouldBe(2);
    }

    [Fact]
    public void ShouldCreateSupplementTransaction_GivenClosingBalanceIsLessThanBudgetAmountAndLessThanOpeningBalance()
    {
        this.subject.Balance = 1;
        Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
        var testInput = new List<LedgerTransaction>
        {
            new BudgetCreditLedgerTransaction { Amount = 1, Date = new DateOnly(2013, 9, 11), Narrative = "Budget Amount" },
            new CreditLedgerTransaction { Amount = -2M, Date = new DateOnly(2013, 9, 11) }
        };
        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

        result.ShouldBeTrue();
        testInput.Count().ShouldBe(3);
    }

    [Fact]
    public void ShouldCreateSupplementTransaction_GivenClosingBalanceIsLessThanBudgetAmountAndLessThanOrEqualToOpeningBalance()
    {
        var testInput = new List<LedgerTransaction>
        {
            new BudgetCreditLedgerTransaction { Amount = 1, Date = new DateOnly(2013, 9, 11), Narrative = "Budget Amount" },
            new CreditLedgerTransaction { Amount = -1M, Date = new DateOnly(2013, 9, 11) }
        };
        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, 0);

        result.ShouldBeTrue();
        testInput.Count().ShouldBe(3);
    }

    [Fact]
    public void ShouldNotAddCompensatingTransaction_GivenClosingBalanceIsEqualToBudgetAndNoWithdrawals()
    {
        var testInput = new List<LedgerTransaction> { new BudgetCreditLedgerTransaction { Amount = 1, Date = this.reconciliationDate, Narrative = "Budget Amount" } };
        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, 0);

        result.ShouldBeFalse();
        testInput.Count().ShouldBe(1);
    }


    [Fact]
    public void ShouldNotAddCompensatingTransaction_GivenClosingBalanceIsEqualToOpeningBalanceAndBudget()
    {
        Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
        var testInput = new List<LedgerTransaction>
        {
            new BudgetCreditLedgerTransaction { Amount = OpeningBalance, Date = this.reconciliationDate, Narrative = "Budget Amount" },
            new CreditLedgerTransaction { Amount = -OpeningBalance, Date = new DateOnly(2013, 9, 11) }
        };
        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

        result.ShouldBeFalse();
        testInput.Count().ShouldBe(2);
    }

    [Fact]
    public void ShouldNotAddCompensatingTransaction_GivenOpeningBalanceAndClosingBalanceAndBudgetAreAllZero()
    {
        this.subject.Balance = 0;
        Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
        var testInput = new List<LedgerTransaction>();
        this.subject.SetTransactionsForReconciliation(testInput);
        this.subject.Output();

        this.subject.Transactions.Count().ShouldBe(0);
    }

    [Fact]
    public void ShouldNotAddCompensatingTransaction_GivenOpeningBalanceEqualsClosingBalanceAndTransactionsEqualise()
    {
        this.subject.Balance = 1;
        Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
        var testInput = new List<LedgerTransaction>
        {
            new BudgetCreditLedgerTransaction { Amount = 0, Date = this.reconciliationDate, Narrative = "Budget Amount" },
            new CreditLedgerTransaction { Amount = 1, Date = new DateOnly(2013, 9, 11) },
            new CreditLedgerTransaction { Amount = -1, Date = new DateOnly(2013, 9, 11) }
        };
        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

        result.ShouldBeFalse();
        testInput.Count().ShouldBe(3);
    }

    [Fact]
    public void ShouldNotCreateSupplementTransaction_GivenClosingBalanceIsLessThanOpeningBalanceAndBudgetAmountEqualsClosingBalance()
    {
        this.subject.Balance = 1;
        Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
        var testInput = new List<LedgerTransaction>
        {
            new BudgetCreditLedgerTransaction { Amount = 1M, Date = this.reconciliationDate, Narrative = "Budget Amount" },
            new CreditLedgerTransaction { Amount = -1, Date = new DateOnly(2013, 9, 11) }
        };
        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

        result.ShouldBeFalse();
        testInput.Count().ShouldBe(2);
    }

    [Fact]
    public void ShouldNotCreateSupplementTransaction_GivenClosingBalanceIsLessThanOpeningBalanceAndNoBudgetAmount()
    {
        var testInput = new List<LedgerTransaction> { new CreditLedgerTransaction { Amount = -1, Date = new DateOnly(2013, 9, 11) } };
        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, 1);

        result.ShouldBeFalse();
        testInput.Count().ShouldBe(1);
    }

    [Fact]
    public void ShouldOnlyRemoveExcessUpToBudgetAmount_GivenBudgetAmountIsGreaterThanOpeningBalance()
    {
        Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
        var testInput = new List<LedgerTransaction>
        {
            new BudgetCreditLedgerTransaction { Amount = 175M, Date = this.reconciliationDate, Narrative = "Budget Amount" },
            new CreditLedgerTransaction { Amount = -75M, Date = new DateOnly(2013, 9, 11) }
        };
        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

        result.ShouldBeTrue();
        (OpeningBalance + testInput.Sum(t => t.Amount)).ShouldBe(175M);
    }

    [Fact]
    public void ShouldOnlyRemoveExcessUpToOpeningBalance_GivenOpeningBalanceIsGreaterThanBudgetAmount()
    {
        Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
        var testInput = new List<LedgerTransaction>
        {
            new BudgetCreditLedgerTransaction { Amount = 105M, Date = this.reconciliationDate, Narrative = "Budget Amount" },
            new CreditLedgerTransaction { Amount = -75M, Date = new DateOnly(2013, 9, 11) }
        };
        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

        result.ShouldBeTrue();
        (OpeningBalance + testInput.Sum(t => t.Amount)).ShouldBe(125M);
    }

    [Fact]
    public void ShouldRemoveExcessToZero_GivenNetDifferenceOfZeroAndNoBudgetAmount()
    {
        this.subject.Balance = 100;
        Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
        var testInput = new List<LedgerTransaction>
        {
            new CreditLedgerTransaction { Amount = 300M, Date = new DateOnly(2013, 9, 11) }, new CreditLedgerTransaction { Amount = -300M, Date = new DateOnly(2013, 9, 11) }
        };

        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

        result.ShouldBeTrue();
        (OpeningBalance + testInput.Sum(t => t.Amount)).ShouldBe(0M);
    }

    [Fact]
    public void ShouldSupplementToBudgetAmount_GivenNetDifferenceOfZeroAndClosingBalanceIsLessThanBudgetAmount()
    {
        this.subject.Balance = 100;
        Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
        var testInput = new List<LedgerTransaction>
        {
            new BudgetCreditLedgerTransaction { Amount = 300M, Date = new DateOnly(2013, 9, 11) }, new CreditLedgerTransaction { Amount = -300M, Date = new DateOnly(2013, 9, 11) }
        };

        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

        result.ShouldBeTrue();
        (OpeningBalance + testInput.Sum(t => t.Amount)).ShouldBe(300M);
    }

    [Fact]
    public void ShouldSupplementToOpeningBalance_GivenClosingBalanceLessThanOpeningBalance()
    {
        this.subject.Balance = 3552.20M;
        Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
        var testInput = new List<LedgerTransaction>
        {
            new CreditLedgerTransaction { Amount = -1497.20M, Date = new DateOnly(2013, 12, 22) },
            new CreditLedgerTransaction { Amount = -1251.17M, Date = new DateOnly(2014, 1, 14) },
            new BudgetCreditLedgerTransaction { Amount = 1620.00M, Date = new DateOnly(2014, 1, 20) }
        };
        this.subject.SetTransactionsForReconciliation(testInput);

        this.subject.Output();

        this.subject.Balance.ShouldBe(3552.20M);
    }

    [Fact]
    public void ShouldSupplementToZero_GivenNoBudgetAmountAndClosingBalanceIsLessThanZero()
    {
        this.subject.Balance = 100;
        Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
        var testInput = new List<LedgerTransaction> { new CreditLedgerTransaction { Amount = -300M, Date = new DateOnly(2013, 9, 11) } };
        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

        result.ShouldBeTrue();
        (OpeningBalance + testInput.Sum(t => t.Amount)).ShouldBe(0M);
    }

    [Fact]
    public void ShouldSupplementUpToBudgetAmount_GivenClosingBalanceIsLessThanBudgetAmountAndBudgetAmountIsGreaterThanOpeningBalance()
    {
        Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
        var testInput = new List<LedgerTransaction>
        {
            new BudgetCreditLedgerTransaction { Amount = 200M, Date = this.reconciliationDate, Narrative = "Budget Amount" },
            new CreditLedgerTransaction { Amount = -200M, Date = new DateOnly(2013, 9, 11) }
        };
        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

        result.ShouldBeTrue();
        (OpeningBalance + testInput.Sum(t => t.Amount)).ShouldBe(200M);
    }

    [Fact]
    public void ShouldSupplementUpToBudgetAmount_GivenClosingBalanceIsLessThanOpeningBalanceAndEqualToBudgetAmount()
    {
        Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
        var testInput = new List<LedgerTransaction>
        {
            new BudgetCreditLedgerTransaction { Amount = 125M, Date = this.reconciliationDate, Narrative = "Budget Amount" },
            new CreditLedgerTransaction { Amount = -200M, Date = new DateOnly(2013, 9, 11) }
        };
        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

        result.ShouldBeTrue();
        (OpeningBalance + testInput.Sum(t => t.Amount)).ShouldBe(OpeningBalance);
    }

    [Fact]
    public void ShouldSupplementUpToBudgetAmount_GivenClosingBalanceIsLessThanOpeningBalanceAndGreaterThanBudgetAmount()
    {
        Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
        var testInput = new List<LedgerTransaction>
        {
            new BudgetCreditLedgerTransaction { Amount = 100M, Date = this.reconciliationDate, Narrative = "Budget Amount" },
            new CreditLedgerTransaction { Amount = -200M, Date = new DateOnly(2013, 9, 11) }
        };
        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

        result.ShouldBeTrue();
        (OpeningBalance + testInput.Sum(t => t.Amount)).ShouldBe(100M);
    }

    [Fact]
    public void ShouldSupplementUpToBudgetAmount_GivenClosingBalanceIsLessThanOpeningBalanceAndLessThanBudgetAmount()
    {
        this.subject.Balance = 100;
        Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
        var testInput = new List<LedgerTransaction>
        {
            new BudgetCreditLedgerTransaction { Amount = 201M, Date = this.reconciliationDate, Narrative = "Budget Amount" },
            new CreditLedgerTransaction { Amount = -300M, Date = new DateOnly(2013, 9, 11) }
        };
        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

        result.ShouldBeTrue();
        (OpeningBalance + testInput.Sum(t => t.Amount)).ShouldBe(201M);
    }
}
