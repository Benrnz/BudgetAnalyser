namespace BudgetAnalyser.Engine.Persistence
{
    /// <summary>
    ///     A Dto object that points to another source location for a persisted object.
    /// </summary>
    public class StorageBranch
    {
        /// <summary>
        ///     Gets or sets the source text that uniquely identifies the source object.
        ///     For example: a path and file name or a database key.
        /// </summary>
        public string Source { get; set; }
    }
}
