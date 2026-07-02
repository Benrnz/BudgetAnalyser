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
    public void EditRuleCommand_ShouldReturnSameInstanceEveryTime()
    {
        var first = this.subject.EditRuleCommand;
        var second = this.subject.EditRuleCommand;

        first.ShouldBeSameAs(second);
    }

    private static MatchingRule CreateRule()
    {
        return new MatchingRule(Substitute.For<IBudgetBucketRepository>());
    }
}
