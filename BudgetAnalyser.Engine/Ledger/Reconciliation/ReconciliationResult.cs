using System.Collections.Generic;

namespace BudgetAnalyser.Engine.Ledger.Reconciliation
{
    /// <summary>
    ///     The result of a period end reconciliation. Bundles the newly created <see cref="LedgerEntryLine"/> and the <see cref="ToDoTask"/> collection.
    /// </summary>
    public class ReconciliationResult
    {
        /// <summary>
        ///     The reconciliation data is represented as a <see cref="LedgerEntryLine" />. This has already been added into the <see cref="LedgerBook" />.
        /// </summary>
        public LedgerEntryLine Reconciliation { get; set; }

        /// <summary>
        ///     Any tasks that result from the reconciliation. These are tasks that the user must manually perform, transfer funds between bank accounts etc.
        /// </summary>
        public IEnumerable<ToDoTask> Tasks { get; set; }
    }
}
