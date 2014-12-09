namespace BudgetAnalyser.Engine.Statement
{
    /// <summary>
    /// A dto object to persist statement data using the Application State mechanism.
    /// </summary>
    public class StatementApplicationState
    {
        public string StorageKey { get; set; }

        public bool? SortByBucket { get; set; }
    }
}
