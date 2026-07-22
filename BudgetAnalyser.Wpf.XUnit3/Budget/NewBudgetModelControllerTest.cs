#nullable enable
using System;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Messaging;
using NSubstitute;
using Rees.UnitTestUtilities;
using Rees.Wpf.Contracts;
using Shouldly;

namespace BudgetAnalyser.Wpf.XUnit3.Budget;

public class NewBudgetModelControllerTest
{
    private readonly IUserMessageBox messageBox = Substitute.For<IUserMessageBox>();
    private readonly IMessenger messenger = new WeakReferenceMessenger();

    [Fact]
    public void Constructor_ShouldDefaultBudgetCycleToMonthly()
    {
        var subject = CreateSubject();

        subject.BudgetCycle.ShouldBe(BudgetCycle.Monthly);
        subject.MonthlyChecked.ShouldBeTrue();
        subject.FortnightlyChecked.ShouldBeFalse();
        subject.CanExecuteCancelButton.ShouldBeTrue();
        subject.CanExecuteOkButton.ShouldBeFalse();
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenMessageBoxIsNull()
    {
        var userPrompts = CreateUserPrompts();
        PrivateAccessor.SetProperty(userPrompts, "MessageBox", null!);

        Should.Throw<ArgumentNullException>(() => new NewBudgetModelController(this.messenger, userPrompts));
    }

    [Fact]
    public void EffectiveFrom_ShouldRaiseCanExecuteRequeryAndEnableSave()
    {
        var subject = CreateSubject();
        ShellDialogCommandRequerySuggestedMessage? requeryMessage = null;
        this.messenger.Register<object, ShellDialogCommandRequerySuggestedMessage>(this, (_, message) => requeryMessage = message);

        subject.EffectiveFrom = DateOnlyExt.Today().AddDays(1);

        subject.CanExecuteSaveButton.ShouldBeTrue();
        requeryMessage.ShouldNotBeNull();
    }

    [Fact]
    public void ShowDialog_ShouldIgnoreResponsesForDifferentCorrelationIds()
    {
        var subject = CreateSubject();
        ShellDialogRequestMessage? request = null;
        NewBudgetModelReadyMessage? ready = null;
        this.messenger.Register<object, ShellDialogRequestMessage>(this, (_, message) => request = message);
        this.messenger.Register<object, NewBudgetModelReadyMessage>(this, (_, message) => ready = message);

        subject.ShowDialog(DateOnlyExt.Today().AddDays(1));
        this.messenger.Send(new ShellDialogResponseMessage(subject, ShellDialogButton.Ok) { CorrelationId = Guid.NewGuid() });

        request.ShouldNotBeNull();
        ready.ShouldBeNull();
    }

    [Fact]
    public void ShowDialog_ShouldSendRequestMessage()
    {
        var subject = CreateSubject();
        ShellDialogRequestMessage? request = null;
        this.messenger.Register<object, ShellDialogRequestMessage>(this, (_, message) => request = message);
        var effectiveFrom = DateOnlyExt.Today().AddDays(1);

        subject.ShowDialog(effectiveFrom);

        request.ShouldNotBeNull();
        request!.Content.ShouldBeSameAs(subject);
        request.Location.ShouldBe(BudgetAnalyserFeature.Budget);
        request.DialogType.ShouldBe(ShellDialogType.SaveCancel);
        request.Title.ShouldBe("Create new Budget based on current");
        request.HelpAvailable.ShouldBeTrue();
        request.CorrelationId.ShouldNotBe(Guid.Empty);
        subject.EffectiveFrom.ShouldBe(effectiveFrom);
    }

    [Fact]
    public void ShowDialog_ThenCancel_ShouldNotSendReadyMessage()
    {
        var subject = CreateSubject();
        ShellDialogRequestMessage? request = null;
        NewBudgetModelReadyMessage? ready = null;
        this.messenger.Register<object, ShellDialogRequestMessage>(this, (_, message) => request = message);
        this.messenger.Register<object, NewBudgetModelReadyMessage>(this, (_, message) => ready = message);

        subject.ShowDialog(DateOnlyExt.Today().AddDays(1));
        this.messenger.Send(new ShellDialogResponseMessage(subject, ShellDialogButton.Cancel) { CorrelationId = request!.CorrelationId });

        ready.ShouldBeNull();
    }

    [Fact]
    public void ShowDialog_ThenHelp_ShouldDisplayHelpTextAndNotSendReadyMessage()
    {
        var subject = CreateSubject();
        ShellDialogRequestMessage? request = null;
        NewBudgetModelReadyMessage? ready = null;
        this.messenger.Register<object, ShellDialogRequestMessage>(this, (_, message) => request = message);
        this.messenger.Register<object, NewBudgetModelReadyMessage>(this, (_, message) => ready = message);

        subject.ShowDialog(DateOnlyExt.Today().AddDays(1));
        this.messenger.Send(new ShellDialogResponseMessage(subject, ShellDialogButton.Help) { CorrelationId = request!.CorrelationId });

        this.messageBox.Received(1).Show(Arg.Is<string>(text => text!.Contains("future dated")));
        ready.ShouldBeNull();
    }

    [Fact]
    public void ShowDialog_ThenOk_ShouldSendReadyMessage()
    {
        var subject = CreateSubject();
        subject.BudgetCycle = BudgetCycle.Fortnightly;
        ShellDialogRequestMessage? request = null;
        NewBudgetModelReadyMessage? ready = null;
        this.messenger.Register<object, ShellDialogRequestMessage>(this, (_, message) => request = message);
        this.messenger.Register<object, NewBudgetModelReadyMessage>(this, (_, message) => ready = message);
        var effectiveFrom = DateOnlyExt.Today().AddDays(1);

        subject.ShowDialog(effectiveFrom);
        this.messenger.Send(new ShellDialogResponseMessage(subject, ShellDialogButton.Ok) { CorrelationId = request!.CorrelationId });

        ready.ShouldNotBeNull();
        ready!.CorrelationId.ShouldBe(request!.CorrelationId);
        ready.EffectiveFrom.ShouldBe(effectiveFrom);
        ready.BudgetCycle.ShouldBe(BudgetCycle.Fortnightly);
    }

    private NewBudgetModelController CreateSubject()
    {
        return new NewBudgetModelController(this.messenger, CreateUserPrompts());
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
