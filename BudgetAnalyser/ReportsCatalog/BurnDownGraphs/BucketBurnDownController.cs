using System;
using System.Linq;
using System.Windows.Media;
using BudgetAnalyser.Converters;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Reports;
using Rees.Wpf;

namespace BudgetAnalyser.ReportsCatalog.BurnDownGraphs
{
    public class BucketBurnDownController : ControllerBase
    {
        private SeriesData doNotUseBalanceLine;
        private SeriesData doNotUseTrendLine;

        public decimal ActualSpendingAxesMinimum { [UsedImplicitly] get; private set; }

        public Brush Background { get; private set; }

        public SeriesData BalanceLine
        {
            [UsedImplicitly] get { return this.doNotUseBalanceLine; }

            private set
            {
                this.doNotUseBalanceLine = value;
                RaisePropertyChanged();
            }
        }

        public SeriesData BudgetLine
        {
            [UsedImplicitly] get { return this.doNotUseTrendLine; }

            private set
            {
                this.doNotUseTrendLine = value;
                RaisePropertyChanged();
            }
        }

        public string ChartTitle { get; private set; }
        public bool IsCustomChart { get; private set; }

        public SeriesData ZeroLine { [UsedImplicitly] get; private set; }

        public void Load([NotNull] BurnDownChartAnalyserResult analysisResult)
        {
            if (analysisResult == null)
            {
                throw new ArgumentNullException(nameof(analysisResult));
            }

            if (analysisResult.IsCustomAggregateChart)
            {
                LoadCustomChart(analysisResult);
            }
            else
            {
                LoadBucketChart(analysisResult);
            }
        }

        private void CopyOutputFromAnalyser(BurnDownChartAnalyserResult analysisResult)
        {
            ActualSpendingAxesMinimum = analysisResult.GraphLines.GraphMinimumValue;
            BalanceLine = analysisResult.GraphLines.Series.Single(s => s.SeriesName == BurnDownChartAnalyserResult.BalanceSeriesName);
            BudgetLine = analysisResult.GraphLines.Series.Single(s => s.SeriesName == BurnDownChartAnalyserResult.BudgetSeriesName);
            ZeroLine = analysisResult.GraphLines.Series.Single(s => s.SeriesName == BurnDownChartAnalyserResult.ZeroSeriesName);
            ChartTitle = analysisResult.ChartTitle;
        }

        private void LoadBucketChart(BurnDownChartAnalyserResult analysisResult)
        {
            Background = ConverterHelper.TileBackgroundBrush;
            CopyOutputFromAnalyser(analysisResult);
        }

        private void LoadCustomChart(BurnDownChartAnalyserResult analysisResult)
        {
            IsCustomChart = true;
            Background = ConverterHelper.SecondaryBackgroundBrush;
            CopyOutputFromAnalyser(analysisResult);
        }
    }
}