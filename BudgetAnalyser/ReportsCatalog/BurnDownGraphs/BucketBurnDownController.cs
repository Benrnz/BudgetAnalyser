using System;
using System.Collections.Generic;
using System.Globalization;
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
        private IEnumerable<KeyValuePair<DateTime, decimal>> doNotUseActualSpending;
        private IEnumerable<KeyValuePair<DateTime, decimal>> doNotUseTrendLine;

        public BucketBurnDownController([NotNull] IBurnDownGraphAnalyser burnDownGraphAnalyser)
        {
            if (burnDownGraphAnalyser == null)
            {
                throw new ArgumentNullException("burnDownGraphAnalyser");
            }

            this.burnDownGraphAnalyser = burnDownGraphAnalyser;
        }

        public IEnumerable<KeyValuePair<DateTime, decimal>> ActualSpending
        {
            get { return this.doNotUseActualSpending; }

            private set
            {
                this.doNotUseActualSpending = value;
                RaisePropertyChanged(() => this.ActualSpending);
            }
        }

        public decimal ActualSpendingAxesMinimum { get; private set; }

        public string ActualSpendingLabel { get; private set; }
        public Brush Background { get; private set; }

        public BudgetBucket Bucket { get; private set; }

        public IEnumerable<KeyValuePair<DateTime, decimal>> BudgetLine
        {
            get { return this.doNotUseTrendLine; }

            private set
            {
                this.doNotUseTrendLine = value;
                RaisePropertyChanged(() => this.BudgetLine);
            }
        }

        public string ChartTitle { get; private set; }

        public bool IsCustomChart { get; private set; }
        public decimal NetWorth { get; private set; }

        public IEnumerable<KeyValuePair<DateTime, decimal>> ZeroLine { get; private set; }

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
            this.Background = ConverterHelper.TileBackgroundBrush;
            this.Bucket = bucket;
            this.ActualSpendingLabel = this.Bucket.Code;
            this.ChartTitle = string.Format(CultureInfo.CurrentCulture, "{0} Spending Chart", bucket.Code);

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
            this.IsCustomChart = true;
            this.Background = ConverterHelper.SecondaryBackgroundBrush;
            this.ChartTitle = chartTitle;
            this.ActualSpendingLabel = "Combined Spending";
            this.Bucket = null;
            this.burnDownGraphAnalyser.Analyse(statementModel, budgetModel, buckets, beginDate, ledgerBook);
            CopyOutputFromAnalyser();

            return this;
        }

        private void CopyOutputFromAnalyser()
        {
            this.ActualSpendingAxesMinimum = this.burnDownGraphAnalyser.ActualSpendingAxesMinimum;
            this.ActualSpending = this.burnDownGraphAnalyser.ActualSpending;
            this.BudgetLine = this.burnDownGraphAnalyser.BudgetLine;
            this.ZeroLine = this.burnDownGraphAnalyser.ZeroLine;
            this.NetWorth = this.burnDownGraphAnalyser.NetWorth;
        }
    }
}