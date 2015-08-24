using BudgetAnalyser.Engine.BankAccount;

namespace BudgetAnalyser.Engine.Ledger
{
    public class BankBalance
    {
        public BankBalance(Account account, decimal balance)
        {
            Account = account;
            Balance = balance;
        }

        public Account Account { get; private set; }
        public decimal Balance { get; private set; }
    }
}