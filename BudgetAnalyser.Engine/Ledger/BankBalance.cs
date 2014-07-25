using BudgetAnalyser.Engine.Account;

namespace BudgetAnalyser.Engine.Ledger
{
    public class BankBalance
    {
        public BankBalance(AccountType account, decimal balance)
        {
            Account = account;
            Balance = balance;
        }

        public AccountType Account { get; private set; }

        public decimal Balance { get; private set; }
    }
}
