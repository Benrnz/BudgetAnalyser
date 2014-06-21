using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Widgets
{
    public class OverspentWarning : Widget
    {
        private decimal tolerance;

        public OverspentWarning()
        {
            Category = "Monthly Budget";
            Dependencies = new[] { typeof(StatementModel), typeof(BudgetCurrencyContext), typeof(GlobalFilterCriteria), typeof(LedgerBook) };
            DetailedText = "Overspent";
            ImageResourceName = null;
            RecommendedTimeIntervalUpdate = TimeSpan.FromHours(12); // Every 12 hours.
            Tolerance = 10;
        }

        public decimal Tolerance
        {
            get { return this.tolerance; }
            set { this.tolerance = Math.Abs(value); }
        }

        public override void Update([NotNull] params object[] input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (!ValidateUpdateInput(input))
            {
                Enabled = false;
                return;
            }

            Enabled = true;
            var statement = (StatementModel)input[0];
            var budget = (BudgetCurrencyContext)input[1];
            var filter = (GlobalFilterCriteria)input[2];
            var ledgerBook = (LedgerBook)input[3];

            if (ledgerBook == null || statement == null || filter == null || filter.Cleared || filter.BeginDate == null || budget == null)
            {
                Enabled = false;
                return;
            }

            IDictionary<BudgetBucket, decimal> overspendingSummary = LedgerCalculation.CalculateCurrentMonthLedgerBalances(ledgerBook, filter, statement);
            int warnings = overspendingSummary.Count(s => s.Value < -Tolerance);

            // Check other budget buckets that are not represented in the ledger book.
            SearchForOverspentLedgers(statement, filter, budget, overspendingSummary, warnings);

            if (warnings > 0)
            {
                LargeNumber = warnings.ToString(CultureInfo.CurrentCulture);
                var builder = new StringBuilder();
                foreach (var ledger in overspendingSummary.Where(kvp => kvp.Value < -Tolerance).OrderBy(kvp => kvp.Key))
                {
                    builder.AppendFormat(CultureInfo.CurrentCulture, "{0} is overspent by {1:C}", ledger.Key, ledger.Value);
                    builder.AppendLine();
                }

                ToolTip = builder.ToString();
                ColourStyleName = WidgetWarningStyle;
            }
            else
            {
                LargeNumber = "0";
                ToolTip = "No overspent ledgers for the month beginning " + filter.BeginDate.Value.ToString("d", CultureInfo.CurrentCulture);
                ColourStyleName = WidgetStandardStyle;
            }
        }

        private void SearchForOverspentLedgers(StatementModel statement, GlobalFilterCriteria filter, BudgetCurrencyContext budget, IDictionary<BudgetBucket, decimal> overspendingSummary, int warnings)
        {
            List<Transaction> transactions = statement.Transactions.Where(t => t.Date < filter.BeginDate.Value.AddMonths(1)).ToList();
            foreach (Expense expense in budget.Model.Expenses.Where(e => e.Bucket is BillToPayExpenseBucket))
            {
                if (overspendingSummary.ContainsKey(expense.Bucket))
                {
                    continue;
                }

                decimal bucketBalance = expense.Amount + transactions.Where(t => t.BudgetBucket == expense.Bucket).Sum(t => t.Amount);
                overspendingSummary.Add(expense.Bucket, bucketBalance);
                if (bucketBalance < -this.Tolerance)
                {
                    warnings++;
                }
            }
        }
    }
}