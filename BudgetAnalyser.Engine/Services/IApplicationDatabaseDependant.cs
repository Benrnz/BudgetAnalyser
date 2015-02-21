namespace BudgetAnalyser.Engine.Services
{
    /// <summary>
    /// An interface to allow the Application Database master service to communicate with subordinate database dependant services.
    /// </summary>
    public interface IApplicationDatabaseDependant
    {
        /// <summary>
        ///     Closes the currently loaded file.  No warnings will be raised if there is unsaved data.
        /// </summary>
        void Close();
    }
}