using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Widgets
{
    /// <summary>
    ///     A savings goal widget to monitor a <see cref="SavingsCommitmentBucket" />
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Widgets.ProgressBarWidget" />
    public class SavingsGoalWidget : ProgressBarWidget
    {
        private readonly string standardStyle;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SavingsGoalWidget" /> class.
        /// </summary>
        public SavingsGoalWidget()
        {
            Category = WidgetGroup.OverviewSectionName;
            Dependencies = new[]
            {
                typeof(IBudgetCurrencyContext), typeof(StatementModel), typeof(GlobalFilterCriteria),
                typeof(LedgerBook)
            };
            RecommendedTimeIntervalUpdate = TimeSpan.FromHours(12); // Every 12 hours.
            this.standardStyle = "WidgetStandardStyle3";
            DetailedText = "Savings Commitment";
            Name = "Save Goal";
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

            var budget = (IBudgetCurrencyContext) input[0];
            var statement = (StatementModel) input[1];
            var filter = (GlobalFilterCriteria) input[2];
            var ledger = (LedgerBook) input[3];

            if (statement == null || budget == null || filter == null || filter.Cleared || filter.BeginDate == null ||
                filter.EndDate == null)
            {
                Enabled = false;
                return;
            }

            Enabled = true;
            var totalMonths = filter.BeginDate.Value.DurationInMonths(filter.EndDate.Value);
            Maximum =
                Convert.ToDouble(budget.Model.Expenses.Where(s => s.Bucket is SavingsCommitmentBucket)
                    .Sum(s => s.Amount)) * totalMonths;

            var savingsToDate = CalculateSavingsToDateWithTrackedLedgers(statement, ledger);

            Value = Convert.ToDouble(savingsToDate);
            ToolTip = string.Format(CultureInfo.CurrentCulture, "You have saved {0:C} of your monthly goal {1:C}",
                savingsToDate, Maximum);

            if ((double) savingsToDate < 0.9 * Maximum)
            {
                ColourStyleName = WidgetWarningStyle;
            }
            else
            {
                ColourStyleName = this.standardStyle;
            }
        }

        private static decimal CalculateSavingsToDateWithTrackedLedgers(StatementModel statement, LedgerBook ledger)
        {
            if (ledger == null)
            {
                return 0;
            }

            List<BudgetBucket> trackedSavingsLedgers = ledger.Ledgers
                .Where(l => l.BudgetBucket is SavingsCommitmentBucket)
                .Select(l => l.BudgetBucket)
                .ToList();
            if (!trackedSavingsLedgers.Any())
            {
                return SumDebitSavingsTransactions(statement);
            }

            var savingsToDate = CalculateTrackedSavingLedgersContributions(statement, trackedSavingsLedgers);

            // Other non-ledger-book-tracked savings will appear as debits in the statement so need to be negated.
            IEnumerable<Transaction> otherNontrackedSavings =
                statement.Transactions.Where(
                    t =>
                        t.BudgetBucket is SavingsCommitmentBucket && trackedSavingsLedgers.All(b => b != t.BudgetBucket));
            savingsToDate += otherNontrackedSavings.Sum(t => -t.Amount);
            return savingsToDate;
        }

        private static decimal CalculateTrackedSavingLedgersContributions(StatementModel statement,
                                                                          IEnumerable<BudgetBucket> trackedSavingsLedgers)
        {
            decimal savingsToDate = 0;
            foreach (var bucket in trackedSavingsLedgers)
            {
                List<Transaction> transactions = statement.Transactions.Where(t => t.BudgetBucket == bucket).ToList();

                // This will give interest earned.  This is because the transaction list will contain both debits and credits for transfering the savings around.
                savingsToDate += transactions.Sum(t => t.Amount);

                // This will give the savings credited.
                IEnumerable<IGrouping<decimal, decimal>> amounts = transactions
                    .Select(t => Math.Abs(t.Amount))
                    .GroupBy(amount => amount, amount => amount)
                    .Where(group => @group.Count() > 1);
                savingsToDate += amounts.Distinct().Sum(g => g.Key);
            }

            return savingsToDate;
        }

        private static decimal SumDebitSavingsTransactions(StatementModel statement)
        {
            var savingsToDate =
                -statement.Transactions.Where(t => t.BudgetBucket is SavingsCommitmentBucket && t.Amount < 0)
                    .Sum(t => t.Amount);
            if (savingsToDate < 0)
            {
                savingsToDate = 0;
            }

            return savingsToDate;
        }
    }
}