namespace BudgetAnalyser.Engine.BankAccount
{
    /// <summary>
    ///     Represents a Visa credit card account. Current limitation is you can only have one Visa account.
    /// </summary>
    public class VisaAccount : Account
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VisaAccount"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public VisaAccount(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the type or classification of the account.
        /// </summary>
        public override AccountType AccountType => AccountType.CreditCard;
        /// <summary>
        /// Gets the path to an image asset.
        /// </summary>
        public override string ImagePath => "VisaLogoImage";
        internal virtual string[] KeyWords => new[] { "VISA" };

        /// <summary>
        /// Clones this instance and give the new clone the specified name.
        /// </summary>
        public virtual Account Clone(string name)
        {
            return new VisaAccount(name);
        }
    }
}