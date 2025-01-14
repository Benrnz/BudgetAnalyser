﻿namespace BudgetAnalyser.Engine.Ledger.Reconciliation;

/// <summary>
///     An interface to describe any behaviour that can be applied to the reconciliation during month end reconciling.
///     For example: Discovering payments made from a different account from where the bucket funds are stored.
/// </summary>
public interface IReconciliationBehaviour
{
    /// <summary>
    ///     Apply the behaviour to apply changes to the reconciliation objects. The behaviour does not change any data until this method is invoked.
    /// </summary>
    void ApplyBehaviour();

    /// <summary>
    ///     Initialise the behaviour with any input it needs.
    /// </summary>
    void Initialise(params object[] anyParameters);
}
