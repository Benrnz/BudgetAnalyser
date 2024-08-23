using System.Windows.Input;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Statement;
using CommunityToolkit.Mvvm.Input;
using Rees.Wpf.Contracts;
using Rees.Wpf;

namespace BudgetAnalyser.Matching
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class AppliedRulesController : ControllerBase
    {
        private readonly IApplicationDatabaseFacade applicationDatabaseService;
        private readonly IUserMessageBox messageBox;
        private readonly ITransactionRuleService ruleService;
        private readonly StatementController statementController;
        private bool doNotUseDirty;

        public AppliedRulesController([NotNull] IUiContext uiContext, [NotNull] ITransactionRuleService ruleService, [NotNull] IApplicationDatabaseFacade applicationDatabaseService)
            : base(uiContext.Messenger)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException(nameof(uiContext));
            }

            RulesController = uiContext.RulesController;
            this.ruleService = ruleService ?? throw new ArgumentNullException(nameof(ruleService));
            this.applicationDatabaseService = applicationDatabaseService ?? throw new ArgumentNullException(nameof(applicationDatabaseService));
            this.statementController = uiContext.StatementController;
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
            return this.statementController.ViewModel.SelectedRow != null;
        }

        private void OnApplyRulesCommandExecute()
        {
            if (this.ruleService.Match(this.statementController.ViewModel.Statement.Transactions))
            {
                Dirty = true;
                this.applicationDatabaseService.NotifyOfChange(ApplicationDataType.Transactions);
            }
        }

        private void OnCreateRuleCommandExecute()
        {
            if (this.statementController.ViewModel.SelectedRow == null)
            {
                this.messageBox.Show("No row selected.");
                return;
            }

            RulesController.CreateNewRuleFromTransaction(this.statementController.ViewModel.SelectedRow);
        }

        private void OnSavedNotificationReceived(object sender, EventArgs eventArgs)
        {
            Dirty = false;
        }

        private void OnShowRulesCommandExecute()
        {
            RulesController.Shown = true;
        }
    }
}