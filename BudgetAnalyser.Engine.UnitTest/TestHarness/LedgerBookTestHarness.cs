using System;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Reconciliation;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.UnitTest.TestHarness
{
    internal class LedgerBookTestHarness : LedgerBook
    {
        /// <summary>
        ///     Constructs a new instance of the <see cref="LedgerBook" /> class.  The Persistence system calls this constructor,
        ///     not the IoC system.
        /// </summary>
        public LedgerBookTestHarness()
        {
        }

        public Action<ReconciliationResult> ReconcileOverride { get; set; }

        internal override void Reconcile(ReconciliationResult newRecon)
        {
            ReconcileOverride?.Invoke(newRecon);
        }
    }
}