#nullable disable
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.XUnit.TestData;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Ledger;

public class LedgerEntryLineTest
{
    private static readonly BankBalance NextReconcileBankBalance = new(TransactionsListModelTestData.ChequeAccount, 1850.5M);
    private static readonly DateOnly NextReconcileDate = new(2013, 09, 15);

    public LedgerEntryLineTest()
    {
        Subject = CreateSubject();
        NewSubject = new LedgerEntryLine(NextReconcileDate, new[] { NextReconcileBankBalance });
        NewSubject.SetNewLedgerEntries(new List<LedgerEntry>
        {
            new(true) { Balance = 120, LedgerBucket = LedgerBookTestData.ClothesLedger }, new(true) { Balance = 200, LedgerBucket = LedgerBookTestData.DocLedger }
        });
        NewSubject.Entries.First().SetTransactionsForReconciliation(new List<LedgerTransaction> { new CreditLedgerTransaction { Amount = -80 }, new BudgetCreditLedgerTransaction { Amount = 200 } });
    }

    private LedgerEntryLine NewSubject { get; }

    private LedgerEntryLine Subject { get; }

    [Fact]
    public void AddAdjustment_ShouldAffectLedgerBalance()
    {
        NewSubject.BalanceAdjustment(-101M, "foo dar far", new ChequeAccount("Chq"));

        NewSubject.LedgerBalance.ShouldBe(1749.50M);
    }

    [Fact]
    public void AddBalanceAdjustment_ShouldAddToAdjustmentCollection()
    {
        NewSubject.BalanceAdjustment(101M, "foo dar far", new ChequeAccount("Chq"));

        Subject.BankBalanceAdjustments.Count().ShouldBe(1);
    }

    [Fact]
    public void Output()
    {
        Console.WriteLine("Date: " + Subject.Date);
        Console.WriteLine("Remarks: " + Subject.Remarks);
        Console.Write("Entries: x{0} (", Subject.Entries.Count());
        foreach (var entry in Subject.Entries)
        {
            Console.Write("{0}, ", entry.LedgerBucket.BudgetBucket.Code);
        }

        Console.WriteLine(")");

        Console.WriteLine("Bank Balances:");
        foreach (var bankBalance in Subject.BankBalances)
        {
            Console.WriteLine("    {0} {1:N}", bankBalance.Account.Name, bankBalance.Balance);
        }

        Console.WriteLine("    ========================");
        Console.WriteLine("TotalBankBalance: " + Subject.TotalBankBalance);

        Console.WriteLine("Balance Adjustments:");
        foreach (var adjustment in Subject.BankBalanceAdjustments)
        {
            Console.WriteLine("    {0} {1} {2:N}", adjustment.BankAccount.Name, adjustment.Narrative, adjustment.Amount);
        }

        Console.WriteLine("    ========================");
        Console.WriteLine("TotalBalanceAdjustments: " + Subject.TotalBalanceAdjustments);

        Console.WriteLine();
        Console.WriteLine("Ledger Balance: " + Subject.LedgerBalance);

        Console.WriteLine();
        Console.WriteLine("Surplus Balances:");
        foreach (var surplusBalance in Subject.SurplusBalances)
        {
            Console.WriteLine("    {0} {1:N}", surplusBalance.Account.Name, surplusBalance.Balance);
        }

        Console.WriteLine("    ========================");
        Console.WriteLine("CalculatedSurplus aka Total Surplus: " + Subject.CalculatedSurplus);
    }

    [Fact]
    public void RemoveTransactionShouldGiveSurplus1555()
    {
        // Starting Surplus is $1,530.50
        var entry = NewSubject.Entries.First();
        entry.RemoveTransaction(entry.Transactions.First(t => t is CreditLedgerTransaction).Id);

        // The balance of a ledger cannot simply be calculated inside the ledger line. It must be recalc'ed at the ledger book level.
        NewSubject.CalculatedSurplus.ShouldBe(1530.50M); // It should be unchanged.
    }

    [Fact]
    public void SurplusBalancesShouldHave2Items()
    {
        var surplusBalances = Subject.SurplusBalances;
        surplusBalances.Count().ShouldBe(2);
    }

    [Fact]
    public void SurplusBalancesShouldHaveSavingsBalanceOf100()
    {
        var surplusBalances = Subject.SurplusBalances;
        surplusBalances.Single(b => b.Account.Name == TestDataConstants.SavingsAccountName).Balance.ShouldBe(100M);
    }

    [Fact]
    public void UpdateRemarks_ShouldSetRemarks()
    {
        NewSubject.UpdateRemarks("Foo bar");

        NewSubject.Remarks.ShouldBe("Foo bar");
    }

    private LedgerEntryLine CreateSubject()
    {
        return LedgerBookTestData.TestData5().Reconciliations.First();
    }
}
