using BudgetAnalyser.Engine.BankAccount;

namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    ///     A bank balance for an account.
    /// </summary>
    public class BankBalance
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BankBalance" /> class.
        /// </summary>
        public BankBalance(Account account, decimal balance)
        {
            Account = account;
            Balance = balance;
        }

        /// <summary>
        ///     Gets the account.
        /// </summary>
        public Account Account { get; private set; }

        /// <summary>
        ///     Gets the balance.
        /// </summary>
        public decimal Balance { get; private set; }
    }
}