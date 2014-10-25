using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using GalaSoft.MvvmLight.CommandWpf;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;

namespace BudgetAnalyser.ReportsCatalog.BurnDownGraphs
{
    public class AddUserDefinedBurnDownController : ControllerBase
    {
        private readonly IBudgetBucketRepository bucketRepository;
        private readonly IViewLoader viewLoader;

        public AddUserDefinedBurnDownController(
            [NotNull] AddUserDefinedBurnDownDialogViewLoader viewLoader,
            [NotNull] IBudgetBucketRepository bucketRepository)
        {
            if (viewLoader == null)
            {
                throw new ArgumentNullException("viewLoader");
            }

            if (bucketRepository == null)
            {
                throw new ArgumentNullException("bucketRepository");
            }

            this.viewLoader = viewLoader;
            this.bucketRepository = bucketRepository;
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
            this.SelectedBuckets = new BindingList<BudgetBucket>();
            this.UnselectedBuckets = new BindingList<BudgetBucket>(this.bucketRepository.Buckets.Where(b => b is ExpenseBucket || b is SurplusBucket).ToList());
            this.ChartTitle = string.Empty;

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
            this.UnselectedBuckets.Remove(parameter);
            this.SelectedBuckets.Add(parameter);
        }

        private void OnRemoveSelectedCommandExecute(BudgetBucket parameter)
        {
            this.SelectedBuckets.Remove(parameter);
            this.UnselectedBuckets.Add(parameter);
        }
    }
}