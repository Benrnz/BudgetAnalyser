using System.Collections.Generic;

namespace BudgetAnalyser.Engine.Ledger.Reconciliation
{
    /// <summary>
    ///     A factory responsible for constructing instances of <see cref="IReconciliationBehaviour" />.
    /// </summary>
    public class ReconciliationBehaviourFactory
    {
        /// <summary>
        ///     Returns a list of all available behaviours to apply during month end reconciliation.
        /// </summary>
        public static IEnumerable<IReconciliationBehaviour> ListAllBehaviours()
        {
            // The order is important
            // 1) CreateBalAdjustmentsForBudgetAmounts
            // 2) AddBalanceAdjustments For Future Transactions
            // 3) Apply Ledger Bucket specific behaviour
            // 4) WrongAccount
            // 5) Overdrawn Surplus last, as it cant be calculated until all other transactions are created.

            return new IReconciliationBehaviour[]
            {
                new ReconciliationBehaviourBudgetAmountBalanceAdjustments(),
                new ReconciliationBehaviourBalanceAdjustsForFutureTransactions(),
                new ReconciliationBehaviourForLedgerBucket(),
                new ReconciliationBehaviourPaidFromWrongAccount(),
                new ReconciliationBehaviourOverdrawnSurplus(),
            };
        }
    }
}