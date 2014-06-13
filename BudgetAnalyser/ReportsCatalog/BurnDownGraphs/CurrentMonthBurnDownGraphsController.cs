using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using BudgetAnalyser.Annotations;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Statement;
using GalaSoft.MvvmLight.Command;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;
using Rees.Wpf.ApplicationState;

namespace BudgetAnalyser.ReportsCatalog.BurnDownGraphs
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class CurrentMonthBurnDownGraphsController : ControllerBase
    {
        private readonly AddUserDefinedBurnDownController addUserDefinedBurnDownController;
        private readonly Func<BucketBurnDownController> bucketSpendingFactory;
        private readonly BurnDownChartsBuilder chartBuilder;
        private readonly IViewLoader viewLoader;
        private BudgetModel budget;
        private string doNotUseDateRangeDescription;
        private BucketBurnDownController doNotUseSelectedChart;
        private Engine.Ledger.LedgerBook ledgerBook;
        private StatementModel statement;
        private DateTime beginDate;

        public CurrentMonthBurnDownGraphsController(
            [NotNull] Func<BucketBurnDownController> bucketSpendingFactory,
            [NotNull] CurrentMonthBurnDownGraphsViewLoader viewLoader,
            [NotNull] AddUserDefinedBurnDownController addUserDefinedBurnDownController,
            [NotNull] IBudgetBucketRepository budgetBucketRepository,
            [NotNull] UiContext uiContext)
        {
            if (bucketSpendingFactory == null)
            {
                throw new ArgumentNullException("bucketSpendingFactory");
            }

            if (viewLoader == null)
            {
                throw new ArgumentNullException("viewLoader");
            }

            if (addUserDefinedBurnDownController == null)
            {
                throw new ArgumentNullException("addUserDefinedBurnDownController");
            }

            if (budgetBucketRepository == null)
            {
                throw new ArgumentNullException("budgetBucketRepository");
            }

            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            this.bucketSpendingFactory = bucketSpendingFactory;
            this.viewLoader = viewLoader;
            this.addUserDefinedBurnDownController = addUserDefinedBurnDownController;
            this.chartBuilder = new BurnDownChartsBuilder(budgetBucketRepository, this.bucketSpendingFactory);

            this.MessengerInstance = uiContext.Messenger;
            this.MessengerInstance.Register<ApplicationStateRequestedMessage>(this, OnApplicationStateRequested);
            this.MessengerInstance.Register<ApplicationStateLoadedMessage>(this, OnApplicationStateLoaded);
        }

        public ICommand AddChartCommand
        {
            get { return new RelayCommand(OnAddChartCommandExecuted); }
        }

        public BindingList<BucketBurnDownController> ChartControllers { get; private set; }

        public string DateRangeDescription
        {
            get { return this.doNotUseDateRangeDescription; }
            private set
            {
                this.doNotUseDateRangeDescription = value;
                RaisePropertyChanged(() => this.DateRangeDescription);
            }
        }

        public ICommand RemoveChartCommand
        {
            get { return new RelayCommand(OnRemoveChartCommandExecuted, RemoveChartCommandCanExecute); }
        }

        public BucketBurnDownController SelectedChart
        {
            get { return this.doNotUseSelectedChart; }

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

        public void Load(
            [NotNull] StatementModel statementModel,
            [NotNull] BudgetModel budgetModel,
            [NotNull] GlobalFilterCriteria criteria,
            Engine.Ledger.LedgerBook ledgerBookModel)
        {
            if (statementModel == null)
            {
                throw new ArgumentNullException("statementModel");
            }

            if (budgetModel == null)
            {
                throw new ArgumentNullException("budgetModel");
            }

            if (criteria == null)
            {
                throw new ArgumentNullException("criteria");
            }

            this.statement = statementModel;
            this.budget = budgetModel;
            this.ledgerBook = ledgerBookModel;

            this.chartBuilder.Build(criteria, statementModel, budgetModel, ledgerBookModel);
            this.beginDate = this.chartBuilder.Results.BeginDate;
            this.DateRangeDescription = this.chartBuilder.Results.DateRangeDescription;
            this.ChartControllers = new BindingList<BucketBurnDownController>(this.chartBuilder.Results.Charts.ToList());

            this.viewLoader.Show(this);
            RaisePropertyChanged(() => this.ChartControllers);
        }

        private void OnAddChartCommandExecuted()
        {
            if (!this.addUserDefinedBurnDownController.AddChart())
            {
                return;
            }

            List<BudgetBucket> buckets = this.addUserDefinedBurnDownController.SelectedBuckets.ToList();
            BucketBurnDownController newChart = this.bucketSpendingFactory();
            newChart.LoadCustomChart(this.statement, this.budget, buckets, this.beginDate, this.ledgerBook, this.addUserDefinedBurnDownController.ChartTitle);
            this.ChartControllers.Insert(0, newChart);
            var persistChart = new CustomAggregateBurnDownGraph
            {
                BucketIds = buckets.Select(b => b.Code).ToList(),
                Name = this.addUserDefinedBurnDownController.ChartTitle,
            };

            this.chartBuilder.CustomCharts = this.chartBuilder.CustomCharts.Union(new[] {persistChart}).ToList();
        }

        private void OnApplicationStateLoaded(ApplicationStateLoadedMessage message)
        {
            if (message.RehydratedModels.ContainsKey(typeof(CustomBurnDownChartsV1)))
            {
                this.chartBuilder.CustomCharts = message.RehydratedModels[typeof(CustomBurnDownChartsV1)].AdaptModel<List<CustomAggregateBurnDownGraph>>();
            }
        }

        private void OnApplicationStateRequested(ApplicationStateRequestedMessage message)
        {
            message.PersistThisModel(new CustomBurnDownChartsV1 { Model = this.chartBuilder.CustomCharts });
        }

        private void OnRemoveChartCommandExecuted()
        {
            this.ChartControllers.Remove(this.SelectedChart);
            var customCharts = this.chartBuilder.CustomCharts.ToList();
            CustomAggregateBurnDownGraph chart = customCharts.FirstOrDefault(c => c.Name == this.SelectedChart.ChartTitle);
            customCharts.Remove(chart);
            this.chartBuilder.CustomCharts = customCharts;
        }

        private bool RemoveChartCommandCanExecute()
        {
            return this.SelectedChart != null && this.SelectedChart.IsCustomChart;
        }
    }
}