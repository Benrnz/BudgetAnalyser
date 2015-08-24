using System;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Ledger
{
    [AutoRegisterWithIoC]
    internal class LedgerBookFactory : ILedgerBookFactory
    {
        private readonly ILogger logger;
        private readonly IReconciliationBuilder reconciliationBuilder;

        public LedgerBookFactory([NotNull] IReconciliationBuilder reconciliationBuilder, [NotNull] ILogger logger)
        {
            if (reconciliationBuilder == null)
            {
                throw new ArgumentNullException(nameof(reconciliationBuilder));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.reconciliationBuilder = reconciliationBuilder;
            this.logger = logger;
        }

        public LedgerBook CreateNew()
        {
            return new LedgerBook(this.reconciliationBuilder);
        }
    }
}