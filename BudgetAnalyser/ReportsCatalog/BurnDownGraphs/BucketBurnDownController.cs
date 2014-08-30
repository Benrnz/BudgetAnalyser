using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Media;
using BudgetAnalyser.Annotations;
using BudgetAnalyser.Converters;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Statement;
using Rees.Wpf;

namespace BudgetAnalyser.ReportsCatalog.BurnDownGraphs
{
    public class BucketBurnDownController : ControllerBase
    {
        private readonly IBurnDownGraphAnalyser burnDownGraphAnalyser;
        private SeriesData doNotUseBalanceLine;
        private SeriesData doNotUseTrendLine;

        public BucketBurnDownController([NotNull] IBurnDownGraphAnalyser burnDownGraphAnalyser)
        {
            if (burnDownGraphAnalyser == null)
            {
                throw new ArgumentNullException("burnDownGraphAnalyser");
            }

            this.burnDownGraphAnalyser = burnDownGraphAnalyser;
        }

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

        public BudgetBucket Bucket { get; private set; }

        public SeriesData BudgetLine
        {
            get { return this.doNotUseTrendLine; }

            private set
            {
                this.doNotUseTrendLine = value;
                RaisePropertyChanged(() => BudgetLine);
            }
        }

        public string ChartTitle { get; private set; }

        public bool IsCustomChart { get; private set; }

        public SeriesData ZeroLine { get; private set; }

        public BucketBurnDownController Load(
            StatementModel statementModel,
            BudgetModel budgetModel,
            [NotNull] BudgetBucket bucket,
            DateTime beginDate,
            Engine.Ledger.LedgerBook ledgerBook)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException("bucket");
            }
            Background = ConverterHelper.TileBackgroundBrush;
            Bucket = bucket;
            ChartTitle = string.Format(CultureInfo.CurrentCulture, "{0} Spending Chart", bucket.Code);

            this.burnDownGraphAnalyser.Analyse(statementModel, budgetModel, new[] { bucket }, beginDate, ledgerBook);
            CopyOutputFromAnalyser();

            return this;
        }

        public BucketBurnDownController LoadCustomChart(
            StatementModel statementModel,
            BudgetModel budgetModel,
            IEnumerable<BudgetBucket> buckets,
            DateTime beginDate,
            Engine.Ledger.LedgerBook ledgerBook,
            string chartTitle)
        {
            IsCustomChart = true;
            Background = ConverterHelper.SecondaryBackgroundBrush;
            ChartTitle = chartTitle;
            Bucket = null;
            this.burnDownGraphAnalyser.Analyse(statementModel, budgetModel, buckets, beginDate, ledgerBook);
            CopyOutputFromAnalyser();

            return this;
        }

        private void CopyOutputFromAnalyser()
        {
            ActualSpendingAxesMinimum = this.burnDownGraphAnalyser.GraphLines.GraphMinimumValue;
            BalanceLine = this.burnDownGraphAnalyser.GraphLines.Series.Single(s => s.SeriesName == BurnDownGraphAnalyser.BalanceSeriesName);
            BudgetLine = this.burnDownGraphAnalyser.GraphLines.Series.Single(s => s.SeriesName == BurnDownGraphAnalyser.BudgetSeriesName);
            ZeroLine = this.burnDownGraphAnalyser.GraphLines.Series.Single(s => s.SeriesName == BurnDownGraphAnalyser.ZeroSeriesName);
        }
    }
}