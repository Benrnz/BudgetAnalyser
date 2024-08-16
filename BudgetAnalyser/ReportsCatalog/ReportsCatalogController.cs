using System.Windows.Input;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Filtering;
using BudgetAnalyser.LedgerBook;
using BudgetAnalyser.ReportsCatalog.OverallPerformance;
using BudgetAnalyser.Statement;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.ReportsCatalog
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class ReportsCatalogController : ControllerBase, IShowableController
    {
        private readonly NewWindowViewLoader newWindowViewLoader;
        private BudgetCollection budgets;
        private Engine.Ledger.LedgerBook currentLedgerBook;
        private StatementModel currentStatementModel;
        private bool doNotUseShown;

        public ReportsCatalogController([NotNull] UiContext uiContext, [NotNull] NewWindowViewLoader newWindowViewLoader) : base(uiContext.Messenger)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException(nameof(uiContext));
            }

            if (newWindowViewLoader == null)
            {
                throw new ArgumentNullException(nameof(newWindowViewLoader));
            }

            this.newWindowViewLoader = newWindowViewLoader;
            OverallPerformanceController = uiContext.OverallPerformanceController;

            Messenger.Register<ReportsCatalogController, StatementReadyMessage>(this, static (r, m) => r.OnStatementReadyMessageReceived(m));
            Messenger.Register<ReportsCatalogController, BudgetReadyMessage>(this, static (r, m) => r.OnBudgetReadyMessageReceived(m));
            Messenger.Register<ReportsCatalogController, LedgerBookReadyMessage>(this, static (r, m) => r.OnLedgerBookReadyMessageReceived(m));
        }

        [UsedImplicitly]
        public ICommand OverallBudgetPerformanceCommand => new RelayCommand(OnOverallBudgetPerformanceCommandExecute, CanExecuteOverallBudgetPerformanceCommand);

        public OverallPerformanceController OverallPerformanceController { get; }

        public bool Shown
        {
            get { return this.doNotUseShown; }
            set
            {
                if (value == this.doNotUseShown)
                {
                    return;
                }
                this.doNotUseShown = value;
                OnPropertyChanged();
            }
        }

        private bool CanExecuteOverallBudgetPerformanceCommand()
        {
            return this.currentStatementModel != null
                   && this.currentStatementModel.Transactions.Any()
                   && this.budgets?.CurrentActiveBudget != null;
        }

        private void OnBudgetReadyMessageReceived(BudgetReadyMessage message)
        {
            this.budgets = message.Budgets;
        }

        private void OnLedgerBookReadyMessageReceived([NotNull] LedgerBookReadyMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            this.currentLedgerBook = message.LedgerBook;
        }

        private void OnOverallBudgetPerformanceCommandExecute()
        {
            OverallPerformanceController.Load(this.currentStatementModel, this.budgets, RequestCurrentFilter());

            this.newWindowViewLoader.MinHeight = this.newWindowViewLoader.Height = 650;
            this.newWindowViewLoader.MinWidth = this.newWindowViewLoader.Width = 740;
            this.newWindowViewLoader.Show(OverallPerformanceController);
        }

        private void OnStatementReadyMessageReceived(StatementReadyMessage message)
        {
            this.currentStatementModel = message.StatementModel;
        }

        private GlobalFilterCriteria RequestCurrentFilter()
        {
            var currentFilterMessage = new RequestFilterMessage(this);
            Messenger.Send(currentFilterMessage);
            return currentFilterMessage.Criteria;
        }
    }
}