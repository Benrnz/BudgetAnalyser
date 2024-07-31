using System.ComponentModel;
using System.Windows.Input;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Services;
using CommunityToolkit.Mvvm.Input;
using Rees.Wpf;
using Rees.Wpf.Contracts;

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
                throw new ArgumentNullException(nameof(viewLoader));
            }

            if (chartsService == null)
            {
                throw new ArgumentNullException(nameof(chartsService));
            }

            this.viewLoader = viewLoader;
            this.chartsService = chartsService;
        }

        [UsedImplicitly]
        public ICommand AddChartCommand => new RelayCommand(OnAddChartCommandExecuted);

        [UsedImplicitly]
        public ICommand AddSelectedCommand => new RelayCommand<BudgetBucket>(OnAddSelectedCommandExecute, AddSelectedCommandCanExecute);

        public string ChartTitle { get; set; }

        [UsedImplicitly]
        public ICommand RemoveSelectedCommand => new RelayCommand<BudgetBucket>(OnRemoveSelectedCommandExecute, AddSelectedCommandCanExecute);

        public BindingList<BudgetBucket> SelectedBuckets { get; private set; }

        [UsedImplicitly]
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

        private static bool AddSelectedCommandCanExecute(BudgetBucket parameter)
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