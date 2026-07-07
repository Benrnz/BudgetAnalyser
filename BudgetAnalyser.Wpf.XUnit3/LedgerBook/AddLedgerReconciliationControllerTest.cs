#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.LedgerBook;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Messaging;
using NSubstitute;
using Rees.UnitTestUtilities;
using Rees.Wpf.Contracts;
using Shouldly;

namespace BudgetAnalyser.Wpf.XUnit3.LedgerBook;

public class AddLedgerReconciliationControllerTest
{
    private readonly Account chequeAccount = new ChequeAccount("CHEQUE");
    private readonly IUserMessageBox messageBox = Substitute.For<IUserMessageBox>();
    private readonly IMessenger messenger = new WeakReferenceMessenger();
    private readonly Account savingsAccount = new SavingsAccount("SAVINGS");
    private readonly AddLedgerReconciliationController subject;

    public AddLedgerReconciliationControllerTest()
    {
        IAccountTypeRepository accountTypeRepository1 = new TestAccountTypeRepository(this.savingsAccount, this.chequeAccount);
        var userPrompts = new UserPrompts(
            this.messageBox,
            () => Substitute.For<IUserPromptOpenFile>(),
            () => Substitute.For<IUserPromptSaveFile>(),
            Substitute.For<IUserQuestionBoxYesNo>(),
            Substitute.For<IUserInputBox>());
        this.subject = new AddLedgerReconciliationController(this.messenger, userPrompts, accountTypeRepository1);
    }

