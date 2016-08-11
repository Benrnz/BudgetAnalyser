namespace BudgetAnalyser.Engine
{
    /// <summary>
    ///     A major category of data for application data. (Not application state aka user preferences).
    /// </summary>
    public enum ApplicationDataType
    {
        /// <summary>
        ///     Identifies a category of data within the application as "Budget".
        /// </summary>
        Budget,

        /// <summary>
        ///     Identifies a category of data within the application as "Ledger".
        /// </summary>
        Ledger,

        /// <summary>
        ///     Identifies a category of data within the application as "Transactions".
        /// </summary>
        Transactions,

        /// <summary>
        ///     Identifies a category of data within the application as "MatchingRules".
        /// </summary>
        MatchingRules,

        /// <summary>
        ///     Identifies a category of data within the application as "Tasks".
        /// </summary>
        Tasks,
    }
}