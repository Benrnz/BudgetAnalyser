namespace BudgetAnalyser.Statement
{
    /// <summary>
    /// A dto object to persist statement data using the Application State mechanism.
    /// </summary>
    public class StatementApplicationState
    {
        public string FileName { get; set; }

        public bool? SortByBucket { get; set; }
    }
}
