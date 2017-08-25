using System.Collections.Generic;

namespace BudgetAnalyser.Engine.Ledger.Reconciliation
{
    /// <summary>
    /// A factory responsible for constructing instances of <see cref="IReconciliationBehaviour"/>.
    /// </summary>
    public class ReconciliationBehaviourFactory
    {
        /// <summary>
        /// Returns a list of all available behaviours to apply during month end reconciliation.
        /// </summary>
        public static IEnumerable<IReconciliationBehaviour> ListAllBehaviours()
        {
            return new IReconciliationBehaviour[] { new ReconciliationBehaviourPaidFromWrongAccount(), };
        }
    }
}
