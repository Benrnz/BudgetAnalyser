using System;
using System.Linq;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Matching;
using CommunityToolkit.Mvvm.Messaging;
using NSubstitute;
using Rees.Wpf.Contracts;
using Shouldly;

namespace BudgetAnalyser.Wpf.XUnit3.Matching;

public class EditRulesControllerTest
{
    private readonly IApplicationDatabaseFacade applicationDatabaseFacade;
    private readonly IUserInputBox inputBox;
    private readonly IUserMessageBox messageBox;
    private readonly IMessenger messenger;
    private readonly NewRuleController newRuleController;
    private readonly ITransactionRuleService ruleService;
    private readonly EditRulesController subject;
    private readonly IUserQuestionBoxYesNo yesNoBox;

    public EditRulesControllerTest()
    {
        this.messenger = Substitute.For<IMessenger>();
        this.messageBox = Substitute.For<IUserMessageBox>();
        this.yesNoBox = Substitute.For<IUserQuestionBoxYesNo>();
        this.inputBox = Substitute.For<IUserInputBox>();
        this.ruleService = Substitute.For<ITransactionRuleService>();
        this.applicationDatabaseFacade = Substitute.For<IApplicationDatabaseFacade>();

        var userPrompts = new UserPrompts(
            this.messageBox,
            () => Substitute.For<IUserPromptOpenFile>(),
            () => Substitute.For<IUserPromptSaveFile>(),
            this.yesNoBox,
            this.inputBox);

        var logger = Substitute.For<ILogger>();
        var bucketRepo = Substitute.For<IBudgetBucketRepository>();
        this.newRuleController = new NewRuleController(this.messenger, logger, userPrompts, this.ruleService, bucketRepo);

        this.subject = new EditRulesController(this.messenger, userPrompts, this.newRuleController, this.ruleService, this.applicationDatabaseFacade);
    }

    // ── DeleteRuleCommand ────────────────────────────────────────────────────

    [Fact]
    public void DeleteRuleCommand_ShouldBecomeExecutableWhenRuleSelected()
    {
        var rule = CreateRule();

        this.subject.DeleteRuleCommand.CanExecute(null).ShouldBeFalse();
        this.subject.SelectedRule = rule;
        this.subject.DeleteRuleCommand.CanExecute(null).ShouldBeTrue();
    }

    [Fact]
    public void DeleteRuleCommand_ShouldReturnSameInstanceEveryTime()
    {
        var first = this.subject.DeleteRuleCommand;
        var second = this.subject.DeleteRuleCommand;

        first.ShouldBeSameAs(second);
    }

