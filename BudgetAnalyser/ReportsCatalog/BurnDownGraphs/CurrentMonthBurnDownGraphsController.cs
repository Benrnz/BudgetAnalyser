using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Input;
using BudgetAnalyser.Annotations;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using GalaSoft.MvvmLight.CommandWpf;
using Rees.Wpf;
using Rees.Wpf.ApplicationState;
using ApplicationStateLoadedMessage = BudgetAnalyser.ApplicationState.ApplicationStateLoadedMessage;
using ApplicationStateRequestedMessage = BudgetAnalyser.ApplicationState.ApplicationStateRequestedMessage;

namespace BudgetAnalyser.ReportsCatalog.BurnDownGraphs
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class CurrentMonthBurnDownGraphsController : ControllerBase
    {
        private readonly AddUserDefinedBurnDownController addUserDefinedBurnDownController;
        private readonly IBurnDownChartsService chartsService;
        private DateTime inclBeginDate;
        private DateTime inclEndDate;
        private BudgetModel budget;
        private string doNotUseDateRangeDescription;
        private BucketBurnDownController doNotUseSelectedChart;
        private Engine.Ledger.LedgerBook ledgerBook;
        private StatementModel statement;

        public CurrentMonthBurnDownGraphsController(
            [NotNull] AddUserDefinedBurnDownController addUserDefinedBurnDownController,
            [NotNull] UiContext uiContext,
            [NotNull] IBurnDownChartsService chartsService)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException(nameof(uiContext));
            }

            this.addUserDefinedBurnDownController = addUserDefinedBurnDownController ?? throw new ArgumentNullException(nameof(addUserDefinedBurnDownController));
            this.chartsService = chartsService ?? throw new ArgumentNullException(nameof(chartsService));

            MessengerInstance = uiContext.Messenger;
            MessengerInstance.Register<ApplicationStateRequestedMessage>(this, OnApplicationStateRequested);
            MessengerInstance.Register<ApplicationStateLoadedMessage>(this, OnApplicationStateLoaded);
        }

        [UsedImplicitly]
        public ICommand AddChartCommand => new RelayCommand(OnAddChartCommandExecuted);

        public BindingList<BucketBurnDownController> ChartControllers { get; private set; }

        public string DateRangeDescription
        {
            [UsedImplicitly] get { return this.doNotUseDateRangeDescription; }
            private set
            {
                this.doNotUseDateRangeDescription = value;
                RaisePropertyChanged();
            }
        }

        [UsedImplicitly]
        public ICommand RemoveChartCommand => new RelayCommand(OnRemoveChartCommandExecuted, RemoveChartCommandCanExecute);

        public BucketBurnDownController SelectedChart
        {
            get { return this.doNotUseSelectedChart; }

            [UsedImplicitly]
            set
            {
                if (value == null)
                {
                    // Remember last selected chart to work around silliness in listbox control deselecting an item when context menu opens.
                    return;
                }

                this.doNotUseSelectedChart = value;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for data binding")]
        [UsedImplicitly]
        public string Title => "Current Month Burndown Graphs";

        public void Load(
            [NotNull] StatementModel statementModel,
            [NotNull] BudgetModel budgetModel,
            [NotNull] GlobalFilterCriteria criteria,
            Engine.Ledger.LedgerBook ledgerBookModel)
        {
            if (statementModel == null)
            {
                throw new ArgumentNullException(nameof(statementModel));
            }

            if (budgetModel == null)
            {
                throw new ArgumentNullException(nameof(budgetModel));
            }

            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            this.statement = statementModel;
            this.budget = budgetModel;
            this.ledgerBook = ledgerBookModel;

            BurnDownCharts results = this.chartsService.BuildAllCharts(statementModel, this.budget, this.ledgerBook, criteria);
            this.inclBeginDate = results.BeginDate;
            this.inclEndDate = results.EndDate;
            DateRangeDescription = results.DateRangeDescription;
            ChartControllers = new BindingList<BucketBurnDownController>(results.Charts.Select(BuildBucketBurnDownController).ToList());

            RaisePropertyChanged(() => ChartControllers);
        }

        protected virtual BucketBurnDownController BuildBucketBurnDownController(BurnDownChartAnalyserResult analysis)
        {
            var controller = new BucketBurnDownController();
            controller.Load(analysis);
            return controller;
        }

        private void OnAddChartCommandExecuted()
        {
            if (!this.addUserDefinedBurnDownController.AddChart())
            {
                return;
            }

            List<BudgetBucket> buckets = this.addUserDefinedBurnDownController.SelectedBuckets.ToList();
            BurnDownChartAnalyserResult result = this.chartsService.CreateNewCustomAggregateChart(
                this.statement,
                this.budget,
                buckets,
                this.ledgerBook,
                this.inclBeginDate,
                this.inclEndDate,
                this.addUserDefinedBurnDownController.ChartTitle);
            BucketBurnDownController newChart = BuildBucketBurnDownController(result);
            ChartControllers.Insert(0, newChart);
        }

        private void OnApplicationStateLoaded(ApplicationStateLoadedMessage message)
        {
            var customChartState = message.ElementOfType<CustomBurnDownChartApplicationState>();
            if (customChartState != null)
            {
                this.chartsService.LoadPersistedStateData(customChartState);
            }
        }

        private void OnApplicationStateRequested(ApplicationStateRequestedMessage message)
        {
            message.PersistThisModel(this.chartsService.PreparePersistentStateData());
        }

        private void OnRemoveChartCommandExecuted()
        {
            ChartControllers.Remove(SelectedChart);
            this.chartsService.RemoveCustomChart(SelectedChart.ChartTitle);
        }

        private bool RemoveChartCommandCanExecute()
        {
            return SelectedChart != null && SelectedChart.IsCustomChart;
        }
    }
}