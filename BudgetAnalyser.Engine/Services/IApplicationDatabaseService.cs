using System.Text;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Persistence;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Services
{
    /// <summary>
    ///     A service class to access and manage the main Budget Analyser Data File.
    /// </summary>
    public interface IApplicationDatabaseService : IServiceFoundation
    {
        /// <summary>
        ///     Gets or sets a value indicating whether there are unsaved changes across all application data.
        /// </summary>
        bool HasUnsavedChanges { get; }

        /// <summary>
        ///     Gets a value indicating whether the underlying data files are encrypted.
        /// </summary>
        bool IsEncrypted { get; }

        /// <summary>
        ///     Closes the currently loaded Budget Analyser file, and therefore any other application data is also closed.
        ///     Changes are discarded, no prompt or error will occur if there are unsaved changes. This check should be done before
        ///     calling this method.
        /// </summary>
        ApplicationDatabase Close();

        /// <summary>
        ///     Creates a new application database storage.
        /// </summary>
        /// <param name="storageKey">The storage key.</param>
        Task<ApplicationDatabase> CreateNewDatabaseAsync([NotNull] string storageKey);

        /// <summary>
        ///     Encrypts or Decrypts the underlying data files asynchronously.
        /// </summary>
        Task EncryptFilesAsync();

        /// <summary>
        ///     Loads the specified Budget Analyser file by file name. This will also trigger a load on all subordinate
        ///     data contained within and referenced by the top level application database.
        ///     No warning will be given if there is any unsaved data. This should be checked before calling this method.
        /// </summary>
        /// <param name="storageKey">Name and path to the file.</param>
        Task<ApplicationDatabase> LoadAsync([NotNull] string storageKey);

        /// <summary>
        ///     Notifies the service that data has changed and will need to be saved.
        /// </summary>
        void NotifyOfChange(ApplicationDataType dataType);

        /// <summary>
        ///     Prepares the persistent data for saving into permenant storage.
        /// </summary>
        MainApplicationState PreparePersistentStateData();

        /// <summary>
        ///     Saves all Budget Analyser application data.
        /// </summary>
        /// <exception cref="ValidationWarningException">Will be thrown if there are any validation errors.</exception>
        Task SaveAsync();

        /// <summary>
        ///     Sets the password claim. This is required if underlying data files are encrypted.
        ///     Otherwise it does not need to be called.
        ///     A password claim can be removed by calling with a null claim value.
        /// </summary>
        /// <param name="passwordClaim">The password claim.</param>
        void SetPassword(object passwordClaim);

        /// <summary>
        ///     Validates all models in the application database.
        ///     This method does not use <see cref="ValidationWarningException" /> to return any validation messages.
        /// </summary>
        /// <param name="messages">Will append any validation messages found.</param>
        /// <returns>True if valid, otherwise false.</returns>
        bool ValidateAll(StringBuilder messages);
    }
}