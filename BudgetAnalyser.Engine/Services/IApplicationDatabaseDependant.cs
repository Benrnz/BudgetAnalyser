using System.Threading.Tasks;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.Services
{
    /// <summary>
    ///     An interface to allow the Application Database master service to communicate with subordinate database dependant
    ///     services. Only <see cref="IServiceFoundation"/> implementations should implement this.
    /// </summary>
    public interface IApplicationDatabaseDependant
    {
        /// <summary>
        /// Gets the initialisation sequence number. Set this to a low number for important data that needs to be loaded first.
        /// Defaults to 50.
        /// </summary>
        int Sequence { get; }

        /// <summary>
        ///     Closes the currently loaded file.  No warnings will be raised if there is unsaved data.
        /// </summary>
        void Close();

        /// <summary>
        ///     Loads a data source with the provided database reference data asynchronously.
        /// </summary>
        Task LoadAsync(ApplicationDatabase applicationDatabase);
    }
}