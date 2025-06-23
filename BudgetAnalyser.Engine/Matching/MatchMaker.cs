using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Matching;

[AutoRegisterWithIoC(SingleInstance = true)]
internal class Matchmaker(ILogger logger, IBudgetBucketRepository bucketRepo) : IMatchmaker
{
    private const string LogPrefix = "Matchmaker:";
    private readonly IBudgetBucketRepository bucketRepo = bucketRepo ?? throw new ArgumentNullException(nameof(bucketRepo));
    private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private SortedSet<string> bucketCodesQuickLookup = new();

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

        SetBucketCodesQuickLookup();

        var matchesOccured = false;
        this.logger.LogInfo(l => l.Format("{0} Matching operation started.", LogPrefix));
        foreach (var transaction in transactions)
        {
            var thisMatch = AutoMatchBasedOnReference(transaction);

            // If auto-matched based on user provided reference number.
            if (!thisMatch)
            {
                thisMatch = MatchToRules(rules, transaction);
            }

            matchesOccured |= thisMatch;
        }
        // Parallel.ForEach(
        //     transactions,
        //     transaction =>
        //     {
        //         var thisMatch = AutoMatchBasedOnReference(transaction);
        //
        //         // If auto-matched based on user provided reference number.
        //         if (!thisMatch)
        //         {
        //             thisMatch = MatchToRules(rules, transaction);
        //         }
        //
        //         matchesOccured |= thisMatch;
        //     });

        this.logger.LogInfo(l => l.Format("{0} Matching operation finished.", LogPrefix));
        return matchesOccured;
    }

    private bool AutoMatchBasedOnReference(Transaction transaction)
    {
        if (MatchReferenceToBucketCode(transaction.Reference1?.Trim(), transaction))
        {
            return true;
        }

        if (MatchReferenceToBucketCode(transaction.Reference2?.Trim(), transaction))
        {
            return true;
        }

        if (MatchReferenceToBucketCode(transaction.Reference3?.Trim(), transaction))
        {
            return true;
        }

        return false;
    }

    private bool MatchReferenceToBucketCode(string? reference, Transaction transaction)
    {
        if (reference is null)
        {
            return false;
        }

        if (this.bucketRepo.IsValidCode(reference))
        {
            this.logger.LogInfo(_ => $"{LogPrefix} Transaction '{transaction.Id}' auto-matched by reference '{reference}' with exact match.");
            transaction.BudgetBucket = this.bucketRepo.GetByCode(reference);
            return true;
        }

        // Partial matches
        // Max Bucket code length is 7 characters. Partial matches should be at least 5 characters.
        reference = reference.ToUpperInvariant();
        if (reference.Length is >= 5 and < 7)
        {
            var code = this.bucketCodesQuickLookup.FirstOrDefault(code => code.StartsWith(reference));
            if (code is not null)
            {
                this.logger.LogInfo(_ => $"{LogPrefix} Transaction '{transaction.Id}' auto-matched by reference '{reference}' with partial match to {code}");
                transaction.BudgetBucket = this.bucketRepo.GetByCode(code);
                return true;
            }
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

    private void SetBucketCodesQuickLookup()
    {
        var query = this.bucketRepo.Buckets.Select(b => b.Code).OrderBy(b => b);
        this.bucketCodesQuickLookup = new SortedSet<string>(query) { SurplusBucket.SurplusCode };
    }
}
