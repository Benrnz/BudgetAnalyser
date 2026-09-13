using System;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.LedgerBook;
using Shouldly;

namespace BudgetAnalyser.Wpf.XUnit3.LedgerBook;

public class BankBalanceViewModelTest
{
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenBalanceIsNull()
    {
        BankBalance balance = null!;

        Should.Throw<ArgumentNullException>(() => new BankBalanceViewModel(null, balance));
    }

    [Fact]
    public void ShowAdjustedBalance_ShouldBeFalse_WhenLineIsNull()
    {
        var subject = new BankBalanceViewModel(null, new ChequeAccount("Main"), 123.45M);

        subject.ShowAdjustedBalance.ShouldBeFalse();
    }

    [Fact]
    public void AdjustedBalance_ShouldReturnBalance_WhenLineIsNull()
    {
        var subject = new BankBalanceViewModel(null, new ChequeAccount("Main"), 123.45M);

        subject.AdjustedBalance.ShouldBe(123.45M);
    }

    [Fact]
    public void ShowAdjustedBalance_ShouldBeTrue_AndAdjustedBalance_ShouldIncludeMatchingAccountAdjustmentsOnly()
    {
        var account = new ChequeAccount("Main");
        var subject = new BankBalanceViewModel(CreateLine(), new BankBalance(account, 123.45M));

        subject.ShowAdjustedBalance.ShouldBeTrue();
        subject.AdjustedBalance.ShouldBe(133.45M);
    }

    [Fact]
    public void Constructor_ShouldCopyAccountAndBalanceFromBankBalance()
    {
        var account = new ChequeAccount("Main");
        var subject = new BankBalanceViewModel(null, new BankBalance(account, 123.45M));

        subject.Account.ShouldBe(account);
        subject.Balance.ShouldBe(123.45M);
    }

    private static LedgerEntryLine CreateLine()
    {
        return new LedgerEntryLine(
            new[]
            {
                new BankBalanceAdjustmentTransaction
                {
                    BankAccount = new ChequeAccount("Main"),
                    Amount = 10M
                },
                new BankBalanceAdjustmentTransaction
                {
                    BankAccount = new SavingsAccount("Main"),
                    Amount = 20M
                },
                new BankBalanceAdjustmentTransaction
                {
                    BankAccount = new ChequeAccount("Other"),
                    Amount = 30M
                }
            },
            Array.Empty<BankBalance>(),
            Array.Empty<LedgerEntry>());
    }
}
