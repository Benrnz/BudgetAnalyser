namespace BudgetAnalyser.Engine.BankAccount
{
    /// <summary>
    ///     An enum to describe the account type or purpose of the account.
    ///     Different account types may have different behaviour or be shown differently in the UI.
    /// </summary>
    public enum AccountType
    {
        /// <summary>
        ///     A general or common account. Ie: Cheque or Transaction account.
        /// </summary>
        General,

        /// <summary>
        ///     A credit card account. Ie: Visa, Mastercard etc
        /// </summary>
        CreditCard,

        /// <summary>
        ///     A loan account that tracks an amount owing.
        /// </summary>
        Loan,

        /// <summary>
        ///     A savings account such as a high interest account.
        /// </summary>
        Savings
    }
}