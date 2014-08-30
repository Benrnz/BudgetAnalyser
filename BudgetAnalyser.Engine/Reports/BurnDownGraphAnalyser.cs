using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Reports
{
    /// <summary>
    ///     Calculates spending data for one <see cref="BudgetBucket" /> and compares it to budget model data.
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class BurnDownGraphAnalyser : IBurnDownGraphAnalyser
    {
        public const string BalanceSeriesName = "Balance Line";
        public const string BudgetSeriesName = "Budget Line";
        public const string ZeroSeriesName = "Zero Line";
        private readonly LedgerCalculation ledgerCalculator;
        private readonly ILogger logger;

        private GraphData graphLines;

        public BurnDownGraphAnalyser([NotNull] LedgerCalculation ledgerCalculator, [NotNull] ILogger logger)
        {
            if (ledgerCalculator == null)
            {
                throw new ArgumentNullException("ledgerCalculator");
            }

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            this.ledgerCalculator = ledgerCalculator;
            this.logger = logger;
        }

        /// <summary>
        ///     Gets the series lines for the burndown graph.  It consists of three lines: Budget, Balance, and zero-line.
        /// </summary>
        public GraphData GraphLines
        {
            get { return this.graphLines; }
        }

        /// <summary>
        ///     The report transactions that decrease the total budgeted amount over time to make up the burn down graph.
        /// </summary>
        public IEnumerable<ReportTransactionWithRunningBalance> ReportTransactions { get; private set; }

        /// <summary>
        ///     Analyse the actual spending over a month as a burn down of the available budget.
        ///     The available budget is either the <paramref name="ledgerBook" /> balance of the ledger or if not tracked in the
        ///     ledger, then
        ///     the budgeted amount from the <paramref name="budgetModel" />.
        /// </summary>
        public void Analyse(
            [NotNull] StatementModel statementModel,
            [NotNull] BudgetModel budgetModel,
            IEnumerable<BudgetBucket> bucketsSubset,
            DateTime beginDate,
            [CanBeNull] LedgerBook ledgerBook)
        {
            if (statementModel == null)
            {
                throw new ArgumentNullException("statementModel");
            }

            if (budgetModel == null)
            {
                throw new ArgumentNullException("budgetModel");
            }

            List<BudgetBucket> bucketsCopy = bucketsSubset.ToList();
            this.logger.LogInfo(l => "BurnDownGraphAnalyser.Analyse: " + string.Join(" ", bucketsCopy.Select(b => b.Code)));
            this.graphLines = new GraphData();

            List<DateTime> datesOfTheMonth = YieldAllDaysInDateRange(beginDate);
            DateTime lastDate = datesOfTheMonth.Last();

            CreateZeroLine(datesOfTheMonth);

            decimal openingBalance = GetBudgetedTotal(budgetModel, ledgerBook, bucketsCopy, beginDate);
            CalculateBudgetLineValues(openingBalance, datesOfTheMonth);

            List<ReportTransactionWithRunningBalance> spendingTransactions = CollateStatementTransactions(statementModel, bucketsCopy, beginDate, lastDate, openingBalance);

            // Only relevant when calculating surplus burndown - ovespent ledgers are supplemented from surplus so affect its burndown.
            if (ledgerBook != null && bucketsCopy.OfType<SurplusBucket>().Any())
            {
                List<ReportTransaction> overSpentLedgers = this.ledgerCalculator.CalculateOverspentLedgers(statementModel, ledgerBook, beginDate).ToList();
                if (overSpentLedgers.Any())
                {
                    spendingTransactions.AddRange(overSpentLedgers.Select(t => new ReportTransactionWithRunningBalance(t)));
                    spendingTransactions = spendingTransactions.OrderBy(t => t.Date).ToList();
                    UpdateReportTransactionRunningBalances(spendingTransactions);
                }
            }

            // Copy running balance from transaction list into burndown chart data
            var actualSpending = new SeriesData { SeriesName = BalanceSeriesName, Description = "Running balance over time as transactions spend, the balance decreases." };
            GraphLines.SeriesList.Add(actualSpending);
            foreach (DateTime day in datesOfTheMonth)
            {
                if (day > DateTime.Today)
                {
                    break;
                }

                decimal dayClosingBalance = GetDayClosingBalance(spendingTransactions, day);
                actualSpending.PlotsList.Add(new DatedGraphPlot { Date = day, Amount = dayClosingBalance });
                this.logger.LogInfo(l => l.Format("    {0} Close Bal:{1:N}", day, dayClosingBalance));
            }

            ReportTransactions = spendingTransactions;
        }

        private static List<ReportTransactionWithRunningBalance> CollateStatementTransactions(
            StatementModel statementModel,
            IEnumerable<BudgetBucket> bucketsCopy,
            DateTime beginDate,
            DateTime lastDate,
            decimal openingBalance)
        {
            List<ReportTransactionWithRunningBalance> query = statementModel.Transactions
                .Join(bucketsCopy, t => t.BudgetBucket, b => b, (t, b) => t)
                .Where(t => t.Date >= beginDate && t.Date <= lastDate)
                .OrderBy(t => t.Date)
                .Select(t => new ReportTransactionWithRunningBalance { Amount = t.Amount, Date = t.Date, Narrative = t.Description })
                .ToList();

            // Insert the opening balance transaction with the earliest date in the list.
            query.Insert(0, new ReportTransactionWithRunningBalance { Amount = openingBalance, Date = beginDate.AddSeconds(-1), Narrative = "Opening Balance", Balance = openingBalance });

            UpdateReportTransactionRunningBalances(query);

            return query;
        }

        private static decimal GetBudgetModelTotalForBucket(BudgetModel budgetModel, BudgetBucket bucket)
        {
            if (bucket is JournalBucket)
            {
                // Ignore
                return 0;
            }

            if (bucket is IncomeBudgetBucket)
            {
                throw new InvalidOperationException("Code Error: Income bucket detected when Bucket Spending only applies to Expenses.");
            }

            // Use budget values instead
            if (bucket is SurplusBucket)
            {
                return budgetModel.Surplus;
            }

            Expense budget = budgetModel.Expenses.FirstOrDefault(e => e.Bucket == bucket);
            if (budget != null)
            {
                return budget.Amount;
            }

            return 0;
        }

        private static decimal GetDayClosingBalance(IEnumerable<ReportTransactionWithRunningBalance> spendingTransactions, DateTime day)
        {
            return spendingTransactions.Last(t => t.Date <= day).Balance;
        }

        private static decimal GetLedgerBalanceForBucket(BudgetModel budgetModel, LedgerEntryLine applicableLine, BudgetBucket bucket)
        {
            if (bucket is SurplusBucket)
            {
                return applicableLine.CalculatedSurplus;
            }

            LedgerEntry ledger = applicableLine.Entries.FirstOrDefault(e => e.LedgerColumn.BudgetBucket == bucket);
            if (ledger != null)
            {
                return ledger.Balance;
            }

            // The Ledger line might not actually have a ledger for the given bucket.
            return GetBudgetModelTotalForBucket(budgetModel, bucket);
        }

        private static void UpdateReportTransactionRunningBalances(List<ReportTransactionWithRunningBalance> query)
        {
            decimal balance = query.First().Balance;
            // Skip 1 because the first row has the opening balance.
            foreach (ReportTransactionWithRunningBalance transaction in query.Skip(1))
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
            DateTime startDate = beginDate;
            DateTime end = beginDate.AddMonths(1).AddDays(-1);

            var data = new List<DateTime>();
            DateTime current = startDate;
            do
            {
                data.Add(current);
                current = current.AddDays(1);
            } while (current <= end);

            return data;
        }

        private void CalculateBudgetLineValues(decimal budgetTotal, List<DateTime> datesOfTheMonth)
        {
            decimal average = budgetTotal / datesOfTheMonth.Count(); 

            var seriesData = new SeriesData
            {
                Description = "The budget line shows ideal linear spending over the month to keep within your budget.",
                SeriesName = BudgetSeriesName,
            };
            this.graphLines.SeriesList.Add(seriesData);

            int iteration = 0;
            foreach (var day in datesOfTheMonth)
            {
                seriesData.PlotsList.Add(new DatedGraphPlot { Amount = budgetTotal - (average * iteration++), Date = day });
            }
        }

        private void CreateZeroLine(IEnumerable<DateTime> datesOfTheMonth)
        {
            var series = new SeriesData
            {
                SeriesName = ZeroSeriesName,
                Description = "Zero line",
            };
            GraphLines.SeriesList.Add(series);
            foreach (DateTime day in datesOfTheMonth)
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

            LedgerEntryLine applicableLine = this.ledgerCalculator.LocateApplicableLedgerLine(ledgerBook, beginDate);
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
    }
}