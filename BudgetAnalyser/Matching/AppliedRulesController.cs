using System.Windows.Input;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Transactions;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;
using Rees.Wpf.Contracts;

namespace BudgetAnalyser.Matching;

[AutoRegisterWithIoC(SingleInstance = true)]
public class AppliedRulesController : ControllerBase
{
    private readonly IApplicationDatabaseFacade applicationDatabaseService;
    private readonly IUserMessageBox messageBox;
    private readonly ITransactionRuleService ruleService;
    private readonly TopTransactionsListController transactionsController;

    public AppliedRulesController(IMessenger messenger, IUiContext uiContext, ITransactionRuleService ruleService, IApplicationDatabaseFacade applicationDatabaseService)
        : base(messenger)
    {
        if (uiContext is null)
        {
            throw new ArgumentNullException(nameof(uiContext));
        }

        TopRulesController = uiContext.Controller<TopRulesController>();
        this.ruleService = ruleService ?? throw new ArgumentNullException(nameof(ruleService));
        this.applicationDatabaseService = applicationDatabaseService ?? throw new ArgumentNullException(nameof(applicationDatabaseService));
        this.transactionsController = uiContext.Controller<TopTransactionsListController>();
        this.messageBox = uiContext.UserPrompts.MessageBox;
        this.ruleService.Saved += OnSavedNotificationReceived;
    }

    [UsedImplicitly]
    public ICommand ApplyRulesCommand => new RelayCommand(OnApplyRulesCommandExecute, CanExecuteApplyRulesCommand);

    [UsedImplicitly]
    public ICommand CreateRuleCommand => new RelayCommand(OnCreateRuleCommandExecute, CanExecuteCreateRuleCommand);

    public bool Dirty
    {
        get;

        set
        {
            field = value;
            if (Dirty)
            {
                this.applicationDatabaseService.NotifyOfChange(ApplicationDataType.MatchingRules);
            }

            OnPropertyChanged();
        }
    }

    [UsedImplicitly]
    public ICommand ShowRulesCommand => new RelayCommand(OnShowRulesCommandExecute);

    public TopRulesController TopRulesController { get; }

    private bool CanExecuteApplyRulesCommand()
    {
        return TopRulesController.RulesGroupedByBucket.Any();
    }

    private bool CanExecuteCreateRuleCommand()
    {
        return this.transactionsController.ViewModel.SelectedRow is not null;
    }

    private void OnApplyRulesCommandExecute()
    {
        var transactions = this.transactionsController.ViewModel.TransactionsList ?? throw new InvalidOperationException("The transactions model is null, not initialised or not loaded.");
        if (this.ruleService.Match(transactions.Transactions))
        {
            Dirty = true;
            this.applicationDatabaseService.NotifyOfChange(ApplicationDataType.Transactions);
        }
    }

    private void OnCreateRuleCommandExecute()
    {
        if (this.transactionsController.ViewModel.SelectedRow is null)
        {
            this.messageBox.Show("No row selected.");
            return;
        }

        TopRulesController.CreateNewRuleFromTransaction(this.transactionsController.ViewModel.SelectedRow);
    }

    private void OnSavedNotificationReceived(object? sender, EventArgs eventArgs)
    {
        Dirty = false;
    }

    private void OnShowRulesCommandExecute()
    {
        TopRulesController.Shown = true;
    }
}
