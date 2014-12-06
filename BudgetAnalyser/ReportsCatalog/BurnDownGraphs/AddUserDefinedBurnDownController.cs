using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Services;
using GalaSoft.MvvmLight.CommandWpf;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;

namespace BudgetAnalyser.ReportsCatalog.BurnDownGraphs
{
    public class AddUserDefinedBurnDownController : ControllerBase
    {
        private readonly IBurnDownChartsService chartsService;
        private readonly IViewLoader viewLoader;

        public AddUserDefinedBurnDownController(
            [NotNull] AddUserDefinedBurnDownDialogViewLoader viewLoader,
            [NotNull] IBurnDownChartsService chartsService)
        {
            if (viewLoader == null)
            {
                throw new ArgumentNullException("viewLoader");
            }

            if (chartsService == null)
            {
                throw new ArgumentNullException("chartsService");
            }

            this.viewLoader = viewLoader;
            this.chartsService = chartsService;
        }

        public ICommand AddChartCommand
        {
            get { return new RelayCommand(OnAddChartCommandExecuted); }
        }

        public ICommand AddSelectedCommand
        {
            get { return new RelayCommand<BudgetBucket>(OnAddSelectedCommandExecute, AddSelectedCommandCanExecute); }
        }

        public string ChartTitle { get; set; }

        public ICommand RemoveSelectedCommand
        {
            get { return new RelayCommand<BudgetBucket>(OnRemoveSelectedCommandExecute, AddSelectedCommandCanExecute); }
        }

        public BindingList<BudgetBucket> SelectedBuckets { get; private set; }

        public BindingList<BudgetBucket> UnselectedBuckets { get; private set; }

        public bool AddChart()
        {
            SelectedBuckets = new BindingList<BudgetBucket>();
            UnselectedBuckets = new BindingList<BudgetBucket>(this.chartsService.AvailableBucketsForBurnDownCharts().ToList());
            ChartTitle = string.Empty;

            bool? result = this.viewLoader.ShowDialog(this);
            if (result != null && result.Value)
            {
                return true;
            }

            return false;
        }

        private bool AddSelectedCommandCanExecute(BudgetBucket parameter)
        {
            return parameter != null;
        }

        private void OnAddChartCommandExecuted()
        {
            this.viewLoader.Close();
        }

        private void OnAddSelectedCommandExecute(BudgetBucket parameter)
        {
            UnselectedBuckets.Remove(parameter);
            SelectedBuckets.Add(parameter);
        }

        private void OnRemoveSelectedCommandExecute(BudgetBucket parameter)
        {
            SelectedBuckets.Remove(parameter);
            UnselectedBuckets.Add(parameter);
        }
    }
}