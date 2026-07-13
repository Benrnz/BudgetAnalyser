#nullable enable
using System;
using System.Linq;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.LedgerBook;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Messaging;
using NSubstitute;
using Rees.UnitTestUtilities;
using Rees.Wpf.Contracts;
using Shouldly;

namespace BudgetAnalyser.Wpf.XUnit3.LedgerBook;

public class LedgerBucketViewControllerTest
{
    private readonly Account chequeAccount = new ChequeAccount("CHEQUE");
    private readonly IAccountTypeRepository accountRepo = Substitute.For<IAccountTypeRepository>();
    private readonly ILedgerService ledgerService = Substitute.For<ILedgerService>();
    private readonly IUserMessageBox messageBox = Substitute.For<IUserMessageBox>();
    private readonly IMessenger messenger = new WeakReferenceMessenger();
    private readonly Account savingsAccount = new SavingsAccount("SAVINGS");

    public LedgerBucketViewControllerTest()
    {
        this.accountRepo.ListCurrentlyUsedAccountTypes().Returns(new[] { this.chequeAccount, this.savingsAccount });
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenAccountRepositoryIsNull()
    {
        var userPrompts = CreateUserPrompts();

        var ex = Should.Throw<ArgumentNullException>(
            () => new LedgerBucketViewController(this.messenger, userPrompts, null!, this.ledgerService));

        ex.ParamName.ShouldBe("accountRepo");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLedgerServiceIsNull()
    {
        var userPrompts = CreateUserPrompts();

        var ex = Should.Throw<ArgumentNullException>(
            () => new LedgerBucketViewController(this.messenger, userPrompts, this.accountRepo, null!));

        ex.ParamName.ShouldBe("ledgerService");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenMessengerIsNull()
    {
        var userPrompts = CreateUserPrompts();

        Should.Throw<ArgumentNullException>(() => new LedgerBucketViewController(null!, userPrompts, this.accountRepo, this.ledgerService));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenUserPromptsMessageBoxIsNull()
    {
        var userPrompts = CreateUserPrompts();
        PrivateAccessor.SetProperty(userPrompts, "MessageBox", null!);

        var ex = Should.Throw<ArgumentNullException>(
            () => new LedgerBucketViewController(this.messenger, userPrompts, this.accountRepo, this.ledgerService));

        ex.ParamName.ShouldBe("MessageBox");
    }

    [Fact]
    public void ShowDialog_ShouldPopulateStateAndSendRequestMessage()
    {
        var subject = CreateSubject();
        subject.BankAccounts.Add(new ChequeAccount("OLD"));
        subject.StoredInAccount = this.savingsAccount;
        var ledgerBook = LedgerBookTestData.TestData1();
        var ledgerBucket = ledgerBook.Ledgers.Single(l => l.BudgetBucket.Code == TestDataConstants.HairBucketCode);
        var budgetModel = BudgetModelTestData.CreateTestData1();
        ShellDialogRequestMessage? request = null;
        this.messenger.Register<object, ShellDialogRequestMessage>(this, (_, message) => request = message);

        subject.ShowDialog(ledgerBucket, budgetModel);

        subject.BankAccounts.Select(a => a.Name).ShouldBe(["CHEQUE", "SAVINGS"]);
        subject.BucketBeingTracked.ShouldBe(ledgerBucket.BudgetBucket);
        subject.StoredInAccount.ShouldBe(ledgerBucket.StoredInAccount);
        subject.BudgetAmount.ShouldBe(55M);
        subject.BudgetCycle.ShouldBe(budgetModel.BudgetCycle);
        request.ShouldNotBeNull();
        request!.Content.ShouldBeSameAs(subject);
        request.Location.ShouldBe(BudgetAnalyserFeature.LedgerBook);
        request.DialogType.ShouldBe(ShellDialogType.OkCancel);
        request.HelpAvailable.ShouldBeTrue();
        request.Title.ShouldBe("Ledger - " + ledgerBucket.BudgetBucket);
        request.CorrelationId.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void ShowDialog_ThenCancel_ShouldLeaveStateIntact()
    {
        var subject = CreateSubject();
        var ledgerBucket = LedgerBookTestData.TestData1().Ledgers.Single(l => l.BudgetBucket.Code == TestDataConstants.HairBucketCode);
        var budgetModel = BudgetModelTestData.CreateTestData1();
        ShellDialogRequestMessage? request = null;
        this.messenger.Register<object, ShellDialogRequestMessage>(this, (_, message) => request = message);

        subject.ShowDialog(ledgerBucket, budgetModel);
        this.messenger.Send(new ShellDialogResponseMessage(subject, ShellDialogButton.Cancel) { CorrelationId = request!.CorrelationId });

        subject.BankAccounts.Select(a => a.Name).ShouldBe(["CHEQUE", "SAVINGS"]);
        subject.BucketBeingTracked.ShouldBe(ledgerBucket.BudgetBucket);
        subject.StoredInAccount.ShouldBe(ledgerBucket.StoredInAccount);
        subject.BudgetAmount.ShouldBe(55M);
        this.ledgerService.DidNotReceive().MoveLedgerToAccount(Arg.Any<LedgerBucket>(), Arg.Any<Account>());
        this.messageBox.DidNotReceive().Show(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public void ShowDialog_ThenHelp_ShouldDisplayHelpAndLeaveStateIntact()
    {
        var subject = CreateSubject();
        var ledgerBucket = LedgerBookTestData.TestData1().Ledgers.Single(l => l.BudgetBucket.Code == TestDataConstants.HairBucketCode);
        var budgetModel = BudgetModelTestData.CreateTestData1();
        ShellDialogRequestMessage? request = null;
        this.messenger.Register<object, ShellDialogRequestMessage>(this, (_, message) => request = message);

        subject.ShowDialog(ledgerBucket, budgetModel);
        this.messenger.Send(new ShellDialogResponseMessage(subject, ShellDialogButton.Help) { CorrelationId = request!.CorrelationId });

        this.messageBox.Received(1).Show(
            Arg.Is<string>(text => text.Contains("Ledgers within the Ledger Book track the actual bank balance over time of a single Bucket.")),
            Arg.Any<string>());
        subject.BankAccounts.Select(a => a.Name).ShouldBe(["CHEQUE", "SAVINGS"]);
        subject.BudgetAmount.ShouldBe(55M);
        this.ledgerService.DidNotReceive().MoveLedgerToAccount(Arg.Any<LedgerBucket>(), Arg.Any<Account>());
    }

    [Fact]
    public void ShowDialog_ThenOkWithSameAccount_ShouldResetWithoutMovingTheLedger()
    {
        var subject = CreateSubject();
        var ledgerBook = LedgerBookTestData.TestData1();
        var ledgerBucket = ledgerBook.Ledgers.Single(l => l.BudgetBucket.Code == TestDataConstants.HairBucketCode);
        var budgetModel = BudgetModelTestData.CreateTestData1();
        ShellDialogRequestMessage? request = null;
        LedgerBucketUpdatedMessage? updatedMessage = null;
        this.messenger.Register<object, ShellDialogRequestMessage>(this, (_, message) => request = message);
        this.messenger.Register<object, LedgerBucketUpdatedMessage>(this, (_, message) => updatedMessage = message);

        subject.ShowDialog(ledgerBucket, budgetModel);
        this.messenger.Send(new ShellDialogResponseMessage(subject, ShellDialogButton.Ok) { CorrelationId = request!.CorrelationId });

        this.ledgerService.DidNotReceive().MoveLedgerToAccount(Arg.Any<LedgerBucket>(), Arg.Any<Account>());
        updatedMessage.ShouldBeNull();
        subject.BankAccounts.ShouldBeEmpty();
        subject.StoredInAccount.ShouldBeNull();
        subject.BudgetAmount.ShouldBe(0);
        subject.BucketBeingTracked.ShouldBe(ledgerBucket.BudgetBucket);
    }

    [Fact]
    public void ShowDialog_ThenOkWithDifferentAccount_ShouldMoveTheLedgerAndResetState()
    {
        var subject = CreateSubject();
        var ledgerBook = LedgerBookTestData.TestData1();
        var ledgerBucket = ledgerBook.Ledgers.Single(l => l.BudgetBucket.Code == TestDataConstants.HairBucketCode);
        var budgetModel = BudgetModelTestData.CreateTestData1();
        ShellDialogRequestMessage? request = null;
        LedgerBucketUpdatedMessage? updatedMessage = null;
        this.messenger.Register<object, ShellDialogRequestMessage>(this, (_, message) => request = message);
        this.messenger.Register<object, LedgerBucketUpdatedMessage>(this, (_, message) => updatedMessage = message);

        subject.ShowDialog(ledgerBucket, budgetModel);
        subject.StoredInAccount = this.savingsAccount;
        this.messenger.Send(new ShellDialogResponseMessage(subject, ShellDialogButton.Ok) { CorrelationId = request!.CorrelationId });

        this.ledgerService.Received(1).MoveLedgerToAccount(ledgerBucket, this.savingsAccount);
        updatedMessage.ShouldNotBeNull();
        updatedMessage!.WasChanged.ShouldBeTrue();
        subject.BankAccounts.ShouldBeEmpty();
        subject.StoredInAccount.ShouldBeNull();
        subject.BudgetAmount.ShouldBe(0);
    }

    [Fact]
    public void ShowDialog_ShouldIgnoreResponsesForDifferentCorrelationIds()
    {
        var subject = CreateSubject();
        var ledgerBucket = LedgerBookTestData.TestData1().Ledgers.Single(l => l.BudgetBucket.Code == TestDataConstants.HairBucketCode);
        var budgetModel = BudgetModelTestData.CreateTestData1();
        ShellDialogRequestMessage? request = null;
        this.messenger.Register<object, ShellDialogRequestMessage>(this, (_, message) => request = message);

        subject.ShowDialog(ledgerBucket, budgetModel);
        this.messenger.Send(new ShellDialogResponseMessage(subject, ShellDialogButton.Ok) { CorrelationId = Guid.NewGuid() });

        this.ledgerService.DidNotReceive().MoveLedgerToAccount(Arg.Any<LedgerBucket>(), Arg.Any<Account>());
        this.messageBox.DidNotReceive().Show(Arg.Any<string>(), Arg.Any<string>());
        subject.BankAccounts.Select(a => a.Name).ShouldBe(["CHEQUE", "SAVINGS"]);
        subject.StoredInAccount.ShouldBe(ledgerBucket.StoredInAccount);
        request.ShouldNotBeNull();
    }

    private LedgerBucketViewController CreateSubject()
    {
        return new LedgerBucketViewController(this.messenger, CreateUserPrompts(), this.accountRepo, this.ledgerService);
    }

    private UserPrompts CreateUserPrompts()
    {
        return new UserPrompts(
            this.messageBox,
            () => Substitute.For<IUserPromptOpenFile>(),
            () => Substitute.For<IUserPromptSaveFile>(),
            Substitute.For<IUserQuestionBoxYesNo>(),
            Substitute.For<IUserInputBox>());
    }
}
