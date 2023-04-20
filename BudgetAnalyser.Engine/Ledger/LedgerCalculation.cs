﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger.Reconciliation;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Ledger;

/// <summary>
///     A calculator that performs common calculations on a Ledger Book.
/// </summary>
[AutoRegisterWithIoC]
public class LedgerCalculation
{
    /// <summary>
    ///     A temporary cache with a short timeout to store results from <see cref="CalculateOverSpentLedgers" />. This method
    ///     is called multiple times over a short period to build a burn down report.
    ///     I did consider using a MemoryCache here, but I don't like the fact it is IDisposable.
    ///     Static to provide a cache for the same data possibly across instances.
    /// </summary>
    private static readonly ConcurrentDictionary<string, object> CalculationsCache = new();

    private static DateTime CacheLastUpdated;
    private readonly ILogger logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LedgerCalculation" /> class.
    /// </summary>
    public LedgerCalculation()
    {
        this.logger = new NullLogger();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="LedgerCalculation" /> class.
    /// </summary>
    public LedgerCalculation(ILogger logger)
    {
        this.logger = logger;
    }

    /// <summary>
    ///     Calculates the current period ledger balances. Period is set by the two dates. In theory other periods are possible.
    /// </summary>
    /// <exception cref="System.ArgumentNullException"></exception>
    public virtual IDictionary<BudgetBucket, decimal> CalculateCurrentPeriodLedgerBalances(
        [NotNull] LedgerEntryLine ledgerLine,
        [NotNull] GlobalFilterCriteria filter,
        [NotNull] StatementModel statement)
    {
        CheckCacheForCleanUp();
        if (ledgerLine == null)
        {
            throw new ArgumentNullException(nameof(ledgerLine));
        }

        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        if (statement == null)
        {
            throw new ArgumentNullException(nameof(statement));
        }

        var ledgers = new Dictionary<BudgetBucket, decimal>();

        if (filter.Cleared || filter.BeginDate == default || filter.EndDate == default)
        {
            return ledgers;
        }

        ledgers = CalculateLedgersCurrentBalances(ledgerLine, filter.BeginDate.Value, filter.EndDate.Value, statement);

        // Check Surplus
        var surplusBalance = CalculateCurrentPeriodSurplusBalance(ledgerLine, filter, statement);
        ledgers.Add(new SurplusBucket(), surplusBalance);

        return ledgers;
    }

    /// <summary>
    ///     Calculates the current period surplus balance.  In theory other periods are possible.
    /// </summary>
    public virtual decimal CalculateCurrentPeriodSurplusBalance([NotNull] LedgerEntryLine ledgerLine, [NotNull] GlobalFilterCriteria filter, [NotNull] StatementModel statement)
    {
        CheckCacheForCleanUp();
        if (ledgerLine == null)
        {
            throw new ArgumentNullException(nameof(ledgerLine));
        }

        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        if (statement == null)
        {
            throw new ArgumentNullException(nameof(statement));
        }

        if (filter.Cleared || filter.BeginDate == default || filter.EndDate == default)
        {
            return 0;
        }

        var balance = ledgerLine.CalculatedSurplus;
        // This includes transactions coded as SURPLUS and any bucket that is a Savings commitment.  A Savings commitment is a target budgeted amount that should be transferred to a savings account.
        var transactionTotal = CalculateTransactionTotal(filter.BeginDate.Value, filter.EndDate.Value, statement, ledgerLine, SurplusBucket.SurplusCode);

        balance += transactionTotal;

        // Find any ledgers that are overpsent and subtract them from the Surplus total.  This is actually what is happening when you overspend a ledger, it spills over and spend Surplus.
        Dictionary<BudgetBucket, decimal> ledgersSummary = CalculateLedgersCurrentBalances(ledgerLine, filter.BeginDate.Value, filter.EndDate.Value, statement);
        balance += ledgersSummary.Where(kvp => kvp.Value < 0).Sum(kvp => kvp.Value);

        return balance;
    }

    /// <summary>
    ///     Calculates the current period bucket spend.  In theory other periods are possible.
    /// </summary>
    public virtual decimal CalculateCurrentPeriodBucketSpend(
        [NotNull] LedgerEntryLine ledgerLine,
        [NotNull] GlobalFilterCriteria filter,
        [NotNull] StatementModel statement,
        [NotNull] string bucketCode)
    {
        CheckCacheForCleanUp();
        if (ledgerLine == null)
        {
            throw new ArgumentNullException(nameof(ledgerLine));
        }

        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        if (statement == null)
        {
            throw new ArgumentNullException(nameof(statement));
        }

        if (bucketCode.IsNothing())
        {
            throw new ArgumentNullException(nameof(bucketCode));
        }

        if (!filter.BeginDate.HasValue || !filter.EndDate.HasValue)
        {
            return 0;
        }

        var transactionTotal = CalculateTransactionTotal(filter.BeginDate.Value, filter.EndDate.Value, statement, ledgerLine, bucketCode);
        return transactionTotal;
    }

    /// <summary>
    ///     Finds any overspent ledgers for the period and returns the date and value the of the total overspend.  This
    ///     resulting collection can then be used to offset overdrawn balances from Surplus. (Not done here).
    ///     Negative values indicate overdrawn ledgers.
    /// </summary>
    public virtual IEnumerable<ReportTransaction> CalculateOverSpentLedgers([NotNull] StatementModel statement, [NotNull] LedgerEntryLine ledgerLine, DateTime inclBeginDate, DateTime inclEndDate)
    {
        CheckCacheForCleanUp();

        if (statement == null)
        {
            throw new ArgumentNullException(nameof(statement));
        }

        if (ledgerLine == null)
        {
            throw new ArgumentNullException(nameof(ledgerLine));
        }

        // Given the same ledger, statement and begin date this data won't change.
        List<ReportTransaction> GetOverSpentTransactions() //private function
        {
            var overSpendTransactions = new List<ReportTransaction>();
            var currentDate = inclBeginDate;
            Dictionary<BudgetBucket, decimal> runningBalances = ledgerLine.Entries.ToDictionary(entry => entry.LedgerBucket.BudgetBucket, entry => entry.Balance);
            Dictionary<BudgetBucket, decimal> previousBalances = ledgerLine.Entries.ToDictionary(entry => entry.LedgerBucket.BudgetBucket, _ => 0M);

            do
            {
                var currentDateCopy = currentDate;
                foreach (var transaction in statement.Transactions.Where(t => t.Date == currentDateCopy))
                {
                    if (runningBalances.ContainsKey(transaction.BudgetBucket))
                    {
                        runningBalances[transaction.BudgetBucket] += transaction.Amount;
                    }
                }

                ProcessOverdrawnLedgers(runningBalances, previousBalances, overSpendTransactions, currentDate);

                currentDate = currentDate.AddDays(1);
            } while (currentDate <= inclEndDate);

            return overSpendTransactions;
        }

        var cacheKey = BuildCacheKey(statement, ledgerLine, inclBeginDate);
        return (IEnumerable<ReportTransaction>)GetOrAddFromCache(cacheKey, GetOverSpentTransactions);
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
            throw new ArgumentNullException(nameof(ledgerBook));
        }

        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        var line = LocateApplicableLedgerLine(ledgerBook, filter);
        if (line == null)
        {
            return 0;
        }

        return line.Entries
            .Where(ledgerEntry => ledgerEntry.LedgerBucket.BudgetBucket.Code == bucketCode)
            .Select(ledgerEntry => ledgerEntry.Balance)
            .FirstOrDefault();
    }

