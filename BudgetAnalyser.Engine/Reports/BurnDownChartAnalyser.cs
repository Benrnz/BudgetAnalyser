using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Reports
{
    [AutoRegisterWithIoC]
    internal class BurnDownChartAnalyser : IBurnDownChartAnalyser
    {
        private readonly LedgerCalculation ledgerCalculator;
        private readonly ILogger logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BurnDownChartAnalyser" /> class.
        /// </summary>
        /// <param name="ledgerCalculator">The ledger calculator.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public BurnDownChartAnalyser([NotNull] LedgerCalculation ledgerCalculator, [NotNull] ILogger logger)
        {
            if (ledgerCalculator == null)
            {
                throw new ArgumentNullException(nameof(ledgerCalculator));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.ledgerCalculator = ledgerCalculator;
            this.logger = logger;
        }

        public BurnDownChartAnalyserResult Analyse(StatementModel statementModel, BudgetModel budgetModel,
                                                   IEnumerable<BudgetBucket> bucketsSubset, LedgerBook ledgerBook, DateTime beginDate)
        {
            if (statementModel == null)
            {
                throw new ArgumentNullException(nameof(statementModel));
            }

            if (budgetModel == null)
            {
                throw new ArgumentNullException(nameof(budgetModel));
            }

            List<BudgetBucket> bucketsCopy = bucketsSubset.ToList();
            this.logger.LogInfo(
                l => "BurnDownChartAnalyser.Analyse: " + string.Join(" ", bucketsCopy.Select(b => b.Code)));

            var result = new BurnDownChartAnalyserResult();

            List<DateTime> datesOfTheMonth = YieldAllDaysInDateRange(beginDate);
            var lastDate = datesOfTheMonth.Last();

            CreateZeroLine(datesOfTheMonth, result);

            var openingBalance = GetBudgetedTotal(budgetModel, ledgerBook, bucketsCopy, beginDate);
            CalculateBudgetLineValues(openingBalance, datesOfTheMonth, result);

            List<ReportTransactionWithRunningBalance> spendingTransactions = CollateStatementTransactions(statementModel, bucketsCopy, beginDate, lastDate,
                openingBalance);

            // Only relevant when calculating surplus burndown - ovespent ledgers are supplemented from surplus so affect its burndown.
            if (ledgerBook != null && bucketsCopy.OfType<SurplusBucket>().Any())
            {
                List<ReportTransaction> overSpentLedgers =
                    this.ledgerCalculator.CalculateOverspentLedgers(statementModel, ledgerBook, beginDate).ToList();
                if (overSpentLedgers.Any())
                {
                    spendingTransactions.AddRange(
                        overSpentLedgers.Select(t => new ReportTransactionWithRunningBalance(t)));
                    spendingTransactions = spendingTransactions.OrderBy(t => t.Date).ToList();
                    UpdateReportTransactionRunningBalances(spendingTransactions);
                }
            }

            // Copy running balance from transaction list into burndown chart data
            var actualSpending = new SeriesData
            {
                SeriesName = BurnDownChartAnalyserResult.BalanceSeriesName,
                Description = "Running balance over time as transactions spend, the balance decreases."
            };
            result.GraphLines.SeriesList.Add(actualSpending);
            foreach (var day in datesOfTheMonth)
            {
                if (day > DateTime.Today)
                {
                    break;
                }

                var dayClosingBalance = GetDayClosingBalance(spendingTransactions, day);
                actualSpending.PlotsList.Add(new DatedGraphPlot { Date = day, Amount = dayClosingBalance });
                var copyOfDay = day;
                this.logger.LogInfo(l => l.Format("    {0} Close Bal:{1:N}", copyOfDay, dayClosingBalance));
            }

            result.ReportTransactions = spendingTransactions;
            return result;
        }

        private static void CalculateBudgetLineValues(decimal budgetTotal, List<DateTime> datesOfTheMonth,
                                                      BurnDownChartAnalyserResult result)
        {
            var average = budgetTotal / datesOfTheMonth.Count();

            var seriesData = new SeriesData
            {
                Description = "The budget line shows ideal linear spending over the month to keep within your budget.",
                SeriesName = BurnDownChartAnalyserResult.BudgetSeriesName
            };
            result.GraphLines.SeriesList.Add(seriesData);

            var iteration = 0;
            foreach (var day in datesOfTheMonth)
            {
                seriesData.PlotsList.Add(new DatedGraphPlot { Amount = budgetTotal - average * iteration++, Date = day });
            }
        }

        private static List<ReportTransactionWithRunningBalance> CollateStatementTransactions(
            StatementModel statementModel,
            IList<BudgetBucket> bucketsToInclude,
            DateTime beginDate,
            DateTime lastDate,
            decimal openingBalance)
        {
            // The below query has to cater for special Surplus buckets which are intended to be equivelent but use a type hierarchy with inheritance.
            List<ReportTransactionWithRunningBalance> query = statementModel.Transactions
                .Join(bucketsToInclude, t => t.BudgetBucket, b => b, (t, b) => t, new SurplusAgnosticBucketComparer())
                .Where(t => t.Date >= beginDate && t.Date <= lastDate)
                .OrderBy(t => t.Date)
                .Select(
                    t =>
                        new ReportTransactionWithRunningBalance
                        {
                            Amount = t.Amount,
                            Date = t.Date,
                            Narrative = t.Description
                        })
                .ToList();

            // Insert the opening balance transaction with the earliest date in the list.
            query.Insert(0,
                new ReportTransactionWithRunningBalance
                {
                    Amount = openingBalance,
                    Date = beginDate.AddSeconds(-1),
                    Narrative = "Opening Balance",
                    Balance = openingBalance
                });

            UpdateReportTransactionRunningBalances(query);

            return query;
        }

        private static void CreateZeroLine(IEnumerable<DateTime> datesOfTheMonth, BurnDownChartAnalyserResult result)
        {
            var series = new SeriesData
            {
                SeriesName = BurnDownChartAnalyserResult.ZeroSeriesName,
                Description = "Zero line"
            };
            result.GraphLines.SeriesList.Add(series);
            foreach (var day in datesOfTheMonth)
            {
                series.PlotsList.Add(new DatedGraphPlot { Amount = 0, Date = day });
            }
        }

        /// <summary>
        ///     Calculates the appropriate budgeted amount for the given buckets.
        ///     This can either be the ledger balance from the ledger book or if not tracked by the ledger book, then from the
        ///     budget model.
        /// </summary>
        private decimal GetBudgetedTotal(
            [NotNull] BudgetModel budgetModel,
            [CanBeNull] LedgerBook ledgerBook,
            [NotNull] IEnumerable<BudgetBucket> buckets,
            DateTime beginDate)
        {
            decimal budgetTotal = 0;
            List<BudgetBucket> bucketsCopy = buckets.ToList();

            var applicableLine = this.ledgerCalculator.LocateApplicableLedgerLine(ledgerBook, beginDate);
            if (applicableLine == null)
            {
                // Use budget values from budget model instead, there is no ledger book line for this month.
                budgetTotal += bucketsCopy.Sum(bucket => GetBudgetModelTotalForBucket(budgetModel, bucket));
            }
            else
            {
                budgetTotal += bucketsCopy.Sum(bucket => GetLedgerBalanceForBucket(budgetModel, applicableLine, bucket));
            }

            return budgetTotal;
        }

        private static decimal GetBudgetModelTotalForBucket(BudgetModel budgetModel, BudgetBucket bucket)
        {
            if (bucket is PayCreditCardBucket)
            {
                // Ignore
                return 0;
            }

            if (bucket is IncomeBudgetBucket)
            {
                throw new InvalidOperationException(
                    "Code Error: Income bucket detected when Bucket Spending only applies to Expenses.");
            }

            // Use budget values instead
            if (bucket is SurplusBucket)
            {
                return budgetModel.Surplus;
            }

            var budget = budgetModel.Expenses.FirstOrDefault(e => e.Bucket == bucket);
            if (budget != null)
            {
                return budget.Amount;
            }

            return 0;
        }

        private static decimal GetDayClosingBalance(
            IEnumerable<ReportTransactionWithRunningBalance> spendingTransactions, DateTime day)
        {
            return spendingTransactions.Last(t => t.Date <= day).Balance;
        }

        private static decimal GetLedgerBalanceForBucket(BudgetModel budgetModel, LedgerEntryLine applicableLine,
                                                         BudgetBucket bucket)
        {
            if (bucket is SurplusBucket)
            {
                return applicableLine.CalculatedSurplus;
            }

            var ledger = applicableLine.Entries.FirstOrDefault(e => e.LedgerBucket.BudgetBucket == bucket);
            if (ledger != null)
            {
                return ledger.Balance;
            }

            // The Ledger line might not actually have a ledger for the given bucket.
            return GetBudgetModelTotalForBucket(budgetModel, bucket);
        }

        private static void UpdateReportTransactionRunningBalances(List<ReportTransactionWithRunningBalance> query)
        {
            var balance = query.First().Balance;
            // Skip 1 because the first row has the opening balance.
            foreach (var transaction in query.Skip(1))
            {
                balance += transaction.Amount;
                transaction.Balance = balance;
            }
        }

        /// <summary>
        ///     Populate a dictionary with an entry for each day of a month beginning at the start date.
        /// </summary>
        private static List<DateTime> YieldAllDaysInDateRange(DateTime beginDate)
        {
            var startDate = beginDate;
            var end = beginDate.AddMonths(1).AddDays(-1);

            var data = new List<DateTime>();
            var current = startDate;
            do
            {
                data.Add(current);
                current = current.AddDays(1);
            } while (current <= end);

            return data;
        }
    }
}