namespace BudgetAnalyser.Engine.Budget;

/// <summary>
///     An income bucket to classify any income, regular gifting, interest-income, salary or wage.
/// </summary>
/// <seealso cref="BudgetAnalyser.Engine.Budget.BudgetBucket" />
public class IncomeBudgetBucket : BudgetBucket
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="IncomeBudgetBucket" /> class.
    /// </summary>
    public IncomeBudgetBucket()
    {
        // Default constructor required for deserialisation.
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="IncomeBudgetBucket" /> class.
    /// </summary>
    public IncomeBudgetBucket(string code, string name) : base(code, name)
    {
    }
}
