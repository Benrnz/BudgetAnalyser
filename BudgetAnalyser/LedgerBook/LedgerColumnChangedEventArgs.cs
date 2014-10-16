using System;
using BudgetAnalyser.Engine.Ledger;

namespace BudgetAnalyser.LedgerBook
{
    public class LedgerColumnChangedEventArgs : EventArgs
    {
        public LedgerColumnChangedEventArgs(LedgerColumn ledger)
        {
            Ledger = ledger;
        }

        public LedgerColumn Ledger { get; private set; }
    }
}
