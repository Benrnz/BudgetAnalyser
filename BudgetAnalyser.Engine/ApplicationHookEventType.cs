namespace BudgetAnalyser.Engine
{
    /// <summary>
    /// A major classification for application events.
    /// </summary>
    public enum ApplicationHookEventType
    {
        /// <summary>
        /// The event is a repository event, it relates to loading and saving of data.
        /// </summary>
        Repository,

        /// <summary>
        /// The event is an application wide event, ie, start-up, shutdown, or unhandled-error.
        /// </summary>
        Application,
    }
}