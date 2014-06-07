using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Reports
{
    public class OverallPerformanceBudgetAnalysis
    {
        private readonly IBudgetBucketRepository bucketRepository;
        private readonly BudgetCollection budgets;
        private readonly StatementModel statement;

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Custom collection")]
        public OverallPerformanceBudgetAnalysis([NotNull] StatementModel statement, [NotNull] BudgetCollection budgets, [NotNull] IBudgetBucketRepository bucketRepository)
        {
            if (statement == null)
            {
                throw new ArgumentNullException("statement");
            }

            if (budgets == null)
            {
                throw new ArgumentNullException("budgets");
            }

            if (bucketRepository == null)
            {
                throw new ArgumentNullException("bucketRepository");
            }

            this.statement = statement;
            this.budgets = budgets;
            this.bucketRepository = bucketRepository;
        }

        public IEnumerable<BucketPerformanceAnalysis> Analyses { get; private set; }

        /// <summary>
        ///     Gets the average spend per month based on statement transaction data over a period of time.
        ///     Expected to be negative.
        /// </summary>
        public decimal AverageSpend { get; private set; }

        /// <summary>
        ///     Gets the average surplus spending per month based on statement transaction data over a period of time.
        /// </summary>
        public decimal AverageSurplus { get; private set; }

        public int DurationInMonths { get; private set; }

        public decimal OverallPerformance { get; private set; }
        public decimal TotalBudgetExpenses { get; private set; }

        public bool UsesMultipleBudgets { get; private set; }

        /// <summary>
        ///     Analyses the supplied statement using the supplied budget within the criteria given to this method.
        /// </summary>
        /// <param name="criteria">The criteria to limit the analysis.</param>
        /// <exception cref="BudgetException">
        ///     Will be thrown if no budget is supplied or if no budget can be found for the dates
        ///     given in the criteria.
        /// </exception>
        /// <exception cref="ArgumentException">If statement or budget is null.</exception>
        public void Analyse([NotNull] GlobalFilterCriteria criteria)
        {
            DateTime endDate, beginDate;
            AnalysisPreconditions(criteria, out beginDate, out endDate);

            List<BudgetModel> budgetsInvolved = this.budgets.ForDates(beginDate, endDate).ToList();
            UsesMultipleBudgets = budgetsInvolved.Count() > 1;
            BudgetModel currentBudget = budgetsInvolved.Last(); // Use most recent budget as the current

            DurationInMonths = StatementModel.CalculateDuration(criteria, this.statement.Transactions);

            CalculateTotalsAndAverage(beginDate);

            Analyses = new List<BucketPerformanceAnalysis>();
            var list = new List<BucketPerformanceAnalysis>();
            foreach (BudgetBucket bucket in this.bucketRepository.Buckets)
            {
                BudgetBucket bucketCopy = bucket;
                List<Transaction> query = this.statement.Transactions.Where(t => t.BudgetBucket == bucketCopy).ToList();
                decimal totalSpent = query.Sum(t => t.Amount);
                decimal averageSpend = totalSpent / DurationInMonths;

                if (bucket == this.bucketRepository.SurplusBucket)
                {
                    decimal budgetedTotal = CalculateBudgetedTotalAmount(beginDate, b => b.Surplus);
                    decimal perMonthBudget = budgetedTotal / DurationInMonths; // Calc an average in case multiple budgets are used and the budgeted amounts are different.
                    var surplusAnalysis = new BucketPerformanceAnalysis
                    {
                        Bucket = bucket,
                        TotalSpent = -totalSpent,
                        Balance = budgetedTotal - totalSpent,
                        BudgetTotal = budgetedTotal,
                        Budget = perMonthBudget,
                        AverageSpend = -averageSpend,
                        BudgetComparedToAverage = string.Format(CultureInfo.CurrentCulture, "Budget per Month: {0:C}, Actual per Month: {1:C}", perMonthBudget, -averageSpend)
                    };
                    list.Add(surplusAnalysis);
                    continue;
                }

                // If the most recent budget does not contain this bucket, then skip it.
                if (currentBudget.Expenses.Any(e => e.Bucket == bucket))
                {
                    decimal totalBudget = CalculateBudgetedTotalAmount(beginDate, BuildExpenseFinder(bucket));
                    decimal perMonthBudget = totalBudget / DurationInMonths;
                    var analysis = new BucketPerformanceAnalysis
                    {
                        Bucket = bucket,
                        TotalSpent = -totalSpent,
                        Balance = totalBudget - totalSpent,
                        BudgetTotal = totalBudget,
                        Budget = perMonthBudget,
                        AverageSpend = -averageSpend,
                        BudgetComparedToAverage = string.Format(CultureInfo.CurrentCulture, "Budget per Month: {0:C}, Actual per Month: {1:C}", perMonthBudget, -averageSpend)
                    };
                    list.Add(analysis);
                    continue;
                }

                // If the most recent budget does not contain this bucket, then skip it.
                if (currentBudget.Incomes.Any(i => i.Bucket == bucket))
                {
                    decimal totalBudget = CalculateBudgetedTotalAmount(beginDate, BuildIncomeFinder(bucket));
                    decimal perMonthBudget = totalBudget / DurationInMonths;
                    var analysis = new BucketPerformanceAnalysis
                    {
                        Bucket = bucket,
                        TotalSpent = totalSpent,
                        Balance = totalBudget - totalSpent,
                        BudgetTotal = totalBudget,
                        Budget = perMonthBudget,
                        AverageSpend = averageSpend,
                        BudgetComparedToAverage = string.Format(CultureInfo.CurrentCulture, "Budget per Month: {0:C}, Actual per month: {1:C}", perMonthBudget, -averageSpend)
                    };
                    list.Add(analysis);
                }
            }

            Analyses = list.OrderByDescending(a => a.Percent).ToList();
        }

        private static Func<BudgetModel, decimal> BuildExpenseFinder(BudgetBucket bucket)
        {
            return b =>
            {
                Expense first = b.Expenses.FirstOrDefault(e => e.Bucket == bucket);
                if (first == null)
                {
                    return 0;
                }

                return first.Amount;
            };
        }

        private static Func<BudgetModel, decimal> BuildIncomeFinder(BudgetBucket bucket)
        {
            return b =>
            {
                Income first = b.Incomes.FirstOrDefault(e => e.Bucket == bucket);
                if (first == null)
                {
                    return 0;
                }

                return first.Amount;
            };
        }

        private void AnalysisPreconditions(GlobalFilterCriteria criteria, out DateTime beginDate, out DateTime endDate)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria");
            }

            if (!criteria.Cleared && (criteria.BeginDate == null || criteria.EndDate == null))
            {
                throw new ArgumentException("The given criteria does not contain any filtering dates.");
            }

            if (this.statement == null)
            {
                throw new ArgumentException("The statement supplied is null, analysis cannot proceed with no statement.");
            }

            if (this.budgets == null)
            {
                throw new ArgumentException("budgets");
            }

            if (criteria.Cleared)
            {
                beginDate = this.statement.AllTransactions.First().Date;
                endDate = this.statement.AllTransactions.Last().Date;
            }
            else
            {
                beginDate = criteria.BeginDate.Value;
                endDate = criteria.EndDate.Value;
            }
        }

        private decimal CalculateBudgetedTotalAmount(DateTime beginDate, Func<BudgetModel, decimal> whichBudgetBucket)
        {
            if (!UsesMultipleBudgets)
            {
                return whichBudgetBucket(this.budgets.ForDate(beginDate)) * DurationInMonths;
            }

            decimal budgetedAmount = 0;
            for (int month = 0; month < DurationInMonths; month++)
            {
                BudgetModel budget = this.budgets.ForDate(beginDate.AddMonths(month));
                budgetedAmount += whichBudgetBucket(budget);
            }

            return budgetedAmount;
        }

        private void CalculateTotalsAndAverage(DateTime beginDate)
        {
            // First total the expenses without the saved up for expenses.
            decimal totalExpensesSpend = this.statement.Transactions
                .Where(t => t.BudgetBucket is ExpenseBucket)
                .Sum(t => t.Amount);

            decimal totalSurplusSpend = this.statement.Transactions
                .Where(t => t.BudgetBucket is SurplusBucket)
                .Sum(t => t.Amount);

            AverageSpend = totalExpensesSpend / DurationInMonths; // Expected to be negative
            AverageSurplus = totalSurplusSpend / DurationInMonths; // Expected to be negative

            for (int month = 0; month < DurationInMonths; month++)
            {
                BudgetModel budget = this.budgets.ForDate(beginDate.AddMonths(month));
                TotalBudgetExpenses += budget.Expenses.Sum(e => e.Amount);
            }

            OverallPerformance = AverageSpend + TotalBudgetExpenses;
        }
    }
}