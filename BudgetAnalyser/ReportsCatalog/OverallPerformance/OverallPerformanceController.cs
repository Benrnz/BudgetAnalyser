using System.Windows.Data;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Transactions;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.ReportsCatalog.OverallPerformance;

[AutoRegisterWithIoC(SingleInstance = true)]
public class OverallPerformanceController(IMessenger messenger, IOverallPerformanceChartService chartService) : ControllerBase(messenger)
{
    private readonly IOverallPerformanceChartService chartService = chartService ?? throw new ArgumentNullException(nameof(chartService));

    public OverallPerformanceBudgetResult? Analysis { get; private set; }

    public string DurationLabel
    {
        get;
        private set
        {
            if (value != field)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    } = string.Empty;

    public bool ExpenseFilter
    {
        get;

        set
        {
            field = value;
            OnPropertyChanged();
            RefreshCollection();
        }
    } = true;

    public bool IncomeFilter
    {
        get;

        set
        {
            field = value;
            OnPropertyChanged();
            RefreshCollection();
        }
    }

    public double OverallPerformance { get; private set; }

    public bool ShowValidationMessage
    {
        get;
        private set
        {
            if (value != field)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    }

    public string Title => "Overall Budget Performance";

    public string ValidationMessage
    {
        get;

        private set
        {
            if (value != field)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    } = string.Empty;

    public void Load(TransactionsListModel transactions, BudgetCollection budgets, GlobalFilterCriteria criteria)
    {
        Analysis = this.chartService.BuildChart(transactions, budgets, criteria);
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

    private void RefreshCollection()
    {
        if (Analysis?.Analyses is null || Analysis.Analyses.None())
        {
            return;
        }

        CollectionViewSource.GetDefaultView(Analysis.Analyses).Refresh();
    }
}