    /// <summary>
    ///     Locates the applicable ledger line.
    /// </summary>
    /// <exception cref="System.ArgumentNullException"></exception>
    public virtual LedgerEntryLine LocateApplicableLedgerLine(LedgerBook ledgerBook, [NotNull] GlobalFilterCriteria filter)
    {
        CheckCacheForCleanUp();
        if (ledgerBook == null)
        {
            return null;
        }

        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        if (filter.Cleared)
        {
            return ledgerBook.Reconciliations.FirstOrDefault();
        }

        return LocateApplicableLedgerLine(ledgerBook, filter.BeginDate, filter.EndDate);
    }

    public virtual LedgerEntryLine LocateApplicableLedgerLine(LedgerBook ledgerBook, DateTime? inclBeginDate, DateTime? inclEndDate)
    {
        CheckCacheForCleanUp();
        if (ledgerBook == null)
        {
            return null;
        }

        if (inclBeginDate == default || inclBeginDate.Value == DateTime.MinValue)
        {
            return null;
        }

        if (inclEndDate == default || inclEndDate.Value == DateTime.MinValue)
        {
            return null;
        }

        return LocateLedgerEntryLine(ledgerBook, inclBeginDate.Value, inclEndDate.Value);
    }

    private static string BuildCacheKey(object dependency1, object dependency2, DateTime dependentDate)
    {
        long key;
        unchecked
        {
            key = dependency1?.GetHashCode() ?? 1 * dependency2?.GetHashCode() ?? 1 * dependentDate.GetHashCode();
        }

        var keyString = key.ToString(CultureInfo.InvariantCulture);
        return keyString;
    }

