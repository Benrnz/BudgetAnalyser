using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BudgetAnalyser.Annotations;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Dashboard;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Filtering;
using BudgetAnalyser.LedgerBook;
using BudgetAnalyser.Matching;
using BudgetAnalyser.Mobile;
using BudgetAnalyser.ReportsCatalog;
using BudgetAnalyser.ReportsCatalog.BurnDownGraphs;
using BudgetAnalyser.ReportsCatalog.LongTermSpendingLineGraph;
using BudgetAnalyser.ReportsCatalog.OverallPerformance;
using BudgetAnalyser.Statement;
using GalaSoft.MvvmLight.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser
{
    /// <summary>
    ///     Controllers required by the <see cref="ShellController" /> and most other <see cref="ControllerBase" /> controllers
    ///     grouped together for convenience.
    ///     This follows an Ambient Context pattern. Not using Thread Local Storage for ease of testing.
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class UiContext : IUiContext
    {
        private List<ControllerBase> controllers;

        public UiContext(
            [NotNull] UserPrompts userPrompts,
            [NotNull] IMessenger messenger)
        {
            if (userPrompts == null)
            {
                throw new ArgumentNullException(nameof(userPrompts));
            }

            if (messenger == null)
            {
                throw new ArgumentNullException(nameof(messenger));
            }

            UserPrompts = userPrompts;
            Messenger = messenger;
        }

        public AddLedgerReconciliationController AddLedgerReconciliationController { get; set; }
        public AppliedRulesController AppliedRulesController { get; set; }
        public BudgetController BudgetController { get; set; }
        public BudgetPieController BudgetPieController { get; set; }
        public ChooseBudgetBucketController ChooseBudgetBucketController { get; set; }
        public IEnumerable<ControllerBase> Controllers => this.controllers ?? (this.controllers = DiscoverAllControllers());
        public CreateNewFixedBudgetController CreateNewFixedBudgetController { get; set; }
        public CreateNewSurprisePaymentMonitorController CreateNewSurprisePaymentMonitorController { get; set; }
        public CurrentMonthBurnDownGraphsController CurrentMonthBurnDownGraphsController { get; set; }
        public DashboardController DashboardController { get; set; }
        public DisusedRulesController DisusedRulesController { get; set; }
        public EditingTransactionController EditingTransactionController { get; set; }

        public EncryptFileController EncryptFileController { get; set; }
        public GlobalFilterController GlobalFilterController { get; set; }
        public LedgerBookController LedgerBookController { get; set; }
        public LedgerBucketViewController LedgerBucketViewController { get; set; }
        public LedgerRemarksController LedgerRemarksController { get; set; }
        public LedgerTransactionsController LedgerTransactionsController { get; set; }
        public ILogger Logger { get; set; }
        public LongTermSpendingGraphController LongTermSpendingGraphController { get; set; }
        public MainMenuController MainMenuController { get; set; }
        public IMessenger Messenger { get; }
        public NewBudgetModelController NewBudgetModelController { get; set; }
        public NewRuleController NewRuleController { get; set; }
        public OverallPerformanceController OverallPerformanceController { get; set; }
        public ReconciliationToDoListController ReconciliationToDoListController { get; set; }
        public ReportsCatalogController ReportsCatalogController { get; set; }
        public RulesController RulesController { get; set; }
        public IEnumerable<IShowableController> ShowableControllers => Controllers.OfType<IShowableController>();
        public ShowSurplusBalancesController ShowSurplusBalancesController { get; set; }
        public SplitTransactionController SplitTransactionController { get; set; }
        public StatementController StatementController { get; set; }
        public StatementControllerNavigation StatementControllerNavigation { get; set; }
        public TransferFundsController TransferFundsController { get; set; }

        public UploadMobileDataController UploadMobileDataController { get; set; }
        public UserPrompts UserPrompts { get; }

        private List<ControllerBase> DiscoverAllControllers()
        {
            return GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => typeof(ControllerBase).IsAssignableFrom(p.PropertyType))
                .Select(p => p.GetValue(this))
                .Cast<ControllerBase>()
                .ToList();
        }
    }
}