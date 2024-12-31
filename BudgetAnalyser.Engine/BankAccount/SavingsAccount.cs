namespace BudgetAnalyser.Engine.BankAccount
{
    /// <summary>
    ///     Represents a Savings account ot high interest bank account.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.BankAccount.Account" />
    public class SavingsAccount : Account
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SavingsAccount" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public SavingsAccount(string name)
        {
            Name = name;
        }

        /// <summary>
        ///     Gets the type or classification of the account.
        /// </summary>
        public override AccountType AccountType => AccountType.Savings;

        /// <summary>
        ///     Gets the path to an image asset.
        /// </summary>
        public override string ImagePath => "SavingsLogoImage";

        internal virtual string[] KeyWords => new[] { "SAVINGS", "SAVE", "DEPOSIT", "TERM DEPOSIT", "ONCALL", "ON CALL" };

        /// <summary>
        ///     Clones the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public virtual Account Clone(string name)
        {
            return new SavingsAccount(name);
        }
    }
}
