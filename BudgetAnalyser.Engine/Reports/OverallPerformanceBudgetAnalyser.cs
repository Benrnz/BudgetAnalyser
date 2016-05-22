using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Reports
{
    /// <summary>
    ///     A analyser class to build report data for the overall performance report.
    /// </summary>
    [AutoRegisterWithIoC]
    internal class OverallPerformanceBudgetAnalyser
    {
        private readonly IBudgetBucketRepository bucketRepository;

        public OverallPerformanceBudgetAnalyser([NotNull] IBudgetBucketRepository bucketRepository)
        {
            if (bucketRepository == null)
            {
                throw new ArgumentNullException(nameof(bucketRepository));
            }

            this.bucketRepository = bucketRepository;
        }

        /// <summary>
        ///     Analyses the supplied statement using the supplied budget within the criteria given to this method.
        /// </summary>
        /// <param name="budgets">The current budgets collection.</param>
        /// <param name="criteria">The criteria to limit the analysis.</param>
        /// <param name="statementModel">The current statement model.</param>
        /// <exception cref="BudgetException">
        ///     Will be thrown if no budget is supplied or if no budget can be found for the dates
        ///     given in the criteria.
        /// </exception>
        /// <exception cref="ArgumentException">If statement or budget is null.</exception>
        public virtual OverallPerformanceBudgetResult Analyse(StatementModel statementModel, BudgetCollection budgets,
                                                              [NotNull] GlobalFilterCriteria criteria)
        {
            DateTime endDate, beginDate;
            AnalysisPreconditions(criteria, statementModel, budgets, out beginDate, out endDate);

            var result = new OverallPerformanceBudgetResult();

            List<BudgetModel> budgetsInvolved = budgets.ForDates(beginDate, endDate).ToList();
            result.UsesMultipleBudgets = budgetsInvolved.Count() > 1;
            var currentBudget = budgetsInvolved.Last(); // Use most recent budget as the current

            result.DurationInMonths = StatementModel.CalculateDuration(criteria, statementModel.Transactions);

            CalculateTotalsAndAverage(beginDate, statementModel, budgets, result);

            result.AnalysesList = new List<BucketPerformanceResult>();
            var list = new List<BucketPerformanceResult>();
            foreach (var bucket in this.bucketRepository.Buckets)
            {
                var bucketCopy = bucket;
                List<Transaction> query = statementModel.Transactions.Where(t => t.BudgetBucket == bucketCopy).ToList();
                var totalSpent = query.Sum(t => t.Amount);
                var averageSpend = totalSpent / result.DurationInMonths;

                if (bucket == this.bucketRepository.SurplusBucket)
                {
                    var budgetedTotal = CalculateBudgetedTotalAmount(beginDate, b => b.Surplus, budgets, result);
                    var perMonthBudget = budgetedTotal / result.DurationInMonths;
                    // Calc an average in case multiple budgets are used and the budgeted amounts are different.
                    var surplusAnalysis = new BucketPerformanceResult
                    {
                        Bucket = bucket,
                        TotalSpent = -totalSpent,
                        Balance = budgetedTotal - totalSpent,
                        BudgetTotal = budgetedTotal,
                        Budget = perMonthBudget,
                        AverageSpend = -averageSpend,
                        BudgetComparedToAverage =
                            string.Format(CultureInfo.CurrentCulture, "Budget per Month: {0:C}, Actual per Month: {1:C}",
                                perMonthBudget, -averageSpend)
                    };
                    list.Add(surplusAnalysis);
                    continue;
                }

                // If the most recent budget does not contain this bucket, then skip it.
                if (currentBudget.Expenses.Any(e => e.Bucket == bucket))
                {
                    var totalBudget = CalculateBudgetedTotalAmount(beginDate, BuildExpenseFinder(bucket), budgets,
                        result);
                    var perMonthBudget = totalBudget / result.DurationInMonths;
                    var analysis = new BucketPerformanceResult
                    {
                        Bucket = bucket,
                        TotalSpent = -totalSpent,
                        Balance = totalBudget - totalSpent,
                        BudgetTotal = totalBudget,
                        Budget = perMonthBudget,
                        AverageSpend = -averageSpend,
                        BudgetComparedToAverage =
                            string.Format(CultureInfo.CurrentCulture, "Budget per Month: {0:C}, Actual per Month: {1:C}",
                                perMonthBudget, -averageSpend)
                    };
                    list.Add(analysis);
                    continue;
                }

                // If the most recent budget does not contain this bucket, then skip it.
                if (currentBudget.Incomes.Any(i => i.Bucket == bucket))
                {
                    var totalBudget = CalculateBudgetedTotalAmount(beginDate, BuildIncomeFinder(bucket), budgets, result);
                    var perMonthBudget = totalBudget / result.DurationInMonths;
                    var analysis = new BucketPerformanceResult
                    {
                        Bucket = bucket,
                        TotalSpent = totalSpent,
                        Balance = totalBudget - totalSpent,
                        BudgetTotal = totalBudget,
                        Budget = perMonthBudget,
                        AverageSpend = averageSpend,
                        BudgetComparedToAverage =
                            string.Format(CultureInfo.CurrentCulture, "Budget per Month: {0:C}, Actual per month: {1:C}",
                                perMonthBudget, -averageSpend)
                    };
                    list.Add(analysis);
                }
            }

            result.AnalysesList = list.OrderByDescending(a => a.Percent).ToList();
            return result;
        }

        private static void AnalysisPreconditions(GlobalFilterCriteria criteria, StatementModel statement,
                                                  BudgetCollection budgets, out DateTime beginDate, out DateTime endDate)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            if (!criteria.Cleared && (criteria.BeginDate == null || criteria.EndDate == null))
            {
                throw new ArgumentException("The given criteria does not contain any filtering dates.");
            }

            if (statement == null)
            {
                throw new ArgumentNullException(nameof(statement),
                    "The statement supplied is null, analysis cannot proceed with no statement.");
            }

            if (budgets == null)
            {
                throw new ArgumentNullException(nameof(budgets));
            }

            if (criteria.Cleared)
            {
                beginDate = statement.AllTransactions.First().Date;
                endDate = statement.AllTransactions.Last().Date;
            }
            else
            {
                beginDate = criteria.BeginDate ?? DateTime.MinValue;
                endDate = criteria.EndDate ?? DateTime.MinValue;
            }
        }

        private static Func<BudgetModel, decimal> BuildExpenseFinder(BudgetBucket bucket)
        {
            return b =>
            {
                var first = b.Expenses.FirstOrDefault(e => e.Bucket == bucket);
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
                var first = b.Incomes.FirstOrDefault(e => e.Bucket == bucket);
                if (first == null)
                {
                    return 0;
                }

                return first.Amount;
            };
        }

        private static decimal CalculateBudgetedTotalAmount(DateTime beginDate,
                                                            Func<BudgetModel, decimal> whichBudgetBucket, BudgetCollection budgets,
                                                            OverallPerformanceBudgetResult result)
        {
            if (!result.UsesMultipleBudgets)
            {
                return whichBudgetBucket(budgets.ForDate(beginDate)) * result.DurationInMonths;
            }

            decimal budgetedAmount = 0;
            for (var month = 0; month < result.DurationInMonths; month++)
            {
                var budget = budgets.ForDate(beginDate.AddMonths(month));
                budgetedAmount += whichBudgetBucket(budget);
            }

            return budgetedAmount;
        }

        private static void CalculateTotalsAndAverage(DateTime beginDate, StatementModel statement,
                                                      BudgetCollection budgets, OverallPerformanceBudgetResult result)
        {
            // First total the expenses without the saved up for expenses.
            var totalExpensesSpend = statement.Transactions
                .Where(t => t.BudgetBucket is ExpenseBucket)
                .Sum(t => t.Amount);

            var totalSurplusSpend = statement.Transactions
                .Where(t => t.BudgetBucket is SurplusBucket)
                .Sum(t => t.Amount);

            result.AverageSpend = totalExpensesSpend / result.DurationInMonths; // Expected to be negative
            result.AverageSurplus = totalSurplusSpend / result.DurationInMonths; // Expected to be negative

            for (var month = 0; month < result.DurationInMonths; month++)
            {
                var budget = budgets.ForDate(beginDate.AddMonths(month));
                result.TotalBudgetExpenses += budget.Expenses.Sum(e => e.Amount);
            }

            result.OverallPerformance = result.AverageSpend + result.TotalBudgetExpenses;
        }
    }
}