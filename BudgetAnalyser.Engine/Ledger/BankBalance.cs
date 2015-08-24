namespace BudgetAnalyser.Engine.Ledger
{
    public class BankBalance
    {
        public BankBalance(BankAccount.Account account, decimal balance)
        {
            Account = account;
            Balance = balance;
        }

        public BankAccount.Account Account { get; private set; }
        public decimal Balance { get; private set; }
    }
}