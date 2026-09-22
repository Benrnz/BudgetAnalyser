using System.Windows.Data;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Transactions;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.ReportsCatalog.OverallPerformance;

[AutoRegisterWithIoC(SingleInstance = true)]
public partial class OverallPerformanceController(IMessenger messenger, OverallPerformanceBudgetAnalyser chartService) : ControllerBase(messenger)
{
    private readonly OverallPerformanceBudgetAnalyser chartService = chartService ?? throw new ArgumentNullException(nameof(chartService));

    public OverallPerformanceBudgetResult? Analysis { get; private set; }

    [ObservableProperty]
    public partial string DurationLabel { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial bool ExpenseFilter { get; set; } = true;

    [ObservableProperty]
    public partial bool IncomeFilter { get; set; }

    public double OverallPerformance { get; private set; }

    [ObservableProperty]
    public partial bool ShowValidationMessage { get; private set; }

    public string Title => "Overall Budget Performance";

    [ObservableProperty]
    public partial string ValidationMessage { get; private set; } = string.Empty;

    public void Load(TransactionsListModel transactions, BudgetCollection budgets, DateOnly startDateIncl, DateOnly endDateIncl)
    {
        Analysis = this.chartService.Analyse(transactions, budgets, startDateIncl, endDateIncl);
        OverallPerformance = (double)Analysis.OverallPerformance;
        ExpenseFilter = true;
        IncomeFilter = false;
        ShowValidationMessage = Analysis.HasValidationMessage;
        ValidationMessage = Analysis.ValidationMessage;
        DurationLabel = $"Duration In {Analysis.BudgetCycle}:";

        OnPropertyChanged(nameof(Analysis));
        var view = CollectionViewSource.GetDefaultView(Analysis.Analyses);
        view.Filter = x =>
        {
            if (x is not BucketPerformanceResult bucketAnalysis)
            {
                return true;
            }

            if (IncomeFilter)
            {
                return bucketAnalysis.Bucket is IncomeBudgetBucket;
            }

            var result = !(bucketAnalysis.Bucket is IncomeBudgetBucket);
            return result;
        };
    }

    partial void OnExpenseFilterChanged(bool value)
    {
        RefreshCollection();
    }

    partial void OnIncomeFilterChanged(bool value)
    {
        RefreshCollection();
    }

    private void RefreshCollection()
    {
        if (Analysis?.Analyses is null || Analysis.Analyses.None())
        {
            return;
        }

        CollectionViewSource.GetDefaultView(Analysis.Analyses).Refresh();
    }
}
