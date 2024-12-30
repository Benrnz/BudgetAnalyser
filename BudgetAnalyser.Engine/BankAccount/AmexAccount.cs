namespace BudgetAnalyser.Engine.BankAccount
{
    /// <summary>
    ///     Represents an Amex credit card bank account.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.BankAccount.Account" />
    public class AmexAccount : Account
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AmexAccount" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public AmexAccount(string name)
        {
            Name = name;
        }

        /// <summary>
        ///     Gets the type or classification of the account.
        /// </summary>
        public override AccountType AccountType => AccountType.CreditCard;

        /// <summary>
        ///     Gets the path to an image asset.
        /// </summary>
        public override string ImagePath => "AmexLogoImage";

        internal virtual string[] KeyWords => new[] { "AMEX" };

        /// <summary>
        ///     Clones this instance and give the new clone the specified name.
        /// </summary>
        public virtual Account Clone(string name)
        {
            return new AmexAccount(name);
        }
    }
}
