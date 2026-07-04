using System.Collections.ObjectModel;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.Matching;

[AutoRegisterWithIoC(SingleInstance = true)]
public class DisusedRulesController : ControllerBase
{
    private readonly IApplicationDatabaseFacade dbService;
    private readonly ITransactionRuleService ruleService;
    private Guid dialogCorrelationId = Guid.NewGuid();
    private List<MatchingRule> removedRules = new();

    public DisusedRulesController(IMessenger messenger, ITransactionRuleService ruleService, IApplicationDatabaseFacade dbService) : base(messenger)
    {
        this.ruleService = ruleService;
        this.dbService = dbService;
        if (messenger is null)
        {
            throw new ArgumentNullException(nameof(messenger));
        }

        if (ruleService is null)
        {
            throw new ArgumentNullException(nameof(ruleService));
        }

        if (dbService is null)
        {
            throw new ArgumentNullException(nameof(dbService));
        }

        RemoveRuleCommand = new RelayCommand<DisusedRuleViewModel?>(OnRemoveRuleExecuted, r => r is not null);
        Messenger.Register<DisusedRulesController, ShellDialogResponseMessage>(this, static (r, m) => r.OnShellDialogResponseReceived(m));
    }

    public ObservableCollection<DisusedRuleViewModel> DisusedRules { get; private set; } = new();

    public IRelayCommand<DisusedRuleViewModel?> RemoveRuleCommand { get; }

    public void ShowDialog()
    {
        var rules = DisusedMatchingRuleWidget.QueryRules(this.ruleService.MatchingRules);
        DisusedRules = new ObservableCollection<DisusedRuleViewModel>(rules.Select(r => new DisusedRuleViewModel { MatchingRule = r, RemoveCommand = RemoveRuleCommand }));
        this.removedRules = new List<MatchingRule>();
        Messenger.Send(new ShellDialogRequestMessage(BudgetAnalyserFeature.Dashboard, this, ShellDialogType.Close)
        {
            CorrelationId = this.dialogCorrelationId,
            Title = "Disused Matching Rules"
        });
    }

    private void OnRemoveRuleExecuted(DisusedRuleViewModel? rule)
    {
        if (rule is null)
        {
            return;
        }

        DisusedRules.Remove(rule);
        this.removedRules.Add(rule.MatchingRule);
    }

    private void OnShellDialogResponseReceived(ShellDialogResponseMessage message)
    {
        if (!message.IsItForMe(this.dialogCorrelationId))
        {
            return;
        }

        if (this.removedRules.Any())
        {
            foreach (var rule in this.removedRules)
            {
                this.ruleService.RemoveRule(rule);
            }

            this.dbService.NotifyOfChange(ApplicationDataType.MatchingRules);
        }

        Reset();
    }

    private void Reset()
    {
        this.removedRules = new List<MatchingRule>();
        DisusedRules = new ObservableCollection<DisusedRuleViewModel>();
        this.dialogCorrelationId = Guid.NewGuid();
    }
}
