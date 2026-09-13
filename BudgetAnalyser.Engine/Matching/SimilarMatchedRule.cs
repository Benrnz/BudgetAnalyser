using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Matching;

/// <summary>
///     Used for the purpose of showing similar rules as a check when creating a new rule. This is a utility for UI purposes not a persisted rule.
/// </summary>
public class SimilarMatchedRule : MatchingRule
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SimilarMatchedRule" /> class.
    /// </summary>
    /// <param name="bucketRepository">The bucket repository.</param>
    /// <param name="rule">The rule.</param>
    /// <exception cref="System.ArgumentNullException"></exception>
    public SimilarMatchedRule(IBudgetBucketRepository bucketRepository, MatchingRule rule)
        : base(bucketRepository)
    {
        if (rule is null)
        {
            throw new ArgumentNullException(nameof(rule));
        }

        Amount = rule.Amount;
        And = rule.And;
        Description = rule.Description;
        Reference1 = rule.Reference1;
        Reference2 = rule.Reference2;
        Reference3 = rule.Reference3;
        TransactionType = rule.TransactionType;
        BucketCode = rule.Bucket.Code;
        RuleId = rule.RuleId;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the amount property was used to match.
    /// </summary>
    public bool AmountMatched
    {
        get;
        set
        {
            if (value == field)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the description property was used to match.
    /// </summary>
    public bool DescriptionMatched
    {
        get;
        set
        {
            if (value == field)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gets or sets a value indicating whether reference1 property was used to match.
    /// </summary>
    public bool Reference1Matched
    {
        get;
        set
        {
            if (value == field)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gets or sets a value indicating whether reference2 property was used to match.
    /// </summary>
    public bool Reference2Matched
    {
        get;
        set
        {
            if (value == field)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gets or sets a value indicating whether reference3 property was used to match.
    /// </summary>
    public bool Reference3Matched
    {
        get;
        set
        {
            if (value == field)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gets the sort order.
    /// </summary>
    public int SortOrder => (AmountMatched ? 100 : 0)
                            + (DescriptionMatched ? 90 : 0)
                            + (Reference1Matched ? 70 : 0)
                            + (Reference2Matched ? 60 : 0)
                            + (Reference3Matched ? 50 : 0)
                            + (TransactionTypeMatched ? 10 : 0);

    /// <summary>
    ///     Gets or sets a value indicating whether the transaction type property was used to match.
    /// </summary>
    public bool TransactionTypeMatched
    {
        get;
        set
        {
            if (value == field)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    }
}
