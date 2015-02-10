namespace BudgetAnalyser.Engine
{
    /// <summary>
    ///     The stored application state for the Main view.
    ///     This is saved when the application exits.
    /// </summary>
    public class MainApplicationStateModel
    {
        /// <summary>
        ///     Gets or sets a string that indicates where the budget analyser stores its data.
        /// </summary>
        public string BudgetAnalyserDataStorage { get; set; }
    }
}