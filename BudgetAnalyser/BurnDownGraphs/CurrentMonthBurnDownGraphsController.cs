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

namespace BudgetAnalyser.BurnDownGraphs
{
    public class CurrentMonthBurnDownGraphsController : ControllerBase
    {
        private readonly AddUserDefinedBurnDownController addUserDefinedBurnDownController;
        private readonly Func<BucketBurnDownController> bucketSpendingFactory;
        private readonly IBudgetBucketRepository budgetBucketRepository;
        private readonly IViewLoader viewLoader;
        private BudgetModel budget;
        private List<CustomAggregateBurnDownGraph> customCharts = new List<CustomAggregateBurnDownGraph>();
        private GlobalFilterCriteria doNotUseCriteria;
        private BucketBurnDownController doNotUseSelectedChart;
        private Engine.Ledger.LedgerBook ledgerBook;
        private StatementModel statement;

        public CurrentMonthBurnDownGraphsController(
            [NotNull] Func<BucketBurnDownController> bucketSpendingFactory,
            [NotNull] CurrentMonthBurnDownGraphsViewLoader viewLoader,
            [NotNull] AddUserDefinedBurnDownController addUserDefinedBurnDownController,
            [NotNull] IBudgetBucketRepository budgetBucketRepository,
            UiContext uiContext)
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

            this.bucketSpendingFactory = bucketSpendingFactory;
            this.viewLoader = viewLoader;
            this.addUserDefinedBurnDownController = addUserDefinedBurnDownController;
            this.budgetBucketRepository = budgetBucketRepository;

            MessengerInstance = uiContext.Messenger;
            MessengerInstance.Register<ApplicationStateRequestedMessage>(this, OnApplicationStateRequested);
            MessengerInstance.Register<ApplicationStateLoadedMessage>(this, OnApplicationStateLoaded);
        }

        public ICommand AddChartCommand
        {
            get { return new RelayCommand(OnAddChartCommandExecuted); }
        }

        public BindingList<BucketBurnDownController> ChartControllers { get; private set; }

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

        public void Close()
        {
            ChartControllers = null;
            this.viewLoader.Close();
        }

        public void Load(StatementModel statementModel, BudgetModel budgetModel, GlobalFilterCriteria criteria, Engine.Ledger.LedgerBook ledgerBookModel)
        {
            this.statement = statementModel;
            this.budget = budgetModel;
            this.ledgerBook = ledgerBookModel;
            var listOfCharts = new List<BucketBurnDownController>(this.budgetBucketRepository.Buckets.Count());

            foreach (BudgetBucket bucket in this.budgetBucketRepository.Buckets.Where(b => b is SpentMonthlyExpense))
            {
                BucketBurnDownController chartController = this.bucketSpendingFactory();
                chartController.Load(statementModel, budgetModel, bucket, criteria, ledgerBookModel);
                listOfCharts.Add(chartController);
            }

            listOfCharts = listOfCharts.OrderByDescending(x => x.NetWorth).ToList();

            // Put surplus at the top.
            listOfCharts.Insert(
                0,
                this.bucketSpendingFactory().Load(statementModel, budgetModel, this.budgetBucketRepository.SurplusBucket, criteria, ledgerBookModel));

            // Put any custom charts on top.
            foreach (CustomAggregateBurnDownGraph customChart in this.customCharts)
            {
                BucketBurnDownController chartController = this.bucketSpendingFactory();
                IEnumerable<BudgetBucket> buckets = this.budgetBucketRepository.Buckets
                    .Join(customChart.BucketIds, bucket => bucket.Code, code => code, (bucket, code) => bucket);
                chartController.LoadCustomChart(statementModel, budgetModel, buckets, criteria, ledgerBookModel, customChart.Name);
                listOfCharts.Insert(0, chartController);
            }

            ChartControllers = new BindingList<BucketBurnDownController>(listOfCharts);
            Criteria = criteria;
            this.viewLoader.Show(this);
            RaisePropertyChanged(() => ChartControllers);
        }

        private void OnAddChartCommandExecuted()
        {
            if (!this.addUserDefinedBurnDownController.AddChart())
            {
                return;
            }

            List<BudgetBucket> buckets = this.addUserDefinedBurnDownController.SelectedBuckets.ToList();
            BucketBurnDownController newChart = this.bucketSpendingFactory();
            newChart.LoadCustomChart(this.statement, this.budget, buckets, Criteria, this.ledgerBook, this.addUserDefinedBurnDownController.ChartTitle);
            ChartControllers.Insert(0, newChart);
            var persistChart = new CustomAggregateBurnDownGraph
            {
                BucketIds = buckets.Select(b => b.Code).ToList(),
                Name = this.addUserDefinedBurnDownController.ChartTitle,
            };

            this.customCharts.Add(persistChart);
        }

        private void OnApplicationStateLoaded(ApplicationStateLoadedMessage message)
        {
            if (message.RehydratedModels.ContainsKey(typeof(CustomBurnDownChartsV1)))
            {
                this.customCharts = message.RehydratedModels[typeof(CustomBurnDownChartsV1)].AdaptModel<List<CustomAggregateBurnDownGraph>>();
            }
        }

        private void OnApplicationStateRequested(ApplicationStateRequestedMessage message)
        {
            message.PersistThisModel(new CustomBurnDownChartsV1 { Model = this.customCharts });
        }

        private void OnRemoveChartCommandExecuted()
        {
            ChartControllers.Remove(SelectedChart);
            CustomAggregateBurnDownGraph chart = this.customCharts.FirstOrDefault(c => c.Name == SelectedChart.ChartTitle);
            this.customCharts.Remove(chart);
        }

        private bool RemoveChartCommandCanExecute()
        {
            return SelectedChart != null && SelectedChart.IsCustomChart;
        }
    }
}