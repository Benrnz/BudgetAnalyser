using System;
using System.Collections.Generic;
using System.Linq;

namespace BudgetAnalyser.Engine.Ledger.Reconciliation
{
    [AutoRegisterWithIoC]
    internal class ReconciliationBehaviourForLedgerBucket : IReconciliationBehaviour
    {
        public LedgerEntryLine NewReconLine { get; private set; }

        public void Initialise(params object[] anyParameters)
        {
            foreach (var argument in anyParameters)
            {
                NewReconLine = NewReconLine ?? argument as LedgerEntryLine;
            }

            if (NewReconLine == null)
            {
                throw new ArgumentNullException(nameof(NewReconLine));
            }
        }

        public void ApplyBehaviour()
        {
            foreach (var ledger in NewReconLine.Entries)
            {
                List<LedgerTransaction> transactions = ledger.Transactions.ToList();
                if (ledger.LedgerBucket.ApplyReconciliationBehaviour(transactions, NewReconLine.Date, ledger.Balance))
                {
                    ledger.SetTransactionsForReconciliation(transactions);
                }
            }
        }
    }
}