namespace Rees.Wpf
{
    /// <summary>
    /// A utility interface to mark a controller as requiring some initilisation before use.
    /// This is useful when a controller needs to do some preparation work before the first usage, 
    /// and its inappropriate to put the initialisation in the constructor.
    /// </summary>
    public interface IInitializableController
    {
        /// <summary>
        /// Initialise the controller to prepare it for use
        /// </summary>
        void Initialize();
    }
}