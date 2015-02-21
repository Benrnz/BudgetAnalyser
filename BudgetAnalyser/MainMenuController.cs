using System;
using System.Windows.Input;
using System.Windows.Threading;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Dashboard;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.Statement;
using GalaSoft.MvvmLight.CommandWpf;
using Rees.Wpf;

namespace BudgetAnalyser
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class MainMenuController : ControllerBase, IInitializableController
    {
        private readonly UiContext uiContext;
        private readonly IApplicationDatabaseService applicationDatabaseService;
        private readonly IDashboardService dashboardService;
        private bool doNotUseBudgetToggle;
        private bool doNotUseDashboardToggle;
        private bool doNotUseLedgerBookToggle;
        private bool doNotUseReportsToggle;
        private bool doNotUseTransactionsToggle;

        public MainMenuController([NotNull] UiContext uiContext, [NotNull] IApplicationDatabaseService applicationDatabaseService, [NotNull] IDashboardService dashboardService)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            if (applicationDatabaseService == null)
            {
                throw new ArgumentNullException("applicationDatabaseService");
            }

            if (dashboardService == null)
            {
                throw new ArgumentNullException("dashboardService");
            }

            this.uiContext = uiContext;
            this.applicationDatabaseService = applicationDatabaseService;
            this.dashboardService = dashboardService;
            uiContext.Messenger.Register<WidgetActivatedMessage>(this, OnWidgetActivatedMessageReceived);
            MessengerInstance = uiContext.Messenger;
            MessengerInstance.Register<NavigateToTransactionMessage>(this, OnNavigateToTransactionRequestReceived);
        }

        public ICommand BudgetCommand
        {
            get { return new RelayCommand(OnBudgetExecuted); }
        }

        public bool BudgetToggle
        {
            get { return this.doNotUseBudgetToggle; }
            set
            {
                this.doNotUseBudgetToggle = value;
                RaisePropertyChanged(() => BudgetToggle);
            }
        }

        public ICommand DashboardCommand
        {
            get { return new RelayCommand(OnDashboardExecuted, CanExecuteDashboardCommand); }
        }

        public bool DashboardToggle
        {
            get { return this.doNotUseDashboardToggle; }
            set
            {
                this.doNotUseDashboardToggle = value;
                RaisePropertyChanged(() => DashboardToggle);
            }
        }

        public ICommand LedgerBookCommand
        {
            get { return new RelayCommand(OnLedgerBookExecuted); }
        }

        public bool LedgerBookToggle
        {
            get { return this.doNotUseLedgerBookToggle; }
            set
            {
                this.doNotUseLedgerBookToggle = value;
                RaisePropertyChanged(() => LedgerBookToggle);
            }
        }

        public ICommand ReportsCommand
        {
            get { return new RelayCommand(OnReportsExecuted); }
        }

        public bool ReportsToggle
        {
            get { return this.doNotUseReportsToggle; }
            set
            {
                this.doNotUseReportsToggle = value;
                RaisePropertyChanged(() => ReportsToggle);
            }
        }

        public ICommand TransactionsCommand
        {
            get { return new RelayCommand(OnTransactionExecuted); }
        }

        public bool TransactionsToggle
        {
            get { return this.doNotUseTransactionsToggle; }
            set
            {
                this.doNotUseTransactionsToggle = value;
                RaisePropertyChanged(() => TransactionsToggle);
            }
        }

        public void Initialize()
        {
            DashboardCommand.Execute(null);
        }

        private void AfterTabExecutedCommon()
        {
            foreach (IShowableController controller in this.uiContext.ShowableControllers)
            {
                controller.Shown = false;
            }

            this.uiContext.DashboardController.Shown = DashboardToggle;
            this.uiContext.StatementController.Shown = TransactionsToggle;
            this.uiContext.LedgerBookController.Shown = LedgerBookToggle;
            this.uiContext.BudgetController.Shown = BudgetToggle;
            this.uiContext.ReportsCatalogController.Shown = ReportsToggle;
        }

        private void BeforeTabExecutedCommon()
        {
            DashboardToggle = false;
            TransactionsToggle = false;
            LedgerBookToggle = false;
            BudgetToggle = false;
            ReportsToggle = false;
        }

        private bool CanExecuteDashboardCommand()
        {
            return true;
        }

        private void OnBudgetExecuted()
        {
            BeforeTabExecutedCommon();
            BudgetToggle = true;
            AfterTabExecutedCommon();
        }

        private void OnDashboardExecuted()
        {
            BeforeTabExecutedCommon();
            DashboardToggle = true;
            AfterTabExecutedCommon();
        }

        private void OnLedgerBookExecuted()
        {
            BeforeTabExecutedCommon();
            LedgerBookToggle = true;
            AfterTabExecutedCommon();
        }

        private void OnNavigateToTransactionRequestReceived(NavigateToTransactionMessage message)
        {
            message.WhenReadyToNavigate.ContinueWith(t =>
            {
                if (t.IsCompleted && !t.IsCanceled && !t.IsFaulted && message.Success)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, OnTransactionExecuted);
                }
            });
        }

        private void OnReportsExecuted()
        {
            BeforeTabExecutedCommon();
            ReportsToggle = true;
            AfterTabExecutedCommon();
        }

        private void OnTransactionExecuted()
        {
            BeforeTabExecutedCommon();
            TransactionsToggle = true;
            AfterTabExecutedCommon();
        }

        private void OnWidgetActivatedMessageReceived([NotNull] WidgetActivatedMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            if (message.Handled)
            {
                return;
            }

            if (message.Widget is DaysSinceLastImport)
            {
                OnTransactionExecuted();
            }

            ProcessCurrentFileWidgetActivated(message);
        }

        private void ProcessCurrentFileWidgetActivated(WidgetActivatedMessage message)
        {
            var widget = message.Widget as CurrentFilesWidget;
            if (widget == null)
            {
                return;
            }

            message.Handled = true;
            bool? response;
            if (this.applicationDatabaseService.HasUnsavedChanges)
            {
                response = this.uiContext.UserPrompts.YesNoBox.Show("Save changes before loading a different file?", "Open Budget Analyser File");
                if (response != null && response.Value) this.applicationDatabaseService.Save();
            }

            this.applicationDatabaseService.Close();
            var openDialog = this.uiContext.UserPrompts.OpenFileFactory();
            openDialog.CheckFileExists = true;
            openDialog.AddExtension = true;
            openDialog.DefaultExt = "*.bax";
            openDialog.Filter = "Budget Analyser files (*.bax)|*.bax|Xml files (*.xml, *.xaml)|*.xml;*.xaml";
            openDialog.Title = "Select Budget Analyser file to open";
            response = openDialog.ShowDialog();
            if (response == null || response.Value == false) return;

            throw new NotImplementedException();
        }
    }
}