namespace BudgetAnalyser.Engine.BankAccount
{
    /// <summary>
    ///     Represents a Chequing or transaction bank account. This is used as the main bank account.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.BankAccount.Account" />
    public class ChequeAccount : Account
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ChequeAccount" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public ChequeAccount(string name)
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
        public override string ImagePath => "ChequeLogoImage";

        // TODO If multiple cheque (or multiple non-savings) accounts are ever allowed, this may need to be more robust.
        /// <summary>
        ///     Gets a value indicating whether this account is an account where salaries are credited into.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is salary account; otherwise, <c>false</c>.
        /// </value>
        public override bool IsSalaryAccount => true;

        internal virtual string[] KeyWords => new[] {"CHEQUE", "CHECK"};

        /// <summary>
        ///     Clones this instance and give the new clone the specified name.
        /// </summary>
        public virtual Account Clone(string name)
        {
            return new ChequeAccount(name);
        }
    }
}