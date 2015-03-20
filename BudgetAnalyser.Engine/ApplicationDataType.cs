namespace BudgetAnalyser.Engine
{
    /// <summary>
    /// A major category of data for application data. (Not application state aka user preferences).
    /// </summary>
    public enum ApplicationDataType
    {
        Budget,
        Ledger,
        Transactions,
        MatchingRules,
        Tasks,
    }
}