using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Widgets
{
    /// <summary>
    ///     A widget to show the number of overspent buckets for the month. Compares actual spent transactions against a ledger
    ///     in the ledgerbook, if there is one, or the current Budget if there isn't.
    ///     The budget used is the currently selected budget from the <see cref="BudgetCurrencyContext" /> instance given.  It
    ///     may not be the current one as compared to today's date.
    /// </summary>
    public class OverspentWarning : Widget
    {
        private decimal tolerance;

        /// <summary>
        ///     Initializes a new instance of the <see cref="OverspentWarning" /> class.
        /// </summary>
        public OverspentWarning()
        {
            Category = WidgetGroup.MonthlyTrackingSectionName;
            Dependencies = new[]
            {
                typeof(StatementModel), typeof(IBudgetCurrencyContext), typeof(GlobalFilterCriteria),
                typeof(LedgerBook), typeof(LedgerCalculation)
            };
            DetailedText = "Overspent";
            ImageResourceName = null;
            RecommendedTimeIntervalUpdate = TimeSpan.FromHours(12); // Every 12 hours.
            Tolerance = 10; // By default must be overspent by 10 dollars to be considered overspent.
        }

        internal IEnumerable<KeyValuePair<BudgetBucket, decimal>> OverSpentSummary { get; private set; }

        /// <summary>
        ///     Gets or sets the tolerance dollar value.
        ///     By default must be overspent by 10 dollars to be considered overspent.
        /// </summary>
        public decimal Tolerance
        {
            get { return this.tolerance; }
            set { this.tolerance = Math.Abs(value); }
        }

        /// <summary>
        ///     Updates the widget with new input.
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
        public override void Update([NotNull] params object[] input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (!ValidateUpdateInput(input))
            {
                Enabled = false;
                return;
            }

            var statement = (StatementModel) input[0];
            var budget = (IBudgetCurrencyContext) input[1];
            var filter = (GlobalFilterCriteria) input[2];
            var ledgerBook = (LedgerBook) input[3];
            var ledgerCalculator = (LedgerCalculation) input[4];

            if (budget == null || ledgerBook == null || statement == null || filter == null || filter.Cleared ||
                filter.BeginDate == null || filter.EndDate == null)
            {
                Enabled = false;
                return;
            }

            if (filter.BeginDate.Value.DurationInMonths(filter.EndDate.Value) != 1)
            {
                Enabled = false;
                ToolTip = DesignedForOneMonthOnly;
                return;
            }

            Enabled = true;
            IDictionary<BudgetBucket, decimal> overspendingSummary = ledgerCalculator.CalculateCurrentMonthLedgerBalances(ledgerBook, filter, statement);
            var warnings = overspendingSummary.Count(s => s.Value < -Tolerance);

            // Check other budget buckets that are not represented in the ledger book.
            warnings += SearchForOtherNonLedgerBookOverspentBuckets(statement, filter, budget, overspendingSummary);

            if (warnings > 0)
            {
                LargeNumber = warnings.ToString(CultureInfo.CurrentCulture);
                var builder = new StringBuilder();
                OverSpentSummary = overspendingSummary.Where(kvp => kvp.Value < -Tolerance).OrderBy(kvp => kvp.Key);
                foreach (KeyValuePair<BudgetBucket, decimal> ledger in OverSpentSummary)
                {
                    builder.AppendFormat(CultureInfo.CurrentCulture, "{0} is overspent by {1:C}", ledger.Key,
                        ledger.Value);
                    builder.AppendLine();
                }

                ToolTip = builder.ToString();
                ColourStyleName = WidgetWarningStyle;
            }
            else
            {
                LargeNumber = "0";
                ToolTip = "No overspent ledgers for the month beginning " +
                          filter.BeginDate.Value.ToString("d", CultureInfo.CurrentCulture);
                ColourStyleName = WidgetStandardStyle;
            }
        }

        private int SearchForOtherNonLedgerBookOverspentBuckets(
            StatementModel statement,
            GlobalFilterCriteria filter,
            IBudgetCurrencyContext budget,
            IDictionary<BudgetBucket, decimal> overspendingSummary)
        {
            var warnings = 0;
            List<Transaction> transactions = statement.Transactions.Where(t => t.Date < filter.BeginDate?.Date.AddMonths(1)).ToList();
            foreach (var expense in budget.Model.Expenses.Where(e => e.Bucket is BillToPayExpenseBucket))
            {
                if (overspendingSummary.ContainsKey(expense.Bucket))
                {
                    continue;
                }

                var bucketBalance = expense.Amount +
                                    transactions.Where(t => t.BudgetBucket == expense.Bucket).Sum(t => t.Amount);
                overspendingSummary.Add(expense.Bucket, bucketBalance);
                if (bucketBalance < -Tolerance)
                {
                    warnings++;
                }
            }

            return warnings;
        }
    }
}