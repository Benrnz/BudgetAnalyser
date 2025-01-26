using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Matching;

[AutoRegisterWithIoC(SingleInstance = true)]
internal class Matchmaker(ILogger logger, IBudgetBucketRepository bucketRepo) : IMatchmaker
{
    private const string LogPrefix = "Matchmaker:";
    private readonly IBudgetBucketRepository bucketRepo = bucketRepo ?? throw new ArgumentNullException(nameof(bucketRepo));
    private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public bool Match(IEnumerable<Transaction> transactions, IEnumerable<MatchingRule> rules)
    {
        if (transactions is null)
        {
            throw new ArgumentNullException(nameof(transactions));
        }

        if (rules is null)
        {
            throw new ArgumentNullException(nameof(rules));
        }

        var matchesOccured = false;
        this.logger.LogInfo(l => l.Format("{0} Matching operation started.", LogPrefix));
        Parallel.ForEach(
            transactions,
            transaction =>
            {
                var thisMatch = AutoMatchBasedOnReference(transaction);

                // If auto-matched based on user provided reference number.
                if (!thisMatch)
                {
                    thisMatch = MatchToRules(rules, transaction);
                }

                matchesOccured |= thisMatch;
            });

        this.logger.LogInfo(l => l.Format("{0} Matching operation finished.", LogPrefix));
        return matchesOccured;
    }

    private bool AutoMatchBasedOnReference(Transaction transaction)
    {
        var reference1 = transaction.Reference1?.Trim();
        var reference2 = transaction.Reference2?.Trim();
        var reference3 = transaction.Reference3?.Trim();

        if (reference1 is not null && this.bucketRepo.IsValidCode(reference1))
        {
            this.logger.LogInfo(l => l.Format("{0} Transaction '{1}' auto-matched by reference '{2}'", LogPrefix, transaction.Id, reference1));
            transaction.BudgetBucket = this.bucketRepo.GetByCode(reference1);
            return true;
        }

        if (reference2 is not null && this.bucketRepo.IsValidCode(reference2))
        {
            this.logger.LogInfo(l => l.Format("{0} Transaction '{1}' auto-matched by reference '{2}'", LogPrefix, transaction.Id, reference2));
            transaction.BudgetBucket = this.bucketRepo.GetByCode(reference2);
            return true;
        }

        if (reference3 is not null && this.bucketRepo.IsValidCode(reference3))
        {
            this.logger.LogInfo(l => l.Format("{0} Transaction '{1}' auto-matched by reference '{2}'", LogPrefix, transaction.Id, reference3));
            transaction.BudgetBucket = this.bucketRepo.GetByCode(reference3);
            return true;
        }

        return false;
    }

    private bool MatchToRules(IEnumerable<MatchingRule> rules, Transaction transaction)
    {
        var matchesOccured = false;
        if (transaction.BudgetBucket?.Code is null)
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
                                "{0} Transaction Matched: {1} {2:C} {3} {4} RuleId:{5}",
                                LogPrefix,
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
