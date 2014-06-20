using System;

namespace BudgetAnalyser.LedgerBook
{
    public class EditBankBalancesEventArgs : EventArgs
    {
        public bool Canceled { get; set; }
    }
}