    [Fact]
    public void AddBankBalanceCommand_ShouldAddBalanceAndResetSelection_WhenInCreateMode()
    {
        PrivateAccessor.SetProperty(this.subject, "CreateMode", true);
        this.subject.SelectedBankAccount = this.chequeAccount;
        this.subject.BankBalance = 123.45M;

        this.subject.AddBankBalanceCommand.CanExecute(null).ShouldBeTrue();
        this.subject.AddBankBalanceCommand.Execute(null);

        this.subject.BankBalances.Count.ShouldBe(1);
        this.subject.BankBalances.Single().Account.ShouldBe(this.chequeAccount);
        this.subject.BankBalances.Single().Balance.ShouldBe(123.45M);
        this.subject.SelectedBankAccount.ShouldBeNull();
        this.subject.BankBalance.ShouldBe(0);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenAccountTypeRepositoryIsNull()
    {
        var userPrompts = CreateUserPrompts();

        Should.Throw<ArgumentNullException>(() => new AddLedgerReconciliationController(this.messenger, userPrompts, null!));
    }

    [Fact]
    public void OnShellDialogResponseReceived_Help_ShouldDisplayHelpText()
    {
        var correlationId = Guid.NewGuid();
        PrivateAccessor.SetField(this.subject, "dialogCorrelationId", correlationId);

        PrivateAccessor.InvokeMethod(
            this.subject,
            "OnShellDialogResponseReceived",
            new ShellDialogResponseMessage(this.subject, ShellDialogButton.Help)
            {
                CorrelationId = correlationId
            });

        this.messageBox.Received(1).Show(Arg.Is<string>(s => s.Contains("actual pay day")));
    }

    [Fact]
    public void OnShellDialogResponseReceived_Ok_ShouldSendCompletionAndResetState()
    {
        var book = CreateLedgerBook();
        var line = CreateLedgerEntryLine();
        this.subject.ShowViewDialog(book, line);
        PrivateAccessor.SetProperty(this.subject, "Editable", true);
        PrivateAccessor.SetProperty(this.subject, "CreateMode", true);
        this.subject.SelectedBankAccount = this.chequeAccount;
        this.subject.BankBalance = 123.45M;
        this.subject.AddBankBalanceCommand.Execute(null);

        var correlationId = Guid.NewGuid();
        PrivateAccessor.SetField(this.subject, "dialogCorrelationId", correlationId);

        PrivateAccessor.InvokeMethod(
            this.subject,
            "OnShellDialogResponseReceived",
            new ShellDialogResponseMessage(this.subject, ShellDialogButton.Ok) { CorrelationId = correlationId });

        this.subject.Canceled.ShouldBeFalse();
        this.subject.BankBalances.ShouldBeEmpty();
        this.subject.BankAccounts.ShouldBeEmpty();
        this.subject.SelectedBankAccount.ShouldBeNull();
    }

    [Fact]
    public void OnShellDialogResponseReceived_ShouldIgnoreMessagesForDifferentCorrelationId()
    {
        var before = new ObservableCollection<BankBalanceViewModel>(new[] { new BankBalanceViewModel(null, this.chequeAccount, 12M) });
        PrivateAccessor.SetProperty(this.subject, "BankBalances", before);
        var correlationId = Guid.NewGuid();
        PrivateAccessor.SetField(this.subject, "dialogCorrelationId", correlationId);

        PrivateAccessor.InvokeMethod(
            this.subject,
            "OnShellDialogResponseReceived",
            new ShellDialogResponseMessage(this.subject, ShellDialogButton.Ok) { CorrelationId = Guid.NewGuid() });

        this.subject.Canceled.ShouldBeFalse();
        this.subject.BankBalances.ShouldBeSameAs(before);
        this.messageBox.DidNotReceive().Show(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public void RemoveBankBalanceCommand_ShouldRemoveBalanceAndInvalidateRequiredBalances()
    {
        var book = CreateLedgerBook();
        var line = CreateLedgerEntryLine();
        this.subject.ShowViewDialog(book, line);
        PrivateAccessor.SetProperty(this.subject, "Editable", true);

        var bankBalance = this.subject.BankBalances.First();

        this.subject.RemoveBankBalanceCommand.CanExecute(bankBalance).ShouldBeTrue();
        this.subject.RemoveBankBalanceCommand.Execute(bankBalance);

        this.subject.BankBalances.Count.ShouldBe(1);
        this.subject.BankBalanceTotal.ShouldBe(200M);
        this.subject.HasRequiredBalances.ShouldBeFalse();
    }

    [Fact]
    public void ShowViewDialog_ShouldPopulateBankBalancesAndDisableEditing()
    {
        var book = CreateLedgerBook();
        var line = CreateLedgerEntryLine();

        this.subject.ShowViewDialog(book, line);

        this.subject.CreateMode.ShouldBeFalse();
        this.subject.Editable.ShouldBeFalse();
        this.subject.AddBalanceVisibility.ShouldBeFalse();
        this.subject.BankBalances.Count.ShouldBe(2);
        this.subject.BankBalances.Select(b => b.Account.Name).ShouldBe(["CHEQUE", "SAVINGS"]);
        this.subject.BankAccounts.Select(a => a.Name).ShouldBe(["CHEQUE", "SAVINGS"]);
        this.subject.HasRequiredBalances.ShouldBeTrue();
    }

    private Engine.Ledger.LedgerBook CreateLedgerBook()
    {
        var powerLedger = new SpentPerPeriodLedger
        {
            BudgetBucket = new SpentPerPeriodExpenseBucket("POWER", "Power"),
            StoredInAccount = this.chequeAccount
        };

        var savingsLedger = new SavedUpForLedger
        {
            BudgetBucket = new SavedUpForExpenseBucket("SAVINGS", "Savings"),
            StoredInAccount = this.savingsAccount
        };

        return new LedgerBookBuilder()
            .IncludeLedger(powerLedger)
            .IncludeLedger(savingsLedger)
            .AppendReconciliation(new DateOnly(2024, 6, 15), new BankBalance(this.chequeAccount, 100M), new BankBalance(this.savingsAccount, 200M))
            .WithReconciliationEntries(entryBuilder =>
            {
                entryBuilder.WithLedger(powerLedger).HasNoTransactions();
                entryBuilder.WithLedger(savingsLedger).HasNoTransactions();
            })
            .Build();
    }

    private LedgerEntryLine CreateLedgerEntryLine()
    {
        return new LedgerEntryLine(
            new DateOnly(2024, 6, 15),
            [
                new BankBalance(this.chequeAccount, 100M),
                new BankBalance(this.savingsAccount, 200M)
            ]);
    }

    private static UserPrompts CreateUserPrompts()
    {
        var messageBox = Substitute.For<IUserMessageBox>();
        return new UserPrompts(
            messageBox,
            () => Substitute.For<IUserPromptOpenFile>(),
            () => Substitute.For<IUserPromptSaveFile>(),
            Substitute.For<IUserQuestionBoxYesNo>(),
            Substitute.For<IUserInputBox>());
    }

    private sealed class TestAccountTypeRepository(params Account[] accounts) : IAccountTypeRepository
    {
        private readonly IEnumerable<Account> accounts = accounts;

        public Account? GetByKey(string key)
        {
            return this.accounts.FirstOrDefault(account => account.Name == key);
        }

        public IEnumerable<Account> ListCurrentlyUsedAccountTypes()
        {
            return this.accounts;
        }
    }
}