    private Dictionary<BudgetBucket, decimal> CalculateLedgersCurrentBalances(LedgerEntryLine ledgerLine, DateTime inclBeginDate, DateTime inclEndDate, StatementModel statement)
    {
        var ledgersSummary = new Dictionary<BudgetBucket, decimal>();
        foreach (var entry in ledgerLine.Entries)
        {
            var transactionTotal = CalculateTransactionTotal(inclBeginDate, inclEndDate, statement, ledgerLine, entry.LedgerBucket.BudgetBucket.Code);
            var currentBalance = entry.Balance + transactionTotal;
            ledgersSummary.Add(entry.LedgerBucket.BudgetBucket, currentBalance);
        }

        return ledgersSummary;
    }

    private decimal CalculateTransactionTotal(DateTime inclBeginDate, DateTime inclEndDate, StatementModel statement, LedgerEntryLine entryLine, string bucketCode)
    {
        IEnumerable<LedgerTransaction> autoMatchLedgerTransactions = GetAutoMatchingTransactions(statement, entryLine, inclBeginDate);

        IEnumerable<Transaction> transactions = statement.Transactions
            .Where(t => t.Date >= inclBeginDate && t.Date <= inclEndDate)
            .Where(txn => !ReconciliationBuilder.IsAutoMatchingTransaction(txn, autoMatchLedgerTransactions));

        if (bucketCode == SurplusBucket.SurplusCode)
        {
            // Special processing for the special Surplus bucket. This is to allow inclusion of special projects that are Surplus subclasses. t.BudgetBucket is SurplusBucket and its subclasses.
            // Also Savings Commitment buckets which are paid out of surplus. Also PayCC transactions which are paid out of Surplus.
            // In addition credits to SAV and PAYCC should not increase the Surplus balance. These funds have been moved to another ledger.
            // TODO I think the SAVINGS bucket should be deleted. It should just simply be a SavedUpForBucket.
            transactions = transactions.Where(t => t.BudgetBucket is SurplusBucket or SavingsCommitmentBucket or PayCreditCardBucket)
                .Where(t => !(t.BudgetBucket is SavingsCommitmentBucket) || t.Amount < 0)
                .Where(t => !(t.BudgetBucket is PayCreditCardBucket) || t.Amount < 0);
        }
        else
        {
            transactions = transactions.Where(t => t.BudgetBucket != null && t.BudgetBucket.Code == bucketCode);
        }

        this.logger.LogInfo(_ =>
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Statement Transactions found that are '{bucketCode}' and not 'Auto-Matching-Transactions':");
            foreach (var txn in transactions)
            {
                builder.AppendLine($"{txn.Date:d}   {txn.Amount:F2}  {txn.Description}  {txn.Account}");
            }

            return builder.ToString();
        });
        var transactionTotal = transactions.Sum(txn => txn.Amount);
        this.logger.LogInfo(l => l.Format("Total Transactions {0:F2}", transactionTotal));
        return transactionTotal;
    }

    private static void CheckCacheForCleanUp()
    {
        var wasLastUsed = DateTime.Now.Subtract(CacheLastUpdated);
        if (wasLastUsed.Minutes > 2 && CacheLastUpdated != default)
        {
            CacheLastUpdated = default;
            CalculationsCache.Clear();
        }
    }

    private IEnumerable<LedgerTransaction> GetAutoMatchingTransactions(StatementModel statement, LedgerEntryLine entryLine, DateTime inclBeginDate)
    {
        var key = BuildCacheKey(statement, entryLine, inclBeginDate);
        var autoMatchLedgerTransactions = (List<LedgerTransaction>)GetOrAddFromCache(key, () => ReconciliationBuilder.FindAutoMatchingTransactions(entryLine, true).ToList());

        this.logger.LogInfo(
                            _ =>
                            {
                                var builder = new StringBuilder();
                                builder.AppendLine("Ledger Transactions found that are 'Auto-Matching-Transactions':");
                                foreach (var txn in autoMatchLedgerTransactions)
                                {
                                    builder.AppendLine($"{txn.Date:d}   {txn.Amount:F2}  {txn.Narrative}  {txn.AutoMatchingReference}");
                                }

                                return builder.ToString();
                            });
        return autoMatchLedgerTransactions;
    }

    private static object GetOrAddFromCache(string cacheKey, Func<object> factory)
    {
        CheckCacheForCleanUp();
        var wrappedFactory = new Func<object>(
                                              () =>
                                              {
                                                  CacheLastUpdated = DateTime.Now;
                                                  return factory();
                                              });
        return CalculationsCache.GetOrAdd(cacheKey, _ => wrappedFactory());
    }

    private static LedgerEntryLine LocateLedgerEntryLine(LedgerBook ledgerBook, DateTime inclBegin, DateTime inclEnd)
    {
        return ledgerBook.Reconciliations.FirstOrDefault(ledgerEntryLine => ledgerEntryLine.Date >= inclBegin && ledgerEntryLine.Date <= inclEnd);
    }

    private static void ProcessOverdrawnLedgers(
        Dictionary<BudgetBucket, decimal> runningBalances,
        Dictionary<BudgetBucket, decimal> previousBalances,
        List<ReportTransaction> overSpendTransactions,
        DateTime currentDate)
    {
        foreach (KeyValuePair<BudgetBucket, decimal> runningBalance in runningBalances)
        {
            var previousBalance = previousBalances[runningBalance.Key];

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
                overSpendTransactions.Add(
                                          new ReportTransaction
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
                var amount = -(previousBalance - runningBalance.Value);
                overSpendTransactions.Add(
                                          new ReportTransaction
                                          {
                                              Date = currentDate,
                                              Amount = amount,
                                              Narrative = string.Format(
                                                                        CultureInfo.CurrentCulture,
                                                                        "{0} was overdrawn, {1}. Will be supplemented from Surplus.",
                                                                        runningBalance.Key,
                                                                        amount < 0
                                                                            ? "and has been further overdrawn"
                                                                            : "has been credited, but is still overdrawn")
                                          });
                continue;
            }

            if (runningBalance.Value >= 0 && previousBalance < 0)
            {
                // Ledger was overdrawn yesterday and is now back in credit.
                overSpendTransactions.Add(
                                          new ReportTransaction
                                          {
                                              Date = currentDate,
                                              Amount = -previousBalance,
                                              Narrative =
                                                  runningBalance.Key +
                                                  " was overdrawn, and has been credited back into a positive balance."
                                          });
            }
        }
    }
}