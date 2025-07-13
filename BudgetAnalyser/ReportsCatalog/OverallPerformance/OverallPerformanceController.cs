using System.Windows.Data;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.ReportsCatalog.OverallPerformance;

[AutoRegisterWithIoC(SingleInstance = true)]
public class OverallPerformanceController(IMessenger messenger, IOverallPerformanceChartService chartService) : ControllerBase(messenger)
{
    private readonly IOverallPerformanceChartService chartService = chartService ?? throw new ArgumentNullException(nameof(chartService));
    private string doNotUseDurationLabel = string.Empty;
    private bool doNotUseExpenseFilter = true;
    private bool doNotUseIncomeFilter;
    private bool doNotUseShowValidationMessage;
    private string doNotUseValidationMessage = string.Empty;

    public OverallPerformanceBudgetResult? Analysis { get; private set; }

    public string DurationLabel
    {
        get => this.doNotUseDurationLabel;
        private set
        {
            if (value != this.doNotUseDurationLabel)
            {
                this.doNotUseDurationLabel = value;
                OnPropertyChanged();
            }
        }
    }

    public bool ExpenseFilter
    {
        get => this.doNotUseExpenseFilter;

        set
        {
            this.doNotUseExpenseFilter = value;
            OnPropertyChanged();
            RefreshCollection();
        }
    }

    public bool IncomeFilter
    {
        get => this.doNotUseIncomeFilter;

        set
        {
            this.doNotUseIncomeFilter = value;
            OnPropertyChanged();
            RefreshCollection();
        }
    }

    public double OverallPerformance { get; private set; }

    public bool ShowValidationMessage
    {
        get => this.doNotUseShowValidationMessage;
        private set
        {
            if (value != this.doNotUseShowValidationMessage)
            {
                this.doNotUseShowValidationMessage = value;
                OnPropertyChanged();
            }
        }
    }

    public string Title => "Overall Budget Performance";

    public string ValidationMessage
    {
        get => this.doNotUseValidationMessage;

        private set
        {
            if (value != this.doNotUseValidationMessage)
            {
                this.doNotUseValidationMessage = value;
                OnPropertyChanged();
            }
        }
    }

    public void Load(TransactionSetModel transactionSetModel, BudgetCollection budgets, GlobalFilterCriteria criteria)
    {
        Analysis = this.chartService.BuildChart(transactionSetModel, budgets, criteria);
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
