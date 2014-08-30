﻿using System;
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
        private readonly LedgerCalculation ledgerCalculator;
        private readonly ILogger logger;

        private List<KeyValuePair<DateTime, decimal>> actualSpending;
        private List<KeyValuePair<DateTime, decimal>> budgetLine;

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
        ///     Gets a collection of x,y cordinate values used to plot a graph line. Using a List of Key Value Pairs is more
        ///     friendly with the graph control than a dictionary.
        ///     These values shows actual spending over the month.
        /// </summary>
        public IEnumerable<KeyValuePair<DateTime, decimal>> ActualSpending
        {
            get { return this.actualSpending; }
        }

        public decimal ActualSpendingAxesMinimum { get; private set; }

        /// <summary>
        ///     Gets a collection of x,y cordinate values used to plot a graph line.
        ///     These values shows how the budget should be spent in a linear fashion over the month.
        ///     Using a List of Key Value Pairs is more friendly with the graph control than a dictionary.
        /// </summary>
        public IEnumerable<KeyValuePair<DateTime, decimal>> BudgetLine
        {
            get { return this.budgetLine; }
        }

        /// <summary>
        /// The report transactions that decrease the total budgeted amount over time to make up the burn down graph.
        /// </summary>
        public IEnumerable<ReportTransactionWithRunningBalance> ReportTransactions { get; private set; }

        /// <summary>
        ///     Gets a collection of x,y cordinate values used to plot a graph line. Using a List of Key Value Pairs is more
        ///     friendly with the graph control than a dictionary.
        ///     These values are used draw a horizontal zero line on the graph.
        /// </summary>
        public IEnumerable<KeyValuePair<DateTime, decimal>> ZeroLine { get; private set; }

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
            ZeroLine = null;
            this.budgetLine = null;
            this.actualSpending = null;

            List<DateTime> datesOfTheMonth = YieldAllDaysInDateRange(beginDate);
            DateTime lastDate = datesOfTheMonth.Last();

            // TODO try using dictionaries here instead of lists of kvps.
            ZeroLine = new List<KeyValuePair<DateTime, decimal>>(datesOfTheMonth.Select(d => new KeyValuePair<DateTime, decimal>(d, 0)));

            decimal openingBalance = GetBudgetedTotal(budgetModel, ledgerBook, bucketsCopy, beginDate);
            CalculateBudgetLineValues(openingBalance);

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
            decimal dayClosingBalance = openingBalance;
            this.actualSpending = new List<KeyValuePair<DateTime, decimal>>(datesOfTheMonth.Count);
            foreach (DateTime day in datesOfTheMonth)
            {
                if (day > DateTime.Today)
                {
                    break;
                }

                dayClosingBalance = GetDayClosingBalance(spendingTransactions, day);
                this.actualSpending.Add(new KeyValuePair<DateTime, decimal>(day, dayClosingBalance));
                this.logger.LogInfo(l => l.Format("    {0} Close Bal:{1:N}", day, dayClosingBalance));
            }

            ActualSpendingAxesMinimum = dayClosingBalance < 0 ? dayClosingBalance : 0;
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

        private void CalculateBudgetLineValues(decimal budgetTotal)
        {
            decimal average = budgetTotal / ZeroLine.Count();

            this.budgetLine = new List<KeyValuePair<DateTime, decimal>>();
            int iteration = 0;
            foreach (var day in ZeroLine)
            {
                this.budgetLine.Add(new KeyValuePair<DateTime, decimal>(day.Key, budgetTotal - (average * iteration++)));
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