#nullable enable
using System;
using System.Linq;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Messaging;
using NSubstitute;
using Shouldly;

namespace BudgetAnalyser.Wpf.XUnit3.Budget;

public class ChooseBudgetBucketControllerTest
{
    private readonly BudgetBucketRepoAlwaysFind bucketRepo = new();
    private readonly IAccountTypeRepository accountRepo = Substitute.For<IAccountTypeRepository>();
    private readonly Account chequeAccount = new ChequeAccount("CHEQUE");
    private readonly Account savingsAccount = new SavingsAccount("SAVINGS");
    private readonly IMessenger messenger = new WeakReferenceMessenger();

    public ChooseBudgetBucketControllerTest()
    {
        this.accountRepo.ListCurrentlyUsedAccountTypes().Returns([this.chequeAccount, this.savingsAccount]);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenAccountRepositoryIsNull()
    {
        Should.Throw<ArgumentNullException>(() => new ChooseBudgetBucketController(this.messenger, this.bucketRepo, null!));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenBucketRepositoryIsNull()
    {
        Should.Throw<ArgumentNullException>(() => new ChooseBudgetBucketController(this.messenger, null!, this.accountRepo));
    }

    [Fact]
    public void Constructor_ShouldPopulateBucketAndAccountLists()
    {
        var subject = CreateSubject();

        subject.BudgetBuckets.Select(bucket => bucket.Code).ShouldBe(this.bucketRepo.Buckets.Select(bucket => bucket.Code));
        subject.BankAccounts.Select(account => account.Name).ShouldBe(["CHEQUE", "SAVINGS"]);
        subject.CanExecuteOkButton.ShouldBeFalse();
        subject.CanExecuteCancelButton.ShouldBeTrue();
        subject.CanExecuteSaveButton.ShouldBeFalse();
    }

    [Fact]
    public void Filter_ShouldReplaceTheBucketListAndRestoreItAfterCancel()
    {
        var subject = CreateSubject();
        var originalCodes = subject.BudgetBuckets.Select(bucket => bucket.Code).ToArray();
        var selectedBucket = this.bucketRepo.GetByCode("SURPLUS");
        ShellDialogRequestMessage? request = null;
        BudgetBucketChosenMessage? chosen = null;
        this.messenger.Register<object, ShellDialogRequestMessage>(this, (_, message) => request = message);
        this.messenger.Register<object, BudgetBucketChosenMessage>(this, (_, message) => chosen = message);

        subject.Filter(bucket => bucket.Code == selectedBucket!.Code, "Surplus only");
        subject.Selected = selectedBucket;
        subject.StoreInThisAccount = this.chequeAccount;
        subject.ShowDialog(BudgetAnalyserFeature.Budget, "Choose bucket", showBankAccountSelector: true);
        this.messenger.Send(new ShellDialogResponseMessage(subject, ShellDialogButton.Cancel) { CorrelationId = request!.CorrelationId });

        subject.FilterDescription.ShouldBe("Surplus only");
        subject.ShowBankAccount.ShouldBeTrue();
        subject.BudgetBuckets.Select(bucket => bucket.Code).ShouldBe(originalCodes);
        subject.Selected.ShouldBeNull();
        subject.StoreInThisAccount.ShouldBeNull();
        chosen.ShouldNotBeNull();
        chosen!.Canceled.ShouldBeTrue();
        chosen.SelectedBucket.ShouldBeNull();
        chosen.StoreInThisAccount.ShouldBeNull();
    }

    [Fact]
    public void Selected_ShouldRaiseCanExecuteRequeryAndEnableOk()
    {
        var subject = CreateSubject();
        var selectedBucket = this.bucketRepo.GetByCode("SURPLUS");
        ShellDialogCommandRequerySuggestedMessage? requeryMessage = null;
        this.messenger.Register<object, ShellDialogCommandRequerySuggestedMessage>(this, (_, message) => requeryMessage = message);

        subject.Selected = selectedBucket;

        subject.CanExecuteOkButton.ShouldBeTrue();
        requeryMessage.ShouldNotBeNull();
    }

    [Fact]
    public void ShowDialog_ShouldSendRequestMessage()
    {
        var subject = CreateSubject();
        ShellDialogRequestMessage? request = null;
        this.messenger.Register<object, ShellDialogRequestMessage>(this, (_, message) => request = message);

        subject.ShowDialog(BudgetAnalyserFeature.Budget, "Choose a bucket", correlationId: Guid.NewGuid(), showBankAccountSelector: true);

        request.ShouldNotBeNull();
        request!.Content.ShouldBeSameAs(subject);
        request.Location.ShouldBe(BudgetAnalyserFeature.Budget);
        request.DialogType.ShouldBe(ShellDialogType.OkCancel);
        request.Title.ShouldBe("Choose a bucket");
        subject.ShowBankAccount.ShouldBeTrue();
    }

    [Fact]
    public void ShowDialog_ThenOk_ShouldSendChosenBucketAndAccount()
    {
        var subject = CreateSubject();
        var selectedBucket = this.bucketRepo.GetByCode("SURPLUS");
        ShellDialogRequestMessage? request = null;
        BudgetBucketChosenMessage? chosen = null;
        this.messenger.Register<object, ShellDialogRequestMessage>(this, (_, message) => request = message);
        this.messenger.Register<object, BudgetBucketChosenMessage>(this, (_, message) => chosen = message);

        subject.ShowDialog(BudgetAnalyserFeature.Budget, "Choose a bucket");
        subject.Selected = selectedBucket;
        subject.StoreInThisAccount = this.savingsAccount;
        this.messenger.Send(new ShellDialogResponseMessage(subject, ShellDialogButton.Ok) { CorrelationId = request!.CorrelationId });

        chosen.ShouldNotBeNull();
        chosen!.Canceled.ShouldBeFalse();
        chosen.CorrelationId.ShouldBe(request!.CorrelationId);
        chosen.SelectedBucket.ShouldBe(selectedBucket);
        chosen.StoreInThisAccount.ShouldBe(this.savingsAccount);
        subject.Selected.ShouldBeNull();
        subject.StoreInThisAccount.ShouldBeNull();
    }

    [Fact]
    public void ShowDialog_ThenResponseForDifferentCorrelationId_ShouldBeIgnored()
    {
        var subject = CreateSubject();
        ShellDialogRequestMessage? request = null;
        BudgetBucketChosenMessage? chosen = null;
        this.messenger.Register<object, ShellDialogRequestMessage>(this, (_, message) => request = message);
        this.messenger.Register<object, BudgetBucketChosenMessage>(this, (_, message) => chosen = message);

        subject.ShowDialog(BudgetAnalyserFeature.Budget, "Choose a bucket");
        this.messenger.Send(new ShellDialogResponseMessage(subject, ShellDialogButton.Ok) { CorrelationId = Guid.NewGuid() });

        request.ShouldNotBeNull();
        chosen.ShouldBeNull();
        subject.Selected.ShouldBeNull();
    }

    private ChooseBudgetBucketController CreateSubject()
    {
        return new ChooseBudgetBucketController(this.messenger, this.bucketRepo, this.accountRepo);
    }
}
