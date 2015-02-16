using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.Services
{
    /// <summary>
    ///     A service class to access and manage the main Budget Analyser Data File.
    /// </summary>
    public interface IApplicationDatabaseService : IServiceFoundation
    {
        /// <summary>
        ///     Initialises the top level Application Database object that contains all information to retrieve data
        ///     for all other Budget Analyser datum.
        ///     This must be called first before other methods of this service can be used.
        /// </summary>
        ApplicationDatabase LoadPersistedStateData([NotNull] MainApplicationStateModelV1 storedState);

        /// <summary>
        ///     Prepares the persistent data for saving into permenant storage.
        /// </summary>
        MainApplicationStateModelV1 PreparePersistentStateData();
    }
}