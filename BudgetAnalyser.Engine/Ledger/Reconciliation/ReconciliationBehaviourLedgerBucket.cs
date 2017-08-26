using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Ledger.Reconciliation
{
    [AutoRegisterWithIoC]
    internal class ReconciliationBehaviourForLedgerBucket : IReconciliationBehaviour
    {
        public LedgerEntryLine NewReconLine { get; private set; }

        public void Dispose()
        {
            NewReconLine = null;
        }

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
                var transactions = ledger.Transactions.ToList();
                if (ledger.LedgerBucket.ApplyReconciliationBehaviour(transactions, NewReconLine.Date, ledger.Balance))
                {
                    ledger.SetTransactionsForReconciliation(transactions);
                }
            }
        }
    }
}