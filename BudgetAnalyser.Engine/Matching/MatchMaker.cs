using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Matching
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    internal class Matchmaker : IMatchmaker
    {
        private readonly ILogger logger;

        public Matchmaker([NotNull] ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.logger = logger;
        }

        public bool Match([NotNull] IEnumerable<Transaction> transactions, [NotNull] IEnumerable<MatchingRule> rules)
        {
            if (transactions == null)
            {
                throw new ArgumentNullException(nameof(transactions));
            }

            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            var matchesOccured = false;
            this.logger.LogInfo(l => l.Format("Matchmaker: Matching operation started."));
            Parallel.ForEach(
                transactions,
                transaction =>
                {
                    if (transaction.BudgetBucket?.Code == null)
                    {
                        foreach (var rule in rules.ToList())
                        {
                            if (rule.Match(transaction))
                            {
                                transaction.BudgetBucket = rule.Bucket;
                                matchesOccured = true;
                                var loggedTransaction = transaction;
                                this.logger.LogInfo(
                                    l =>
                                        l.Format(
                                            "Matchmaker: Transaction Matched: {0} {1:C} {2} {3} RuleId:{4}",
                                            loggedTransaction.Date,
                                            loggedTransaction.Amount,
                                            loggedTransaction.Description.Truncate(15, true),
                                            loggedTransaction.BudgetBucket.Code,
                                            rule.RuleId));
                            }
                        }
                    }
                });

            this.logger.LogInfo(l => l.Format("Matchmaker: Matching operation finished."));
            return matchesOccured;
        }
    }
}