using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Uwp.ApplicationState
{
    /// <summary>
    /// An interface for a mechanism to persist user data to some permenant storage. Typically an App would only have one implementation of this.
    /// </summary>
    public interface IPersistApplicationState
    {
        /// <summary>
        /// Load the user state from some persistent storage.
        /// </summary>
        /// <returns>An array of data objects that are self identifying. This array will need to be processed or broadcasted to the components that consume this data.</returns>
        Task<IEnumerable<IPersistentApplicationStateObject>> LoadAsync();

        /// <summary>
        /// Persist the user data to some persistent storage.
        /// </summary>
        /// <param name="modelsToPersist">All components in the App that implement <see cref="IPersistentApplicationStateObject"/> so the implementation can go get the data to persist.</param>
        Task PersistAsync(IEnumerable<IPersistentApplicationStateObject> modelsToPersist);
    }
}
