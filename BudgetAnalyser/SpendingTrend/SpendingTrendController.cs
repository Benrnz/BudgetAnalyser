using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using BudgetAnalyser.Annotations;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Statement;
using GalaSoft.MvvmLight.Command;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;
using Rees.Wpf.ApplicationState;

namespace BudgetAnalyser.SpendingTrend
{
    public class SpendingTrendController : ControllerBase
    {
        private readonly AddUserDefinedSpendingChartController addUserDefinedSpendingChartController;
        private readonly Func<BucketSpendingController> bucketSpendingFactory;
        private readonly IBudgetBucketRepository budgetBucketRepository;
        private readonly IViewLoader viewLoader;
        private BudgetModel budget;
        private List<CustomAggregateSpendingGraph> customCharts = new List<CustomAggregateSpendingGraph>();
        private GlobalFilterCriteria doNotUseCriteria;
        private BucketSpendingController doNotUseSelectedChart;
        private StatementModel statement;

        public SpendingTrendController(
            [NotNull] Func<BucketSpendingController> bucketSpendingFactory,
            [NotNull] SpendingTrendViewLoader viewLoader,
            [NotNull] AddUserDefinedSpendingChartController addUserDefinedSpendingChartController,
            [NotNull] IBudgetBucketRepository budgetBucketRepository)
        {
            if (bucketSpendingFactory == null)
            {
                throw new ArgumentNullException("bucketSpendingFactory");
            }

            if (viewLoader == null)
            {
                throw new ArgumentNullException("viewLoader");
            }

            if (addUserDefinedSpendingChartController == null)
            {
                throw new ArgumentNullException("addUserDefinedSpendingChartController");
            }

            if (budgetBucketRepository == null)
            {
                throw new ArgumentNullException("budgetBucketRepository");
            }

            this.bucketSpendingFactory = bucketSpendingFactory;
            this.viewLoader = viewLoader;
            this.addUserDefinedSpendingChartController = addUserDefinedSpendingChartController;
            this.budgetBucketRepository = budgetBucketRepository;
            MessagingGate.Register<ApplicationStateRequestedMessage>(this, OnApplicationStateRequested);
            MessagingGate.Register<ApplicationStateLoadedMessage>(this, OnApplicationStateLoaded);
        }

        public ICommand AddChartCommand
        {
            get { return new RelayCommand(OnAddChartCommandExecuted); }
        }

        public BindingList<BucketSpendingController> ChartControllers { get; private set; }

        public GlobalFilterCriteria Criteria
        {
            get { return this.doNotUseCriteria; }
            set
            {
                this.doNotUseCriteria = value;
                RaisePropertyChanged(() => Criteria);
            }
        }

        public ICommand RemoveChartCommand
        {
            get { return new RelayCommand(OnRemoveChartCommandExecuted, RemoveChartCommandCanExecute); }
        }

        public BucketSpendingController SelectedChart
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

        public void Close()
        {
            ChartControllers = null;
            this.viewLoader.Close();
        }

        public void Load(StatementModel statementModel, BudgetModel budgetModel, GlobalFilterCriteria criteria)
        {
            this.statement = statementModel;
            this.budget = budgetModel;
            var listOfCharts = new List<BucketSpendingController>(this.budgetBucketRepository.Buckets.Count());

            foreach (BudgetBucket bucket in this.budgetBucketRepository.Buckets.Where(b => b is SpentMonthlyExpense))
            {
                BucketSpendingController chartController = this.bucketSpendingFactory();
                chartController.Load(statementModel, budgetModel, bucket, criteria);
                listOfCharts.Add(chartController);
            }

            listOfCharts = listOfCharts.OrderByDescending(x => x.NetWorth).ToList();

            // Put surplus at the top.
            listOfCharts.Insert(
                0,
                this.bucketSpendingFactory().Load(statementModel, budgetModel, this.budgetBucketRepository.SurplusBucket, criteria));

            // Put any custom charts on top.
            foreach (CustomAggregateSpendingGraph customChart in this.customCharts)
            {
                BucketSpendingController chartController = this.bucketSpendingFactory();
                IEnumerable<BudgetBucket> buckets = this.budgetBucketRepository.Buckets
                    .Join(customChart.BucketIds, bucket => bucket.Id, id => id, (bucket, id) => bucket);
                chartController.LoadCustomChart(statementModel, budgetModel, buckets, criteria, customChart.Name);
                listOfCharts.Insert(0, chartController);
            }

            ChartControllers = new BindingList<BucketSpendingController>(listOfCharts);
            Criteria = criteria;
            this.viewLoader.Show(this);
            RaisePropertyChanged(() => ChartControllers);
        }

        private void OnAddChartCommandExecuted()
        {
            if (!this.addUserDefinedSpendingChartController.AddChart())
            {
                return;
            }

            List<BudgetBucket> buckets = this.addUserDefinedSpendingChartController.SelectedBuckets.ToList();
            BucketSpendingController newChart = this.bucketSpendingFactory();
            newChart.LoadCustomChart(this.statement, this.budget, buckets, Criteria, this.addUserDefinedSpendingChartController.ChartTitle);
            ChartControllers.Insert(0, newChart);
            var persistChart = new CustomAggregateSpendingGraph
            {
                BucketIds = buckets.Select(b => b.Id).ToList(),
                Name = this.addUserDefinedSpendingChartController.ChartTitle,
            };

            this.customCharts.Add(persistChart);
        }

        private void OnApplicationStateLoaded(ApplicationStateLoadedMessage message)
        {
            if (message.RehydratedModels.ContainsKey(typeof(CustomSpendingChartsV1)))
            {
                this.customCharts = message.RehydratedModels[typeof(CustomSpendingChartsV1)].AdaptModel<List<CustomAggregateSpendingGraph>>();
            }
        }

        private void OnApplicationStateRequested(ApplicationStateRequestedMessage message)
        {
            message.PersistThisModel(new CustomSpendingChartsV1 { Model = this.customCharts });
        }

        private void OnRemoveChartCommandExecuted()
        {
            ChartControllers.Remove(SelectedChart);
            CustomAggregateSpendingGraph chart = this.customCharts.FirstOrDefault(c => c.Name == SelectedChart.ChartTitle);
            this.customCharts.Remove(chart);
        }

        private bool RemoveChartCommandCanExecute()
        {
            return SelectedChart != null && SelectedChart.IsCustomChart;
        }
    }
}