    [Fact]
    public void DeleteRuleCommand_Execute_WhenUserDeclines_DoesNotCallRuleServiceRemove()
    {
        this.subject.SelectedRule = CreateRule();
        this.yesNoBox.Show(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        this.subject.DeleteRuleCommand.Execute(null);

        this.ruleService.DidNotReceive().RemoveRule(Arg.Any<MatchingRule>());
    }

    [Fact]
    public void DeleteRuleCommand_Execute_WhenUserConfirms_CallsRuleServiceRemove()
    {
        var rule = CreateRule();
        this.subject.SelectedRule = rule;
        this.yesNoBox.Show(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        this.ruleService.RemoveRule(Arg.Any<MatchingRule>()).Returns(true);

        this.subject.DeleteRuleCommand.Execute(null);

        this.ruleService.Received(1).RemoveRule(rule);
    }

    [Fact]
    public void DeleteRuleCommand_Execute_WhenUserConfirmsAndRemoveSucceeds_ClearsSelectedRule()
    {
        this.subject.SelectedRule = CreateRule();
        this.yesNoBox.Show(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        this.ruleService.RemoveRule(Arg.Any<MatchingRule>()).Returns(true);

        this.subject.DeleteRuleCommand.Execute(null);

        this.subject.SelectedRule.ShouldBeNull();
    }

    [Fact]
    public void DeleteRuleCommand_Execute_WhenUserConfirmsAndRemoveSucceeds_NotifiesApplicationDatabase()
    {
        this.subject.SelectedRule = CreateRule();
        this.yesNoBox.Show(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        this.ruleService.RemoveRule(Arg.Any<MatchingRule>()).Returns(true);

        this.subject.DeleteRuleCommand.Execute(null);

        this.applicationDatabaseFacade.Received(1).NotifyOfChange(ApplicationDataType.MatchingRules);
    }

    [Fact]
    public void DeleteRuleCommand_Execute_WhenRuleServiceReturnsFalse_DoesNotNotifyApplicationDatabase()
    {
        this.subject.SelectedRule = CreateRule();
        this.yesNoBox.Show(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        this.ruleService.RemoveRule(Arg.Any<MatchingRule>()).Returns(false);

        this.subject.DeleteRuleCommand.Execute(null);

        this.applicationDatabaseFacade.DidNotReceive().NotifyOfChange(Arg.Any<ApplicationDataType>());
    }

    // ── EditRuleCommand ──────────────────────────────────────────────────────

    [Fact]
    public void EditRuleCommand_ShouldReturnSameInstanceEveryTime()
    {
        var first = this.subject.EditRuleCommand;
        var second = this.subject.EditRuleCommand;

        first.ShouldBeSameAs(second);
    }

    [Fact]
    public void EditRuleCommand_CanExecute_ReturnsFalseWhenNoRuleSelected()
    {
        this.subject.EditRuleCommand.CanExecute(null).ShouldBeFalse();
    }

    [Fact]
    public void EditRuleCommand_CanExecute_ReturnsTrueWhenRuleSelected()
    {
        this.subject.SelectedRule = CreateRule();

        this.subject.EditRuleCommand.CanExecute(null).ShouldBeTrue();
    }

    [Fact]
    public void EditRuleCommand_Execute_SetsEditingRuleTrue()
    {
        this.subject.SelectedRule = CreateRule();

        this.subject.EditRuleCommand.Execute(null);

        this.subject.EditingRule.ShouldBeTrue();
    }

    [Fact]
    public void EditRuleCommand_Execute_WhenAlreadyEditing_TogglesEditingRuleToFalse()
    {
        this.subject.SelectedRule = CreateRule();
        this.subject.EditRuleCommand.Execute(null); // enter edit mode

        this.subject.EditRuleCommand.Execute(null); // exit edit mode

        this.subject.EditingRule.ShouldBeFalse();
    }

    [Fact]
    public void EditRuleCommand_Execute_NotifiesApplicationDatabaseOfChange()
    {
        this.subject.SelectedRule = CreateRule();

        this.subject.EditRuleCommand.Execute(null);

        this.applicationDatabaseFacade.Received(1).NotifyOfChange(ApplicationDataType.MatchingRules);
    }

    // ── SortBy property ──────────────────────────────────────────────────────

    [Fact]
    public void SortBy_WhenSetToBucketSortKey_SetsGroupByListBoxVisibilityTrue()
    {
        this.subject.SortBy = EditRulesController.BucketSortKey;

        this.subject.GroupByListBoxVisibility.ShouldBeTrue();
    }

    [Fact]
    public void SortBy_WhenSetToBucketSortKey_SetsFlatListBoxVisibilityFalse()
    {
        this.subject.SortBy = EditRulesController.BucketSortKey;

        this.subject.FlatListBoxVisibility.ShouldBeFalse();
    }

    [Fact]
    public void SortBy_WhenSetToDescriptionSortKey_SetsFlatListBoxVisibilityTrue()
    {
        this.subject.SortBy = EditRulesController.DescriptionSortKey;

        this.subject.FlatListBoxVisibility.ShouldBeTrue();
    }

    [Fact]
    public void SortBy_WhenSetToDescriptionSortKey_SetsGroupByListBoxVisibilityFalse()
    {
        this.subject.SortBy = EditRulesController.DescriptionSortKey;

        this.subject.GroupByListBoxVisibility.ShouldBeFalse();
    }

    [Fact]
    public void SortBy_WhenSetToMatchesSortKey_SetsFlatListBoxVisibilityTrue()
    {
        this.subject.SortBy = EditRulesController.MatchesSortKey;

        this.subject.FlatListBoxVisibility.ShouldBeTrue();
    }

    [Fact]
    public void SortBy_WhenSetToMatchesSortKey_SetsGroupByListBoxVisibilityFalse()
    {
        this.subject.SortBy = EditRulesController.MatchesSortKey;

        this.subject.GroupByListBoxVisibility.ShouldBeFalse();
    }

    [Fact]
    public void SortBy_WhenSetToInvalidKey_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => this.subject.SortBy = "NotAValidSortKey");
    }

    [Fact]
    public void SortBy_WhenSetToInvalidKey_LeavesPropertyUnchanged()
    {
        this.subject.SortBy = EditRulesController.DescriptionSortKey;

        Should.Throw<ArgumentException>(() => this.subject.SortBy = "NotAValidSortKey");

        this.subject.SortBy.ShouldBe(EditRulesController.DescriptionSortKey);
    }

    // ── AndOrText property ───────────────────────────────────────────────────

    [Fact]
    public void AndOrText_WhenNoRuleSelected_ReturnsNull()
    {
        this.subject.AndOrText.ShouldBeNull();
    }

    [Fact]
    public void AndOrText_WhenRuleSelectedWithAndTrue_ReturnsAndText()
    {
        var rule = CreateRule();
        rule.And = true;
        this.subject.SelectedRule = rule;

        this.subject.AndOrText.ShouldBe("AND");
    }

    [Fact]
    public void AndOrText_WhenRuleSelectedWithAndFalse_ReturnsOrText()
    {
        var rule = CreateRule();
        rule.And = false;
        this.subject.SelectedRule = rule;

        this.subject.AndOrText.ShouldBe("OR");
    }

    // ── ShowReadOnlyRuleDetails property ─────────────────────────────────────

    [Fact]
    public void ShowReadOnlyRuleDetails_WhenNoRuleSelected_ReturnsFalse()
    {
        this.subject.ShowReadOnlyRuleDetails.ShouldBeFalse();
    }

    [Fact]
    public void ShowReadOnlyRuleDetails_WhenRuleSelectedAndNotEditing_ReturnsTrue()
    {
        this.subject.SelectedRule = CreateRule();

        this.subject.ShowReadOnlyRuleDetails.ShouldBeTrue();
    }

    [Fact]
    public void ShowReadOnlyRuleDetails_WhenEditingRule_ReturnsFalse()
    {
        this.subject.SelectedRule = CreateRule();
        this.subject.EditRuleCommand.Execute(null); // enter edit mode

        this.subject.ShowReadOnlyRuleDetails.ShouldBeFalse();
    }

    // ── SortCommand ──────────────────────────────────────────────────────────

    [Fact]
    public void SortCommand_WhenExecutedWithBucketKey_UpdatesSortByProperty()
    {
        this.subject.SortCommand.Execute(EditRulesController.BucketSortKey);

        this.subject.SortBy.ShouldBe(EditRulesController.BucketSortKey);
    }

    [Fact]
    public void SortCommand_WhenExecutedWithDescriptionKey_UpdatesSortByProperty()
    {
        this.subject.SortCommand.Execute(EditRulesController.DescriptionSortKey);

        this.subject.SortBy.ShouldBe(EditRulesController.DescriptionSortKey);
    }

    // ── ruleService event handlers ───────────────────────────────────────────

    [Fact]
    public void RuleServiceClosedEvent_ClearsRulesCollection()
    {
        ConfigureRuleServiceWithOneRule();
        this.ruleService.NewDataSourceAvailable += Raise.Event(); // populate Rules

        this.ruleService.Closed += Raise.Event();

        this.subject.Rules.ShouldBeEmpty();
    }

    [Fact]
    public void RuleServiceClosedEvent_ClearsGroupedRulesCollection()
    {
        ConfigureRuleServiceWithOneRule();
        this.ruleService.NewDataSourceAvailable += Raise.Event();

        this.ruleService.Closed += Raise.Event();

        this.subject.RulesGroupedByBucket.ShouldBeEmpty();
    }

    [Fact]
    public void RuleServiceClosedEvent_ClearsSelectedRule()
    {
        this.subject.SelectedRule = CreateRule();

        this.ruleService.Closed += Raise.Event();

        this.subject.SelectedRule.ShouldBeNull();
    }

    [Fact]
    public void RuleServiceSavedEvent_SetsEditingRuleToFalse()
    {
        this.subject.SelectedRule = CreateRule();
        this.subject.EditRuleCommand.Execute(null); // enter edit mode
        this.subject.EditingRule.ShouldBeTrue();

        this.ruleService.Saved += Raise.Event();

        this.subject.EditingRule.ShouldBeFalse();
    }

    [Fact]
    public void RuleServiceNewDataSourceAvailableEvent_PopulatesRulesFromService()
    {
        var rule = CreateRule();
        this.ruleService.MatchingRules.Returns(new[] { rule });
        this.ruleService.MatchingRulesGroupedByBucket.Returns(Enumerable.Empty<RulesGroupedByBucket>());

        this.ruleService.NewDataSourceAvailable += Raise.Event();

        this.subject.Rules.ShouldContain(rule);
    }

    [Fact]
    public void RuleServiceNewDataSourceAvailableEvent_SetsSortByToBucketSortKey()
    {
        this.ruleService.MatchingRules.Returns(Enumerable.Empty<MatchingRule>());
        this.ruleService.MatchingRulesGroupedByBucket.Returns(Enumerable.Empty<RulesGroupedByBucket>());

        this.ruleService.NewDataSourceAvailable += Raise.Event();

        this.subject.SortBy.ShouldBe(EditRulesController.BucketSortKey);
    }

    [Fact]
    public void RuleServiceNewDataSourceAvailableEvent_ClearsSelectedRule()
    {
        this.subject.SelectedRule = CreateRule();
        this.ruleService.MatchingRules.Returns(Enumerable.Empty<MatchingRule>());
        this.ruleService.MatchingRulesGroupedByBucket.Returns(Enumerable.Empty<RulesGroupedByBucket>());

        this.ruleService.NewDataSourceAvailable += Raise.Event();

        this.subject.SelectedRule.ShouldBeNull();
    }

    // ── ShowDialog ───────────────────────────────────────────────────────────

    [Fact]
    public void ShowDialog_WhenRulesIsEmpty_LoadsRulesFromService()
    {
        var rule = CreateRule();
        this.ruleService.MatchingRules.Returns(new[] { rule });
        this.ruleService.MatchingRulesGroupedByBucket.Returns(Enumerable.Empty<RulesGroupedByBucket>());

        this.subject.ShowDialog();

        this.subject.Rules.ShouldContain(rule);
    }

    [Fact]
    public void ShowDialog_WhenRulesAlreadyLoaded_DoesNotReloadFromService()
    {
        // Populate Rules first via the event, then call ShowDialog
        var firstRule = CreateRule();
        this.ruleService.MatchingRules.Returns(new[] { firstRule });
        this.ruleService.MatchingRulesGroupedByBucket.Returns(Enumerable.Empty<RulesGroupedByBucket>());
        this.ruleService.NewDataSourceAvailable += Raise.Event();
        this.ruleService.ClearReceivedCalls();

        this.subject.ShowDialog();

        this.ruleService.DidNotReceive().MatchingRules.GetEnumerator();
    }

    private void ConfigureRuleServiceWithOneRule()
    {
        this.ruleService.MatchingRules.Returns(new[] { CreateRule() });
        this.ruleService.MatchingRulesGroupedByBucket.Returns(Enumerable.Empty<RulesGroupedByBucket>());
    }

    private static MatchingRule CreateRule()
    {
        return new MatchingRule(Substitute.For<IBudgetBucketRepository>());
    }
}
