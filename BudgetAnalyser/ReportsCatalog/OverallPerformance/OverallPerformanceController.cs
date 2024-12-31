using System.Windows.Data;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.ReportsCatalog.OverallPerformance
{
    public class OverallPerformanceController(IMessenger messenger, IOverallPerformanceChartService chartService) : ControllerBase(messenger)
    {
        private readonly IOverallPerformanceChartService chartService = chartService ?? throw new ArgumentNullException(nameof(chartService));
        private bool doNotUseShowValidationMessage;
        private bool doNotUseExpenseFilter = true;
        private bool doNotUseIncomeFilter;
        private string doNotUseValidationMessage = string.Empty;

        public OverallPerformanceBudgetResult? Analysis { get; private set; }

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

        public string Title => "Overall Budget Performance";

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

        public void Load(StatementModel statementModel, BudgetCollection budgets, GlobalFilterCriteria criteria)
        {
            Analysis = this.chartService.BuildChart(statementModel, budgets, criteria);
            OverallPerformance = (double)Analysis.OverallPerformance;
            ExpenseFilter = true;
            IncomeFilter = false;
            ShowValidationMessage = Analysis.HasValidationMessage;
            ValidationMessage = Analysis.ValidationMessage;

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
}
