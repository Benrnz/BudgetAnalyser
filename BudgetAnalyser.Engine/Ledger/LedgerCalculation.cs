using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Ledger
{
    [AutoRegisterWithIoC]
    public class LedgerCalculation
    {
        /// <summary>
        ///     A temporary cache with a short timeout to store results from <see cref="CalculateOverspentLedgers" />. This method
        ///     is called multiple times over a short period to build a burn down report.
        ///     I did consider using a MemoryCache here, but I don't like the fact it is IDisposable.
        /// </summary>
        private static readonly ConcurrentDictionary<string, object> CalculationsCache = new ConcurrentDictionary<string, object>();

        private static DateTime CacheLastUpdated;

        public virtual IDictionary<BudgetBucket, decimal> CalculateCurrentMonthLedgerBalances([NotNull] LedgerBook ledgerBook, [NotNull] GlobalFilterCriteria filter, [NotNull] StatementModel statement)
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

        public virtual decimal CalculateCurrentMonthSurplusBalance([NotNull] LedgerBook ledgerBook, [NotNull] GlobalFilterCriteria filter, [NotNull] StatementModel statement)
        {
            CheckCacheForCleanUp();
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

            // Find any ledgers that are overpsent and subtract them from the Surplus total.  This is actually what is happening when you overspend a ledger, it spills over and spend Surplus.
            Dictionary<BudgetBucket, decimal> ledgersSummary = CalculateLedgersBalanceSummary(ledgerBook, filter.BeginDate.Value, statement);
            beginningOfMonthBalance += ledgersSummary.Where(kvp => kvp.Value < 0).Sum(kvp => kvp.Value);

            return beginningOfMonthBalance;
        }

        /// <summary>
        ///     Finds any overspent ledgers for the month and returns the date and value the of the total overspend.  This
        ///     resulting
        ///     collection can then be used to subtract from Surplus.
        ///     Overdrawn ledgers are supplemented from Surplus.
        ///     Negative values indicate overdrawn ledgers.
        /// </summary>
        public virtual IEnumerable<ReportTransaction> CalculateOverspentLedgers([NotNull] StatementModel statement, [NotNull] LedgerBook ledger, DateTime beginDate)
        {
            CheckCacheForCleanUp();

            if (statement == null)
            {
                throw new ArgumentNullException("statement");
            }

            if (ledger == null)
            {
                throw new ArgumentNullException("ledger");
            }

            // Given the same ledger, statement and begin date this data won't change.
            IEnumerable<ReportTransaction> cachedValue = FoundInOverSpentLedgerCache(statement, ledger, beginDate);
            if (cachedValue != null)
            {
                return cachedValue;
            }

            var overSpendTransactions = new List<ReportTransaction>();
            LedgerEntryLine ledgerLine = LocateApplicableLedgerLine(ledger, beginDate);
            if (ledgerLine == null)
            {
                return overSpendTransactions;
            }

            DateTime endDate = beginDate.AddMonths(1);
            DateTime currentDate = beginDate;
            Dictionary<BudgetBucket, decimal> runningBalances = ledgerLine.Entries.ToDictionary(entry => entry.LedgerBucket.BudgetBucket, entry => entry.Balance);
            Dictionary<BudgetBucket, decimal> previousBalances = ledgerLine.Entries.ToDictionary(entry => entry.LedgerBucket.BudgetBucket, entry => 0M);

            do
            {
                DateTime currentDateCopy = currentDate;
                foreach (Transaction transaction in statement.Transactions.Where(t => t.Date == currentDateCopy))
                {
                    if (runningBalances.ContainsKey(transaction.BudgetBucket))
                    {
                        runningBalances[transaction.BudgetBucket] += transaction.Amount;
                    }
                }

                ProcessOverdrawnLedgers(runningBalances, previousBalances, overSpendTransactions, currentDate);

                currentDate = currentDate.AddDays(1);
            } while (currentDate < endDate);

            AddToCache(statement, ledger, beginDate, overSpendTransactions);
            return overSpendTransactions;
        }

        /// <summary>
        ///     Locates the most recent <see cref="LedgerEntryLine" /> for the given date filter. Note that this will only return
        ///     the most recent line that fits the criteria.
        /// </summary>
        public virtual decimal LocateApplicableLedgerBalance([NotNull] LedgerBook ledgerBook, [NotNull] GlobalFilterCriteria filter, string bucketCode)
        {
            CheckCacheForCleanUp();
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
                .Where(ledgerEntry => ledgerEntry.LedgerBucket.BudgetBucket.Code == bucketCode)
                .Select(ledgerEntry => ledgerEntry.Balance)
                .FirstOrDefault();
        }

        public virtual LedgerEntryLine LocateApplicableLedgerLine(LedgerBook ledgerBook, [NotNull] GlobalFilterCriteria filter)
        {
            CheckCacheForCleanUp();
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
                return ledgerBook.Reconciliations.FirstOrDefault();
            }

            Debug.Assert(filter.BeginDate != null);
            Debug.Assert(filter.EndDate != null);
            return LocateLedgerEntryLine(ledgerBook, filter.BeginDate.Value, filter.EndDate.Value);
        }

        public virtual LedgerEntryLine LocateApplicableLedgerLine(LedgerBook ledgerBook, DateTime beginDate)
        {
            CheckCacheForCleanUp();
            if (ledgerBook == null)
            {
                return null;
            }

            return LocateLedgerEntryLine(ledgerBook, beginDate, beginDate.AddMonths(1).AddDays(-1));
        }

        private static void AddToCache(StatementModel statement, LedgerBook ledger, DateTime beginDate, object cacheData)
        {
            CacheLastUpdated = DateTime.Now;
            Thread.MemoryBarrier();
            string keyString = BuildCacheKey(statement, ledger, beginDate);
            CalculationsCache.AddOrUpdate(keyString, key => cacheData, (key, o) => cacheData);
        }

        private static string BuildCacheKey(StatementModel statement, LedgerBook ledger, DateTime beginDate)
        {
            long key;
            unchecked
            {
                key = statement.GetHashCode() * ledger.GetHashCode() * beginDate.GetHashCode();
            }

            string keyString = key.ToString(CultureInfo.InvariantCulture);
            return keyString;
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
                var transactionsSubset = transactions.Where(t => t.BudgetBucket == entry.LedgerBucket.BudgetBucket);
                decimal balance = entry.Balance + transactionsSubset.Sum(t => t.Amount);
                ledgersSummary.Add(entry.LedgerBucket.BudgetBucket, balance);
            }

            return ledgersSummary;
        }

        private static void CheckCacheForCleanUp()
        {
            TimeSpan wasLastUsed = DateTime.Now.Subtract(CacheLastUpdated);
            if (wasLastUsed.Minutes > 2 && CacheLastUpdated != default(DateTime))
            {
                CacheLastUpdated = default(DateTime);
                Thread.MemoryBarrier();
                CalculationsCache.Clear();
            }
        }

        private static IEnumerable<ReportTransaction> FoundInOverSpentLedgerCache(StatementModel statement, LedgerBook ledger, DateTime beginDate)
        {
            string keyString = BuildCacheKey(statement, ledger, beginDate);
            object item;
            if (CalculationsCache.TryGetValue(keyString, out item))
            {
                CacheLastUpdated = DateTime.Now;
                Thread.MemoryBarrier();
                return (IEnumerable<ReportTransaction>)item;
            }

            return null;
        }

        private static LedgerEntryLine LocateLedgerEntryLine(LedgerBook ledgerBook, DateTime begin, DateTime end)
        {
            return ledgerBook.Reconciliations.FirstOrDefault(ledgerEntryLine => ledgerEntryLine.Date >= begin && ledgerEntryLine.Date <= end);
        }

        private static void ProcessOverdrawnLedgers(
            Dictionary<BudgetBucket, decimal> runningBalances,
            Dictionary<BudgetBucket, decimal> previousBalances,
            List<ReportTransaction> overSpendTransactions,
            DateTime currentDate)
        {
            foreach (var runningBalance in runningBalances)
            {
                decimal previousBalance = previousBalances[runningBalance.Key];

                // Update previous balance with today's
                previousBalances[runningBalance.Key] = runningBalance.Value;

                if (previousBalance == runningBalance.Value)
                {
                    // Previous balance and current balance are the same so there is no change, ledger hasn't got any worse or better.
                    continue;
                }

                if (runningBalance.Value < 0 && previousBalance >= 0)
                {
                    // Ledger has been overdrawn today.
                    overSpendTransactions.Add(new ReportTransaction
                    {
                        Date = currentDate,
                        Amount = runningBalance.Value,
                        Narrative = runningBalance.Key + " overdrawn - will be supplemented from Surplus."
                    });
                    continue;
                }

                if (runningBalance.Value < 0 && previousBalance < 0)
                {
                    // Ledger was overdrawn yesterday and is still overdrawn today. Ensure the difference is added to the overSpend.
                    decimal amount = -(previousBalance - runningBalance.Value);
                    overSpendTransactions.Add(new ReportTransaction
                    {
                        Date = currentDate,
                        Amount = amount,
                        Narrative = string.Format(
                            CultureInfo.CurrentCulture,
                            "{0} was overdrawn, {1}. Will be supplemented from Surplus.",
                            runningBalance.Key,
                            amount < 0 ? "and has been further overdrawn" : "has been credited, but is still overdrawn"),
                    });
                    continue;
                }

                if (runningBalance.Value >= 0 && previousBalance < 0)
                {
                    // Ledger was overdrawn yesterday and is now back in credit.
                    overSpendTransactions.Add(new ReportTransaction
                    {
                        Date = currentDate,
                        Amount = -previousBalance,
                        Narrative = runningBalance.Key + " was overdrawn, and has been credited back into a positive balance."
                    });
                }
            }
        }
    }
}