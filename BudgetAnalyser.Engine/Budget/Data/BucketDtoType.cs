﻿namespace BudgetAnalyser.Engine.Budget.Data;

/// <summary>
///     A simple persistence type representing the kind of budget bucket.
/// </summary>
public enum BucketDtoType
{
    /// <summary>
    ///     A spent monthly expense bucket. <seealso cref="SpentPerPeriodExpenseBucket" />
    ///     OBSOLETE DO NOT USE THIS.  Kept only for backwards compatibility.  When deserialising this will be automatically
    ///     changed to SpentPeriodicallyExpense.
    /// </summary>
    SpentMonthlyExpense,

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
    ///     A savings commitment bucket. 
    ///     OBSOLETE DO NOT USE THIS. Use SavedUpForExpenseBucket instead.
    /// </summary>
    SavingsCommitment,

    /// <summary>
    ///     A fixed budget project bucket. <seealso cref="FixedBudgetProjectBucket" />
    /// </summary>
    FixedBudgetProject,

    /// <summary>
    ///     A spent periodically (either fortnightly or monthly) expense bucket. <seealso cref="SpentPerPeriodExpenseBucket" />
    /// </summary>
    SpentPeriodicallyExpense
}
