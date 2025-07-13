using System.Windows.Input;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Statement;
using CommunityToolkit.Mvvm.Input;
using Rees.Wpf;
using Rees.Wpf.Contracts;

namespace BudgetAnalyser.Matching;

[AutoRegisterWithIoC(SingleInstance = true)]
public class AppliedRulesController : ControllerBase
{
    private readonly IApplicationDatabaseFacade applicationDatabaseService;
    private readonly IUserMessageBox messageBox;
    private readonly ITransactionRuleService ruleService;
    private readonly TabTransactionsController tabTransactionsController;
    private bool doNotUseDirty;

    public AppliedRulesController(IUiContext uiContext, ITransactionRuleService ruleService, IApplicationDatabaseFacade applicationDatabaseService)
        : base(uiContext.Messenger)
    {
        if (uiContext is null)
        {
            throw new ArgumentNullException(nameof(uiContext));
        }

        // TODO Direct controller references are not ideal.
        RulesController = uiContext.Controller<RulesController>();
        this.ruleService = ruleService ?? throw new ArgumentNullException(nameof(ruleService));
        this.applicationDatabaseService = applicationDatabaseService ?? throw new ArgumentNullException(nameof(applicationDatabaseService));
        this.tabTransactionsController = uiContext.Controller<TabTransactionsController>();
        this.messageBox = uiContext.UserPrompts.MessageBox;
        this.ruleService.Saved += OnSavedNotificationReceived;
    }

    [UsedImplicitly]
    public ICommand ApplyRulesCommand => new RelayCommand(OnApplyRulesCommandExecute, CanExecuteApplyRulesCommand);

    [UsedImplicitly]
    public ICommand CreateRuleCommand => new RelayCommand(OnCreateRuleCommandExecute, CanExecuteCreateRuleCommand);

    public bool Dirty
    {
        get => this.doNotUseDirty;

        set
        {
            this.doNotUseDirty = value;
            if (Dirty)
            {
                this.applicationDatabaseService.NotifyOfChange(ApplicationDataType.MatchingRules);
            }

            OnPropertyChanged();
        }
    }

    public RulesController RulesController { get; }

    [UsedImplicitly]
    public ICommand ShowRulesCommand => new RelayCommand(OnShowRulesCommandExecute);

    private bool CanExecuteApplyRulesCommand()
    {
        return RulesController.RulesGroupedByBucket.Any();
    }

    private bool CanExecuteCreateRuleCommand()
    {
        return this.tabTransactionsController.ViewModel.SelectedRow is not null;
    }

    private void OnApplyRulesCommandExecute()
    {
        var statement = this.tabTransactionsController.ViewModel.Statement ?? throw new InvalidOperationException("The statement model is null, not initialised or not loaded.");
        if (this.ruleService.Match(statement.Transactions))
        {
            Dirty = true;
            this.applicationDatabaseService.NotifyOfChange(ApplicationDataType.Transactions);
        }
    }

    private void OnCreateRuleCommandExecute()
    {
        if (this.tabTransactionsController.ViewModel.SelectedRow is null)
        {
            this.messageBox.Show("No row selected.");
            return;
        }

        RulesController.CreateNewRuleFromTransaction(this.tabTransactionsController.ViewModel.SelectedRow);
    }

    private void OnSavedNotificationReceived(object? sender, EventArgs eventArgs)
    {
        Dirty = false;
    }

    private void OnShowRulesCommandExecute()
    {
        RulesController.Shown = true;
    }
}
