using System.Windows.Input;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Transactions;
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

    public AppliedRulesController(
        IMessenger messenger,
        UserPrompts userPrompts,
        EditRulesController editRulesController,
        ITransactionRuleService ruleService,
        IApplicationDatabaseFacade applicationDatabaseService)
        : base(messenger)
    {
        EditRulesController = editRulesController ?? throw new ArgumentNullException(nameof(editRulesController));
        this.ruleService = ruleService ?? throw new ArgumentNullException(nameof(ruleService));
        this.applicationDatabaseService = applicationDatabaseService ?? throw new ArgumentNullException(nameof(applicationDatabaseService));
        this.messageBox = userPrompts.MessageBox ?? throw new ArgumentNullException(nameof(userPrompts.MessageBox));
        this.ruleService.Saved += OnSavedNotificationReceived;
        ApplyRulesCommand = new RelayCommand<TransactionsListModel?>(OnApplyRulesCommandExecute);
        CreateRuleCommand = new RelayCommand<Transaction?>(OnCreateRuleCommandExecute, CanExecuteCreateRuleCommand);
        ShowRulesCommand = new RelayCommand(OnShowRulesCommandExecute);
    }

    public IRelayCommand<TransactionsListModel?> ApplyRulesCommand { get; }

    public ICommand CreateRuleCommand { get; }

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

    public EditRulesController EditRulesController { get; }

    public ICommand ShowRulesCommand { get; }

    private bool CanExecuteCreateRuleCommand(Transaction? transaction)
    {
        return transaction is not null;
    }

    private void OnApplyRulesCommandExecute(TransactionsListModel? transactions = null)
    {
        var t = transactions ?? throw new ArgumentNullException(nameof(transactions));
        if (this.ruleService.Match(t.Transactions))
        {
            Dirty = true;
            this.applicationDatabaseService.NotifyOfChange(ApplicationDataType.Transactions);
        }
    }

    private void OnCreateRuleCommandExecute(Transaction? transaction)
    {
        if (transaction is null)
        {
            this.messageBox.Show("No row selected.");
            return;
        }

        EditRulesController.CreateNewRuleFromTransaction(transaction);
    }

    private void OnSavedNotificationReceived(object? sender, EventArgs eventArgs)
    {
        Dirty = false;
    }

    private void OnShowRulesCommandExecute()
    {
        EditRulesController.ShowDialog();
    }
}
