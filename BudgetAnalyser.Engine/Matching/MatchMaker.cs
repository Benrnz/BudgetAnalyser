using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Matching
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class Matchmaker : IMatchmaker
    {
        private readonly ILogger logger;

        public Matchmaker([NotNull] ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            this.logger = logger;
        }

        public bool Match(IEnumerable<Transaction> transactions, IEnumerable<MatchingRule> rules)
        {
            bool matchesOccured = false;
            var copyOfRules = rules.ToList();
            foreach (Transaction transaction in transactions)
            {
                if (transaction.BudgetBucket == null || transaction.BudgetBucket.Code == null)
                {
                    foreach (MatchingRule rule in copyOfRules)
                    {
                        if (rule.Match(transaction))
                        {
                            transaction.BudgetBucket = rule.Bucket;
                            matchesOccured = true;
                            Transaction loggedTransaction = transaction;
                            this.logger.LogInfo(() => string.Format("Matchmaker: Transaction Matched: {0} {1:C} {2} {3} RuleId:{4}", loggedTransaction.Date, loggedTransaction.Amount, loggedTransaction.Description.Truncate(15, true), loggedTransaction.BudgetBucket.Code, rule.RuleId));
                        }
                    }
                }
            }

            return matchesOccured;
        }
    }
}
