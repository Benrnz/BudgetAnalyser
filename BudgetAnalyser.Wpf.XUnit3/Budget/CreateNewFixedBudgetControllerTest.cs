#nullable enable
using System;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Messaging;
using NSubstitute;
using Rees.UnitTestUtilities;
using Rees.Wpf.Contracts;
using Shouldly;

namespace BudgetAnalyser.Wpf.XUnit3.Budget;

public class CreateNewFixedBudgetControllerTest
{
    private readonly BudgetBucketRepoAlwaysFind bucketRepo = new();
    private readonly IMessenger messenger = new WeakReferenceMessenger();
    private readonly IUserMessageBox messageBox = Substitute.For<IUserMessageBox>();

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenBucketRepositoryIsNull()
    {
        var userPrompts = CreateUserPrompts();

        Should.Throw<ArgumentNullException>(() => new CreateNewFixedBudgetController(this.messenger, userPrompts, null!));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenMessageBoxIsNull()
    {
        var userPrompts = CreateUserPrompts();
        PrivateAccessor.SetProperty(userPrompts, "MessageBox", null!);

        Should.Throw<ArgumentNullException>(() => new CreateNewFixedBudgetController(this.messenger, userPrompts, this.bucketRepo));
    }

    [Fact]
    public void ShowDialog_ShouldSendRequestMessage()
    {
        var subject = CreateSubject();
        ShellDialogRequestMessage? request = null;
        this.messenger.Register<object, ShellDialogRequestMessage>(this, (_, message) => request = message);

        subject.ShowDialog(BudgetAnalyserFeature.Budget, Guid.NewGuid());

        request.ShouldNotBeNull();
        request!.Content.ShouldBeSameAs(subject);
        request.Location.ShouldBe(BudgetAnalyserFeature.Budget);
        request.DialogType.ShouldBe(ShellDialogType.OkCancel);
        request.Title.ShouldBe("Create new fixed budget project");
        request.HelpAvailable.ShouldBeTrue();
    }

    [Fact]
    public void CanExecuteOkButton_ShouldBeFalse_WhenBucketCodeAlreadyExists()
    {
        var subject = CreateSubject();
        ShellDialogCommandRequerySuggestedMessage? requeryMessage = null;
        this.messenger.Register<object, ShellDialogCommandRequerySuggestedMessage>(this, (_, message) => requeryMessage = message);

        subject.Code = "surplus";
        subject.Description = "A new project";
        subject.Amount = 50M;

        subject.CanExecuteOkButton.ShouldBeFalse();
        requeryMessage.ShouldNotBeNull();
    }

    [Fact]
    public void ShowDialog_ThenHelp_ShouldDisplayHelpTextAndNotSendCompletion()
    {
        var subject = CreateSubject();
        ShellDialogRequestMessage? request = null;
        CreateNewFixedBudgetCompletedMessage? completed = null;
        this.messenger.Register<object, ShellDialogRequestMessage>(this, (_, message) => request = message);
        this.messenger.Register<object, CreateNewFixedBudgetCompletedMessage>(this, (_, message) => completed = message);

        subject.ShowDialog(BudgetAnalyserFeature.Budget, Guid.NewGuid());
        this.messenger.Send(new ShellDialogResponseMessage(subject, ShellDialogButton.Help) { CorrelationId = request!.CorrelationId });

        this.messageBox.Received(1).Show(Arg.Is<string>(text => text.Contains("special temporary budget bucket")));
        completed.ShouldBeNull();
    }

    [Fact]
    public void ShowDialog_ThenOk_ShouldSendCompletionMessage()
    {
        var subject = CreateSubject();
        ShellDialogRequestMessage? request = null;
        CreateNewFixedBudgetCompletedMessage? completed = null;
        this.messenger.Register<object, ShellDialogRequestMessage>(this, (_, message) => request = message);
        this.messenger.Register<object, CreateNewFixedBudgetCompletedMessage>(this, (_, message) => completed = message);

        subject.ShowDialog(BudgetAnalyserFeature.Budget, Guid.NewGuid());
        subject.Code = "proj";
        subject.Description = "Project";
        subject.Amount = 123.45M;
        this.messenger.Send(new ShellDialogResponseMessage(subject, ShellDialogButton.Ok) { CorrelationId = request!.CorrelationId });

        completed.ShouldNotBeNull();
        completed!.CorrelationId.ShouldBe(request!.CorrelationId);
        completed.Canceled.ShouldBeFalse();
        completed.Code.ShouldBe("proj");
        completed.Description.ShouldBe("Project");
        completed.Amount.ShouldBe(123.45M);
    }

    [Fact]
    public void ShowDialog_ThenCancel_ShouldSendCanceledCompletionMessage()
    {
        var subject = CreateSubject();
        ShellDialogRequestMessage? request = null;
        CreateNewFixedBudgetCompletedMessage? completed = null;
        this.messenger.Register<object, ShellDialogRequestMessage>(this, (_, message) => request = message);
        this.messenger.Register<object, CreateNewFixedBudgetCompletedMessage>(this, (_, message) => completed = message);

        subject.ShowDialog(BudgetAnalyserFeature.Budget, Guid.NewGuid());
        subject.Code = "proj";
        subject.Description = "Project";
        subject.Amount = 123.45M;
        this.messenger.Send(new ShellDialogResponseMessage(subject, ShellDialogButton.Cancel) { CorrelationId = request!.CorrelationId });

        completed.ShouldNotBeNull();
        completed!.Canceled.ShouldBeTrue();
    }

    [Fact]
    public void ShowDialog_ShouldIgnoreResponsesForDifferentCorrelationIds()
    {
        var subject = CreateSubject();
        ShellDialogRequestMessage? request = null;
        CreateNewFixedBudgetCompletedMessage? completed = null;
        this.messenger.Register<object, ShellDialogRequestMessage>(this, (_, message) => request = message);
        this.messenger.Register<object, CreateNewFixedBudgetCompletedMessage>(this, (_, message) => completed = message);

        subject.ShowDialog(BudgetAnalyserFeature.Budget, Guid.NewGuid());
        this.messenger.Send(new ShellDialogResponseMessage(subject, ShellDialogButton.Ok) { CorrelationId = Guid.NewGuid() });

        request.ShouldNotBeNull();
        completed.ShouldBeNull();
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

    private CreateNewFixedBudgetController CreateSubject()
    {
        return new CreateNewFixedBudgetController(this.messenger, CreateUserPrompts(), this.bucketRepo);
    }
}
