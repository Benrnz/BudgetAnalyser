using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Media;
using BudgetAnalyser.Annotations;
using BudgetAnalyser.Converters;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Statement;
using Rees.Wpf;

namespace BudgetAnalyser.Budget
{
    public class BucketSpendingController : ControllerBase
    {
        private readonly ISpendingGraphAnalyser spendingGraphAnalyser;
        private List<KeyValuePair<DateTime, decimal>> doNotUseActualSpending;
        private List<KeyValuePair<DateTime, decimal>> doNotUseTrendLine;

        public BucketSpendingController([NotNull] ISpendingGraphAnalyser spendingGraphAnalyser)
        {
            if (spendingGraphAnalyser == null)
            {
                throw new ArgumentNullException("spendingGraphAnalyser");
            }

            this.spendingGraphAnalyser = spendingGraphAnalyser;
        }

        public List<KeyValuePair<DateTime, decimal>> ActualSpending
        {
            get { return this.doNotUseActualSpending; }

            private set
            {
                this.doNotUseActualSpending = value;
                RaisePropertyChanged(() => ActualSpending);
            }
        }

        public decimal ActualSpendingAxesMinimum { get; private set; }

        public string ActualSpendingLabel { get; private set; }
        public Brush Background { get; private set; }

        public BudgetBucket Bucket { get; private set; }

        public List<KeyValuePair<DateTime, decimal>> BudgetLine
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
        public decimal NetWorth { get; private set; }

        public List<KeyValuePair<DateTime, decimal>> ZeroLine { get; private set; }

        public BucketSpendingController Load(StatementModel statementModel, BudgetModel budgetModel, BudgetBucket bucket, GlobalFilterCriteria criteria)
        {
            Background = ConverterHelper.SecondaryBackgroundBrush;
            Bucket = bucket;
            ActualSpendingLabel = Bucket.Code;
            ChartTitle = string.Format(CultureInfo.CurrentCulture, "{0} Spending Chart", bucket.Code);

            this.spendingGraphAnalyser.Analyse(statementModel, budgetModel, new[] { bucket }, criteria);
            CopyOutputFromAnalyser();

            return this;
        }

        public BucketSpendingController LoadCustomChart(
            StatementModel statementModel,
            BudgetModel budgetModel,
            IEnumerable<BudgetBucket> buckets,
            GlobalFilterCriteria criteria,
            string chartTitle)
        {
            IsCustomChart = true;
            Background = ConverterHelper.TertiaryBackgroundBrush;
            ChartTitle = chartTitle;
            ActualSpendingLabel = "Combined Spending";
            Bucket = null;
            this.spendingGraphAnalyser.Analyse(statementModel, budgetModel, buckets, criteria);
            CopyOutputFromAnalyser();

            return this;
        }

        private void CopyOutputFromAnalyser()
        {
            ActualSpendingAxesMinimum = this.spendingGraphAnalyser.ActualSpendingAxesMinimum;
            ActualSpending = this.spendingGraphAnalyser.ActualSpending;
            BudgetLine = this.spendingGraphAnalyser.BudgetLine;
            ZeroLine = this.spendingGraphAnalyser.ZeroLine;
            NetWorth = this.spendingGraphAnalyser.NetWorth;
        }
    }
}