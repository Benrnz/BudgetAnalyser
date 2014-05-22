using BudgetAnalyser.Engine.Account;

namespace BudgetAnalyser.Engine.Ledger
{
    public class BankBalance
    {
        public AccountType Account { get; set; }

        public decimal Balance { get; set; }
    }
}
