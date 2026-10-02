using System.Runtime.ExceptionServices;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Matching;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Messaging;
using NSubstitute;
using Rees.Wpf.Contracts;
using Shouldly;

namespace BudgetAnalyser.Wpf.XUnit3.Matching;

public class NewRuleControllerTest
{
    private const string MalformedPattern = "[unclosed";
    private readonly IBudgetBucketRepository bucketRepo;
    private readonly ILogger logger;
    private readonly ITransactionRuleService ruleService;
    private readonly NewRuleController subject;
    private readonly UserPrompts userPrompts;

    public NewRuleControllerTest()
    {
        this.ruleService = Substitute.For<ITransactionRuleService>();
        this.bucketRepo = Substitute.For<IBudgetBucketRepository>();
        this.logger = Substitute.For<ILogger>();
        this.userPrompts = new UserPrompts(
            Substitute.For<IUserMessageBox>(),
            () => Substitute.For<IUserPromptOpenFile>(),
            () => Substitute.For<IUserPromptSaveFile>(),
            Substitute.For<IUserQuestionBoxYesNo>(),
            Substitute.For<IUserInputBox>());

        this.subject = new NewRuleController(Substitute.For<IMessenger>(), this.logger, this.userPrompts, this.ruleService, this.bucketRepo);
    }

    // ── Regular expression validation ────────────────────────────────────────
    [Fact]
    public void Initialize_ShouldResetUseRegularExpressionsToFalse()
    {
        this.subject.UseRegularExpressions = true;

        this.subject.Initialize();

        this.subject.UseRegularExpressions.ShouldBeFalse();
    }

    [Fact]
    public void CanExecuteSaveButton_ShouldBeFalse_WhenUsingRegularExpressionsAndPatternIsMalformed()
    {
        this.subject.Initialize();
        this.subject.Description.Value = MalformedPattern;

        this.subject.UseRegularExpressions = true;

        this.subject.ValidRegexPattern.ShouldBeFalse();
        this.subject.CanExecuteSaveButton.ShouldBeFalse();
    }

    [Fact]
    public void CanExecuteSaveButton_ShouldBeTrue_WhenPatternIsMalformedButNotUsingRegularExpressions()
    {
        this.subject.Initialize();
        this.subject.Description.Value = MalformedPattern;

        this.subject.UseRegularExpressions = false;

        // Without regular expressions the value is only ever compared as literal text, so it cannot be malformed.
        this.subject.ValidRegexPattern.ShouldBeTrue();
        this.subject.CanExecuteSaveButton.ShouldBeTrue();
    }

    [Fact]
    public void CanExecuteSaveButton_ShouldBeTrue_WhenUsingRegularExpressionsAndPatternIsValid()
    {
        this.subject.Initialize();
        this.subject.Description.Value = "^NETFLIX.*";

        this.subject.UseRegularExpressions = true;

        this.subject.ValidRegexPattern.ShouldBeTrue();
        this.subject.CanExecuteSaveButton.ShouldBeTrue();
    }

    // ── UseRegularExpressions ────────────────────────────────────────────────

    [Fact]
    public void SaveResponse_ShouldNotSetUseRegularExpressionsOnNewRule_WhenOptionIsNotTicked()
    {
        var createdRule = SaveNewRuleViaDialog(false);

        createdRule.UseRegularExpressions.ShouldBeFalse();
    }

    // ── Creating the rule ────────────────────────────────────────────────────

    [Fact]
    public void SaveResponse_ShouldSetUseRegularExpressionsOnNewRule_WhenOptionIsTicked()
    {
        var createdRule = SaveNewRuleViaDialog(true);

        createdRule.UseRegularExpressions.ShouldBeTrue();
    }

    [Fact]
    public void ValidRegexPattern_ShouldBeTrue_WhenMalformedCriteriaIsNotApplicable()
    {
        this.subject.Initialize();
        this.subject.Reference1.Value = MalformedPattern;
        this.subject.Reference1.Applicable = false;

        this.subject.UseRegularExpressions = true;

        this.subject.ValidRegexPattern.ShouldBeTrue();
    }

    /// <summary>
    ///     Showing the dialog builds a WPF collection view over the similar rules, so the round trip is run on an STA thread.
    /// </summary>
    private static void RunOnStaThread(Action action)
    {
        ExceptionDispatchInfo? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                failure = ExceptionDispatchInfo.Capture(ex);
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        failure?.Throw();
    }

    /// <summary>
    ///     Drives the full dialog round trip: show the dialog, then respond to it with the Save button, and return the rule the controller created.
    /// </summary>
    private MatchingRule SaveNewRuleViaDialog(bool useRegularExpressions)
    {
        MatchingRule? result = null;

        // A real messenger is required because the controller only acts on a dialog response carrying the correlation id it generated when the dialog was shown.
        RunOnStaThread(() =>
        {
            var messenger = new WeakReferenceMessenger();
            var ruleFromService = new MatchingRule(this.bucketRepo);
            this.ruleService
                .CreateNewRule(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<string>(), Arg.Any<decimal?>(), Arg.Any<bool>())
                .Returns(ruleFromService);

            var controller = new NewRuleController(messenger, this.logger, this.userPrompts, this.ruleService, this.bucketRepo);
            controller.Initialize();
            controller.Bucket = new SurplusBucket();
            controller.Description.Value = "^NETFLIX.*";
            controller.UseRegularExpressions = useRegularExpressions;

            var correlationId = Guid.Empty;
            messenger.Register<ShellDialogRequestMessage>(this, (_, message) => correlationId = message.CorrelationId);

            controller.ShowDialog([]);
            messenger.Send(new ShellDialogResponseMessage(controller, ShellDialogButton.Save) { CorrelationId = correlationId });

            result = controller.NewRule;
        });

        result.ShouldNotBeNull();
        return result!;
    }
}
