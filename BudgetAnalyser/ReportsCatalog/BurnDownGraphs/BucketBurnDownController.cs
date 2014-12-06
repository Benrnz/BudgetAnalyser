using System.Linq;
using System.Windows.Media;
using BudgetAnalyser.Converters;
using BudgetAnalyser.Engine.Reports;
using Rees.Wpf;

namespace BudgetAnalyser.ReportsCatalog.BurnDownGraphs
{
    public class BucketBurnDownController : ControllerBase, IBurnDownChartViewModel
    {
        private SeriesData doNotUseBalanceLine;
        private SeriesData doNotUseTrendLine;

        public decimal ActualSpendingAxesMinimum { get; private set; }

        public Brush Background { get; private set; }

        public SeriesData BalanceLine
        {
            get { return this.doNotUseBalanceLine; }

            private set
            {
                this.doNotUseBalanceLine = value;
                RaisePropertyChanged(() => BalanceLine);
            }
        }

        //public BudgetBucket Bucket { get; private set; }

        public SeriesData BudgetLine
        {
            get { return this.doNotUseTrendLine; }

            private set
            {
                this.doNotUseTrendLine = value;
                RaisePropertyChanged(() => BudgetLine);
            }
        }

        public string ChartTitle { get; set; }

        public bool IsCustomChart { get; private set; }

        public SeriesData ZeroLine { get; private set; }

        public void Load(BurnDownChartAnalyserResult analysisResult)
        {
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