namespace BudgetAnalyser.Engine.Budget;

/// <summary>
///     This is a special system bucket used to classify payments to credit cards so those transactions do not interfere
///     with calculations on other buckets. It is considered a subset of the Surplus bucket. Funds debited using PAYCC are considered to come out of the Surplus.
/// </summary>
/// <seealso cref="BudgetAnalyser.Engine.Budget.BudgetBucket" />
public class PayCreditCardBucket : BudgetBucket
{
    /// <summary>
    ///     The constant 'pay credit card code'
    /// </summary>
    public const string PayCreditCardCode = "PAYCC";

    /// <summary>
    ///     Initializes a new instance of the <see cref="PayCreditCardBucket" /> class.
    /// </summary>
    public PayCreditCardBucket()
    {
        // Default constructor required for deserialisation.
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PayCreditCardBucket" /> class.
    /// </summary>
    public PayCreditCardBucket(string code, string description) : base(code, description)
    {
    }
}
