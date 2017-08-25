using System;
using BudgetAnalyser.Engine.Ledger.Reconciliation;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Ledger
{
    [AutoRegisterWithIoC]
    internal class LedgerBookFactory : ILedgerBookFactory
    {
        private readonly IReconciliationBuilder reconciliationBuilder;

        public LedgerBookFactory([NotNull] IReconciliationBuilder reconciliationBuilder)
        {
            if (reconciliationBuilder == null)
            {
                throw new ArgumentNullException(nameof(reconciliationBuilder));
            }

            this.reconciliationBuilder = reconciliationBuilder;
        }

        public LedgerBook CreateNew()
        {
            return new LedgerBook(this.reconciliationBuilder);
        }
    }
}