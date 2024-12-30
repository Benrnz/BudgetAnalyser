namespace BudgetAnalyser.Engine.BankAccount
{
    /// <summary>
    ///     Represents a miscellaneous or general use account that isn't a <see cref="ChequeAccount" />
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.BankAccount.Account" />
    public class MiscellaneousAccount : Account
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MiscellaneousAccount" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public MiscellaneousAccount(string name)
        {
            Name = name;
        }

        /// <summary>
        ///     Gets the type or classification of the account.
        /// </summary>
        public override AccountType AccountType => AccountType.General;

        /// <summary>
        ///     Gets the path to an image asset.
        /// </summary>
        public override string ImagePath => "../Assets/Misc1Logo.png";

        internal virtual string[] KeyWords => new string[] { };

        /// <summary>
        ///     Clones this instance and give the new clone the specified name.
        /// </summary>
        public virtual Account Clone(string name)
        {
            return new MiscellaneousAccount(name);
        }
    }
}
