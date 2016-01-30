namespace BudgetAnalyser.Uwp
{
    /// <summary>
    /// An interface that a controller can optionally implement to indicate it needs to do some initialisation after instantiation and before first use and any data is loaded.
    /// </summary>
    public interface IInitialisableController
    {
        /// <summary>
        /// Initialises this instance.
        /// </summary>
        void Initialise();
    }
}