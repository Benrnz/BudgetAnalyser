using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Ledger
{
    public static class LedgerCalculation
    {
        public static IDictionary<BudgetBucket, decimal> CalculateCurrentMonthLedgerBalances([NotNull] LedgerBook ledgerBook, [NotNull] GlobalFilterCriteria filter, [NotNull] StatementModel statement)
        {
            if (ledgerBook == null)
            {
                throw new ArgumentNullException("ledgerBook");
            }

            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }

            if (statement == null)
            {
                throw new ArgumentNullException("statement");
            }

            var ledgers = new Dictionary<BudgetBucket, decimal>();

            if (filter.Cleared || filter.BeginDate == null)
            {
                return ledgers;
            }

            Dictionary<BudgetBucket, decimal> ledgersSummary = CalculateLedgersBalanceSummary(ledgerBook, filter.BeginDate.Value, statement);

            // Check Surplus
            decimal surplusBalance = CalculateCurrentMonthSurplusBalance(ledgerBook, filter, statement);
            ledgersSummary.Add(new SurplusBucket(), surplusBalance);

            return ledgersSummary;
        }

        public static decimal CalculateCurrentMonthSurplusBalance([NotNull] LedgerBook ledgerBook, [NotNull] GlobalFilterCriteria filter, [NotNull] StatementModel statement)
        {
            if (ledgerBook == null)
            {
                throw new ArgumentNullException("ledgerBook");
            }

            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }

            if (statement == null)
            {
                throw new ArgumentNullException("statement");
            }

            if (filter.Cleared || filter.BeginDate == null)
            {
                return 0;
            }

            LedgerEntryLine entryLine = LocateApplicableLedgerLine(ledgerBook, filter);
            if (entryLine == null)
            {
                return 0;
            }

            decimal beginningOfMonthBalance = entryLine.CalculatedSurplus;
            decimal transactionTotal = statement.Transactions
                .Where(t => t.Date < filter.BeginDate.Value.AddMonths(1) && t.BudgetBucket is SurplusBucket)
                .Sum(t => t.Amount);

            beginningOfMonthBalance += transactionTotal;
            Dictionary<BudgetBucket, decimal> ledgersSummary = CalculateLedgersBalanceSummary(ledgerBook, filter.BeginDate.Value, statement);
            beginningOfMonthBalance += ledgersSummary.Where(kvp => kvp.Value < 0).Sum(kvp => kvp.Value);

            return beginningOfMonthBalance;
        }

        /// <summary>
        /// Locates the most recent <see cref="LedgerEntryLine"/> for the given date filter. Note that this will only return the most recent line that fits the criteria.
        /// </summary>
        public static decimal LocateApplicableLedgerBalance([NotNull] LedgerBook ledgerBook, [NotNull] GlobalFilterCriteria filter, string bucketCode)
        {
            if (ledgerBook == null)
            {
                throw new ArgumentNullException("ledgerBook");
            }

            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }

            LedgerEntryLine line = LocateApplicableLedgerLine(ledgerBook, filter);
            if (line == null)
            {
                return 0;
            }

            return line.Entries
                            .Where(ledgerEntry => ledgerEntry.LedgerColumn.BudgetBucket.Code == bucketCode)
                            .Select(ledgerEntry => ledgerEntry.Balance)
                            .FirstOrDefault();
        }

        public static LedgerEntryLine LocateApplicableLedgerLine(LedgerBook ledgerBook, [NotNull] GlobalFilterCriteria filter)
        {
            if (ledgerBook == null)
            {
                return null;
            }

            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }

            if (filter.Cleared)
            {
                return ledgerBook.DatedEntries.FirstOrDefault();
            }

            Debug.Assert(filter.BeginDate != null);
            Debug.Assert(filter.EndDate != null);
            return LocateLedgerEntryLine(ledgerBook, filter.BeginDate.Value, filter.EndDate.Value);
        }

        private static Dictionary<BudgetBucket, decimal> CalculateLedgersBalanceSummary(LedgerBook ledgerBook, DateTime beginDate, StatementModel statement)
        {
            DateTime endDate = beginDate.AddMonths(1).AddDays(-1);
            List<Transaction> transactions = statement.Transactions.Where(t => t.Date < beginDate.AddMonths(1)).ToList();

            LedgerEntryLine currentLegderLine = LocateLedgerEntryLine(ledgerBook, beginDate, endDate);
            if (currentLegderLine == null)
            {
                return new Dictionary<BudgetBucket, decimal>();
            }

            var ledgersSummary = new Dictionary<BudgetBucket, decimal>();
            foreach (LedgerEntry entry in currentLegderLine.Entries)
            {
                decimal balance = entry.Balance + transactions.Where(t => t.BudgetBucket == entry.LedgerColumn.BudgetBucket).Sum(t => t.Amount);
                ledgersSummary.Add(entry.LedgerColumn.BudgetBucket, balance);
            }

            return ledgersSummary;
        }

        private static LedgerEntryLine LocateLedgerEntryLine(LedgerBook ledgerBook, DateTime begin, DateTime end)
        {
            return ledgerBook.DatedEntries.FirstOrDefault(ledgerEntryLine => ledgerEntryLine.Date >= begin && ledgerEntryLine.Date <= end);
        }
    }
}