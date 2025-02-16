namespace BudgetAnalyser.Engine.Budget.Data;

/// <summary>
///     A simple persistence type representing the kind of budget bucket.
/// </summary>
public enum BucketDtoType
{
    /// <summary>
    ///     A saved up for expense bucket. <seealso cref="SavedUpForExpenseBucket" />
    /// </summary>
    SavedUpForExpense,

    /// <summary>
    ///     The surplus bucket. <seealso cref="SurplusBucket" />
    /// </summary>
    Surplus,

    /// <summary>
    ///     The pay credit card bucket. <seealso cref="PayCreditCardBucket" />
    /// </summary>
    PayCreditCard,

    /// <summary>
    ///     An income bucket. <seealso cref="IncomeBudgetBucket" />
    /// </summary>
    Income,

    /// <summary>
    ///     A fixed budget project bucket. <seealso cref="FixedBudgetProjectBucket" />
    /// </summary>
    FixedBudgetProject,

    /// <summary>
    ///     A spent periodically (either fortnightly or monthly) expense bucket. <seealso cref="SpentPerPeriodExpenseBucket" />
    /// </summary>
    SpentPeriodicallyExpense
}
