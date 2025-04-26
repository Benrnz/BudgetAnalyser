using System.Text;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Services;

namespace BudgetAnalyser;

/// <summary>
///     An interface to represent a clientside facade to marshall all access to the <see cref="IApplicationDatabaseService" />.  This is to allow clientside code to be notified of changing data and send
///     messages between controllers.
/// </summary>
public interface IApplicationDatabaseFacade
{
    // NOTE This interface could inherit from IApplicationDatabaseService, but for simplicity of IOC registration, it does not. Also, not necessarily a bad thing because it forces local code to use
    // this facade and not accidentally use the service directly.

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
    ApplicationDatabase? Close();

    /// <summary>
    ///     Creates a new application database storage.
    /// </summary>
    /// <param name="storageKey">The storage key.</param>
    Task<ApplicationDatabase> CreateNewDatabaseAsync(string storageKey);

    /// <summary>
    ///     Decrypt the underlying data files.
    /// </summary>
    /// <param name="confirmCredentialsClaim">A duplicate credential must be provided to ensure the user can decrypt their files.</param>
    /// <exception cref="EncryptionKeyNotProvidedException">Will be thrown if the data files are encrypted and no credentials are provided. See <see cref="ICredentialStore" /> to provide credentials.</exception>
    Task DecryptFilesAsync(object confirmCredentialsClaim);

    /// <summary>
    ///     Encrypts the underlying data files.
    /// </summary>
    /// <exception cref="EncryptionKeyNotProvidedException">
    ///     Will be thrown if the data files are encrypted and no credentials
    ///     are provided. See <see cref="ICredentialStore" /> to provide credentials.
    /// </exception>
    Task EncryptFilesAsync();

    /// <summary>
    ///     Loads the specified Budget Analyser file by file name. This will also trigger a load on all subordinate
    ///     data contained within and referenced by the top level application database.
    ///     No warning will be given if there is any unsaved data. This should be checked before calling this method.
    /// </summary>
    /// <param name="storageKey">Name and path to the file.</param>
    /// <exception cref="ArgumentNullException">Will be thrown if <paramref name="storageKey" /> is null or empty.</exception>
    /// <exception cref="EncryptionKeyNotProvidedException">
    ///     Will be thrown if the data files are encrypted and no credentials
    ///     are provided. See <see cref="ICredentialStore" /> to provide credentials.
    /// </exception>
    /// <exception cref="EncryptionKeyIncorrectException">
    ///     Will be thrown if the data files are encrypted and the credentials
    ///     supplied are incorrect.
    /// </exception>
    /// <exception cref="DataFormatException">
    ///     Will be thrown if an underlying data file is in an incorrect format. Can occur if
    ///     file format is out of date and no longer supported.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    ///     Will be thrown if the specified storage cannot be found based on the
    ///     <paramref name="storageKey" /> provided.
    /// </exception>
    /// <exception cref="NotSupportedException ">
    ///     Will be thrown if an underlying data file is completely the wrong format. Most
    ///     likely not a Budget Analyser file.
    /// </exception>
    Task<ApplicationDatabase> LoadAsync(string storageKey);

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
    ///     Sets the credential that will be used when underlying data files are encrypted.
    ///     If data files are not encrypted this does not need to be called.
    ///     The claim can be removed by calling with a null claim value. Only one claim is stored.
    /// </summary>
    /// <param name="claim">The password claim.</param>
    void SetCredential(object claim);

    /// <summary>
    ///     Validates all models in the application database.
    ///     This method does not use <see cref="ValidationWarningException" /> to return any validation messages.
    /// </summary>
    /// <param name="messages">Will append any validation messages found.</param>
    /// <returns>True if valid, otherwise false.</returns>
    bool ValidateAll(StringBuilder messages);
}

[AutoRegisterWithIoC(SingleInstance = true)]
internal class WpfApplicationDatabaseFacade(IApplicationDatabaseService engineService) : IApplicationDatabaseFacade
{
    public bool HasUnsavedChanges => engineService.HasUnsavedChanges;
    public bool IsEncrypted => engineService.IsEncrypted;

    public ApplicationDatabase? Close()
    {
        var result = engineService.Close();
        PersistenceOperationCommands.SaveDatabaseCommand.NotifyCanExecuteChanged();
        // Send message here?
        return result;
    }

    public async Task<ApplicationDatabase> CreateNewDatabaseAsync(string storageKey)
    {
        var result = await engineService.CreateNewDatabaseAsync(storageKey);
        PersistenceOperationCommands.SaveDatabaseCommand.NotifyCanExecuteChanged();
        return result;
    }

    public async Task DecryptFilesAsync(object confirmCredentialsClaim)
    {
        await engineService.DecryptFilesAsync(confirmCredentialsClaim);
    }

    public async Task EncryptFilesAsync()
    {
        await engineService.EncryptFilesAsync();
    }

    public async Task<ApplicationDatabase> LoadAsync(string storageKey)
    {
        var result = await engineService.LoadAsync(storageKey);
        PersistenceOperationCommands.SaveDatabaseCommand.NotifyCanExecuteChanged();
        return result;
    }

    public void NotifyOfChange(ApplicationDataType dataType)
    {
        engineService.NotifyOfChange(dataType);
        PersistenceOperationCommands.SaveDatabaseCommand.NotifyCanExecuteChanged();
    }

    public MainApplicationState PreparePersistentStateData()
    {
        return engineService.PreparePersistentStateData();
    }

    public async Task SaveAsync()
    {
        await engineService.SaveAsync();
        PersistenceOperationCommands.SaveDatabaseCommand.NotifyCanExecuteChanged();
    }

    public void SetCredential(object claim)
    {
        engineService.SetCredential(claim);
    }

    public bool ValidateAll(StringBuilder messages)
    {
        return engineService.ValidateAll(messages);
    }
}
