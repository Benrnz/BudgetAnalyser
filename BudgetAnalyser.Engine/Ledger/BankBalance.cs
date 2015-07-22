namespace BudgetAnalyser.Engine.Ledger
{
    public class BankBalance
    {
        public BankBalance(Account.Account account, decimal balance)
        {
            Account = account;
            Balance = balance;
        }

        public Account.Account Account { get; private set; }

        public decimal Balance { get; private set; }
    }
}
