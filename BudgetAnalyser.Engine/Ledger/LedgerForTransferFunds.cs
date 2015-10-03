using BudgetAnalyser.Engine.BankAccount;

namespace BudgetAnalyser.Engine.Ledger
{
    public class LedgerForTransferFunds
    {
        public Account Account { get; set; }
        public string DisplayName { get; set; }

        public string Key { get; set; }
    }
}