using System;

namespace BudgetAnalyser.LedgerBook
{
    public class LedgerTransactionEventArgs : EventArgs
    {
        public LedgerTransactionEventArgs(bool wasChanged)
        {
            WasModified = wasChanged;
        }

        public bool WasModified { get; private set; }
    }
}
