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
using BudgetAnalyser.ReportsCatalog;
using BudgetAnalyser.ReportsCatalog.BurnDownGraphs;
using BudgetAnalyser.ReportsCatalog.LongTermSpendingLineGraph;
using BudgetAnalyser.ReportsCatalog.OverallPerformance;
using BudgetAnalyser.Statement;
using GalaSoft.MvvmLight.Messaging;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;

namespace BudgetAnalyser
{
    /// <summary>
    ///     Controllers required by the <see cref="ShellController" /> and most other <see cref="ControllerBase"/> controllers grouped together for convenience.
    ///     This follows an Ambient Context pattern. Not using Thread Local Storage for ease of testing.
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class UiContext : IUiContext
    {
        private List<ControllerBase> controllers;

        public UiContext(
            [NotNull] IBackgroundProcessingJobMetadata backgroundJobMetadata,
            [NotNull] Func<IWaitCursor> waitCursorFactory,
            [NotNull] UserPrompts userPrompts,
            [NotNull] IMessenger messenger)
        {
            if (waitCursorFactory == null)
            {
                throw new ArgumentNullException("waitCursorFactory");
            }

            if (userPrompts == null)
            {
                throw new ArgumentNullException("userPrompts");
            }

            if (messenger == null)
            {
                throw new ArgumentNullException("messenger");
            }

            if (backgroundJobMetadata == null)
            {
                throw new ArgumentNullException("backgroundJobMetadata");
            }

            WaitCursorFactory = waitCursorFactory;
            BackgroundJob = backgroundJobMetadata;
            UserPrompts = userPrompts;
            Messenger = messenger;
        }

        public ShowSurplusBalancesController ShowSurplusBalancesController { get; set; }
        public AddLedgerReconciliationController AddLedgerReconciliationController { get; set; }

        public OverallPerformanceController OverallPerformanceController { get; set; }
        public AppliedRulesController AppliedRulesController { get; set; }
        public IBackgroundProcessingJobMetadata BackgroundJob { get; private set; }

        public BudgetController BudgetController { get; set; }
        public BudgetPieController BudgetPieController { get; set; }
        public ChooseBudgetBucketController ChooseBudgetBucketController { get; set; }

        public IEnumerable<ControllerBase> Controllers
        {
            get { return this.controllers ?? (this.controllers = DiscoverAllControllers()); }
        }

        public CurrentMonthBurnDownGraphsController CurrentMonthBurnDownGraphsController { get; set; }

        public DashboardController DashboardController { get; set; }
        public EditingTransactionController EditingTransactionController { get; set; }

        public GlobalFilterController GlobalFilterController { get; set; }
        public LedgerBookController LedgerBookController { get; set; }
        public LedgerRemarksController LedgerRemarksController { get; set; }
        public LedgerTransactionsController LedgerTransactionsController { get; set; }
        public ILogger Logger { get; set; }
        public MainMenuController MainMenuController { get; set; }
        public IMessenger Messenger { get; private set; }
        public NewRuleController NewRuleController { get; set; }
        public ReportsCatalogController ReportsCatalogController { get; set; }
        public RulesController RulesController { get; set; }

        public IEnumerable<IShowableController> ShowableControllers
        {
            get { return Controllers.OfType<IShowableController>(); }
        }

        public SplitTransactionController SplitTransactionController { get; set; }

        public StatementController StatementController { get; set; }
        public StatementControllerNavigation StatementControllerNavigation { get; set; }
        public UserPrompts UserPrompts { get; private set; }
        public Func<IWaitCursor> WaitCursorFactory { get; private set; }
        public LongTermSpendingGraphController LongTermSpendingGraphController { get; set; }

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