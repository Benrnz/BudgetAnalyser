﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Matching
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    internal class Matchmaker : IMatchmaker
    {
        private readonly ILogger logger;
        private readonly IBudgetBucketRepository bucketRepo;

        public Matchmaker([NotNull] ILogger logger, [NotNull] IBudgetBucketRepository bucketRepo)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.bucketRepo = bucketRepo ?? throw new ArgumentNullException(nameof(bucketRepo));
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
                    matchesOccured = AutoMatchBasedOnReference(transaction);

                    // If automatched based on user provided reference number.
                    if (!matchesOccured)
                    {
                        matchesOccured = MatchToRules(rules, transaction);
                    }
                });

            this.logger.LogInfo(l => l.Format("Matchmaker: Matching operation finished."));
            return matchesOccured;
        }

        private bool AutoMatchBasedOnReference(Transaction transaction)
        {
            var reference1 = transaction.Reference1?.Trim();
            var reference2 = transaction.Reference2?.Trim();
            var reference3 = transaction.Reference3?.Trim();

            if (reference1 != null && this.bucketRepo.IsValidCode(reference1))
            {
                this.logger.LogInfo(l => l.Format("Transaction '{0}' automatched by reference '{1}'", transaction.Id, reference1));
                transaction.BudgetBucket = this.bucketRepo.GetByCode(reference1);
                return true;
            }
            if (reference2 != null && this.bucketRepo.IsValidCode(reference2))
            {
                this.logger.LogInfo(l => l.Format("Transaction '{0}' automatched by reference '{1}'", transaction.Id, reference2));
                transaction.BudgetBucket = this.bucketRepo.GetByCode(reference2);
                return true;
            }
            if (reference3 != null && this.bucketRepo.IsValidCode(reference3))
            {
                this.logger.LogInfo(l => l.Format("Transaction '{0}' automatched by reference '{1}'", transaction.Id, reference3));
                transaction.BudgetBucket = this.bucketRepo.GetByCode(reference3);
                return true;
            }

            return false;
        }

        private bool MatchToRules(IEnumerable<MatchingRule> rules, Transaction transaction)
        {
            bool matchesOccured = false;
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
            return matchesOccured;
        }
    }
}