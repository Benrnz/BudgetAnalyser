using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.UnitTest.Helper;
using BudgetAnalyser.Engine.UnitTest.TestData;

namespace BudgetAnalyser.Engine.UnitTest.Ledger;

/// <summary>
///     These tests resulted from investigating Issue 83.
/// </summary>
[TestClass]
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
    private DateOnly reconciliationDate;
    private LedgerEntry subject;
    private SpentPerPeriodLedger subject2;

    [TestMethod]
    [Description("Test Case 6.1")]
    public void ShouldAddCompensatingTransaction_GivenOpeningBalanceEqualsClosingBalanceAndNoBudgetAmount()
    {
        this.subject.Balance = 1;
        Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
        var testInput = new List<LedgerTransaction>
        {
            new CreditLedgerTransaction { Amount = 1, Date = new DateOnly(2013, 9, 11) }, new CreditLedgerTransaction { Amount = -1, Date = new DateOnly(2013, 9, 11) }
        };
        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

        Assert.IsTrue(result);
        Assert.AreEqual(3, testInput.Count());
    }

    [TestMethod]
    [Description("Test case 5")]
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

        Assert.IsTrue(result);
        Assert.AreEqual(3, testInput.Count());
    }

    [TestMethod]
    [Description("Test case 5.1")]
    public void ShouldCreateRemoveExcessTransaction_GivenClosingBalanceIsGreaterThanOpeningBalanceAndNoBudgetAmount()
    {
        this.subject.Balance = 0;
        Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
        var testInput = new List<LedgerTransaction> { new CreditLedgerTransaction { Amount = 1M, Date = new DateOnly(2013, 9, 11) } };
        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

        Assert.IsTrue(result);
        Assert.AreEqual(2, testInput.Count());
    }

    [TestMethod]
    [Description("Test case 4")]
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

        Assert.IsTrue(result);
        Assert.AreEqual(3, testInput.Count());
    }

    [TestMethod]
    [Description("Test case 3")]
    public void ShouldCreateSupplementTransaction_GivenClosingBalanceIsLessThanBudgetAmountAndLessThanOrEqualToOpeningBalance()
    {
        var testInput = new List<LedgerTransaction>
        {
            new BudgetCreditLedgerTransaction { Amount = 1, Date = new DateOnly(2013, 9, 11), Narrative = "Budget Amount" },
            new CreditLedgerTransaction { Amount = -1M, Date = new DateOnly(2013, 9, 11) }
        };
        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, 0);

        Assert.IsTrue(result);
        Assert.AreEqual(3, testInput.Count());
    }

    [TestMethod]
    [Description("Test case 7")]
    public void ShouldNotAddCompensatingTransaction_GivenClosingBalanceIsEqualToBudgetAndNoWithdrawals()
    {
        var testInput = new List<LedgerTransaction> { new BudgetCreditLedgerTransaction { Amount = 1, Date = this.reconciliationDate, Narrative = "Budget Amount" } };
        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, 0);

        Assert.IsFalse(result);
        Assert.AreEqual(1, testInput.Count());
    }


    [TestMethod]
    [Description("Test case 8")]
    public void ShouldNotAddCompensatingTransaction_GivenClosingBalanceIsEqualToOpeningBalanceAndBudget()
    {
        Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
        var testInput = new List<LedgerTransaction>
        {
            new BudgetCreditLedgerTransaction { Amount = OpeningBalance, Date = this.reconciliationDate, Narrative = "Budget Amount" },
            new CreditLedgerTransaction { Amount = -OpeningBalance, Date = new DateOnly(2013, 9, 11) }
        };
        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

        Assert.IsFalse(result);
        Assert.AreEqual(2, testInput.Count());
    }

    [TestMethod]
    [Description("Test case 1")]
    public void ShouldNotAddCompensatingTransaction_GivenOpeningBalanceAndClosingBalanceAndBudgetAreAllZero()
    {
        this.subject.Balance = 0;
        Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
        var testInput = new List<LedgerTransaction>();
        this.subject.SetTransactionsForReconciliation(testInput);
        this.subject.Output();

        Assert.AreEqual(0, this.subject.Transactions.Count());
    }

    [TestMethod]
    [Description("Test Case 6")]
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

        Assert.IsFalse(result);
        Assert.AreEqual(3, testInput.Count());
    }

    [TestMethod]
    [Description("Test case 2.1 - budget amount == closing balance")]
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

        Assert.IsFalse(result);
        Assert.AreEqual(2, testInput.Count());
    }

    [TestMethod]
    [Description("Test case 2 - no budget amount allocated at all")]
    public void ShouldNotCreateSupplementTransaction_GivenClosingBalanceIsLessThanOpeningBalanceAndNoBudgetAmount()
    {
        var testInput = new List<LedgerTransaction> { new CreditLedgerTransaction { Amount = -1, Date = new DateOnly(2013, 9, 11) } };
        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, 1);

        Assert.IsFalse(result);
        Assert.AreEqual(1, testInput.Count());
    }

    [TestMethod]
    [Description("Test case 5.1 Budget amount > Opening Balance (aka Previous Balance)")]
    public void ShouldOnlyRemoveExcessUpToBudgetAmount_GivenBudgetAmountIsGreaterThanOpeningBalance()
    {
        Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
        var testInput = new List<LedgerTransaction>
        {
            new BudgetCreditLedgerTransaction { Amount = 175M, Date = this.reconciliationDate, Narrative = "Budget Amount" },
            new CreditLedgerTransaction { Amount = -75M, Date = new DateOnly(2013, 9, 11) }
        };
        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

        Assert.IsTrue(result);
        Assert.AreEqual(175M, OpeningBalance + testInput.Sum(t => t.Amount));
    }

    [TestMethod]
    [Description("Test case 5.2 Opening Balance > Budget amount")]
    public void ShouldOnlyRemoveExcessUpToOpeningBalance_GivenOpeningBalanceIsGreaterThanBudgetAmount()
    {
        Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
        var testInput = new List<LedgerTransaction>
        {
            new BudgetCreditLedgerTransaction { Amount = 105M, Date = this.reconciliationDate, Narrative = "Budget Amount" },
            new CreditLedgerTransaction { Amount = -75M, Date = new DateOnly(2013, 9, 11) }
        };
        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

        Assert.IsTrue(result);
        Assert.AreEqual(125M, OpeningBalance + testInput.Sum(t => t.Amount));
    }

    [TestMethod]
    public void ShouldRemoveExcessToZero_GivenNetDifferenceOfZeroAndNoBudgetAmount()
    {
        this.subject.Balance = 100;
        Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
        var testInput = new List<LedgerTransaction>
        {
            new CreditLedgerTransaction { Amount = 300M, Date = new DateOnly(2013, 9, 11) }, new CreditLedgerTransaction { Amount = -300M, Date = new DateOnly(2013, 9, 11) }
        };

        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

        Assert.IsTrue(result);
        Assert.AreEqual(0M, OpeningBalance + testInput.Sum(t => t.Amount));
    }

    [TestMethod]
    public void ShouldSupplementToBudgetAmount_GivenNetDifferenceOfZeroAndClosingBalanceIsLessThanBudgetAmount()
    {
        this.subject.Balance = 100;
        Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
        var testInput = new List<LedgerTransaction>
        {
            new BudgetCreditLedgerTransaction { Amount = 300M, Date = new DateOnly(2013, 9, 11) }, new CreditLedgerTransaction { Amount = -300M, Date = new DateOnly(2013, 9, 11) }
        };

        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

        Assert.IsTrue(result);
        Assert.AreEqual(300M, OpeningBalance + testInput.Sum(t => t.Amount));
    }

    [TestMethod]
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

        Assert.AreEqual(3552.20M, this.subject.Balance);
    }

    [TestMethod]
    public void ShouldSupplementToZero_GivenNoBudgetAmountAndClosingBalanceIsLessThanZero()
    {
        this.subject.Balance = 100;
        Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
        var testInput = new List<LedgerTransaction> { new CreditLedgerTransaction { Amount = -300M, Date = new DateOnly(2013, 9, 11) } };
        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

        Assert.IsTrue(result);
        Assert.AreEqual(0M, OpeningBalance + testInput.Sum(t => t.Amount));
    }

    [TestMethod]
    [Description("Test case 3.1 Closing Balance < Budget Amount 200 && Budget Amount 200 > Opening Balance 125. Closing balance cannot be less than budget amount")]
    public void ShouldSupplementUpToBudgetAmount_GivenClosingBalanceIsLessThanBudgetAmountAndBudgetAmountIsGreaterThanOpeningBalance()
    {
        Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
        var testInput = new List<LedgerTransaction>
        {
            new BudgetCreditLedgerTransaction { Amount = 200M, Date = this.reconciliationDate, Narrative = "Budget Amount" },
            new CreditLedgerTransaction { Amount = -200M, Date = new DateOnly(2013, 9, 11) }
        };
        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

        Assert.IsTrue(result);
        Assert.AreEqual(200M, OpeningBalance + testInput.Sum(t => t.Amount));
    }

    [TestMethod]
    [Description("Test case 4.1 Closing Balance 50 < Opening Balance && Opening Balance == Budget Amount 125. Closing balance cannot be less than budget amount")]
    public void ShouldSupplementUpToBudgetAmount_GivenClosingBalanceIsLessThanOpeningBalanceAndEqualToBudgetAmount()
    {
        Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
        var testInput = new List<LedgerTransaction>
        {
            new BudgetCreditLedgerTransaction { Amount = 125M, Date = this.reconciliationDate, Narrative = "Budget Amount" },
            new CreditLedgerTransaction { Amount = -200M, Date = new DateOnly(2013, 9, 11) }
        };
        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

        Assert.IsTrue(result);
        Assert.AreEqual(OpeningBalance, OpeningBalance + testInput.Sum(t => t.Amount));
    }

    [TestMethod]
    [Description("Test case 2.1 Closing Balance 25 < Opening Balance 125 && Opening Balance 125 > Budget Amount 100. Closing balance cannot be less than budget amount")]
    public void ShouldSupplementUpToBudgetAmount_GivenClosingBalanceIsLessThanOpeningBalanceAndGreaterThanBudgetAmount()
    {
        Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
        var testInput = new List<LedgerTransaction>
        {
            new BudgetCreditLedgerTransaction { Amount = 100M, Date = this.reconciliationDate, Narrative = "Budget Amount" },
            new CreditLedgerTransaction { Amount = -200M, Date = new DateOnly(2013, 9, 11) }
        };
        var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

        Assert.IsTrue(result);
        Assert.AreEqual(100M, OpeningBalance + testInput.Sum(t => t.Amount));
    }

    [TestMethod]
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

        Assert.IsTrue(result);
        Assert.AreEqual(201M, OpeningBalance + testInput.Sum(t => t.Amount));
    }

    [TestInitialize]
    public void TestInitialise()
    {
        this.reconciliationDate = new DateOnly(2013, 9, 20);

        this.subject2 = new SpentPerPeriodLedger { BudgetBucket = StatementModelTestData.PowerBucket, StoredInAccount = StatementModelTestData.ChequeAccount };
        this.subject = new LedgerEntry(true) { LedgerBucket = LedgerBookTestData.PowerLedger, Balance = OpeningBalance };
    }
}
