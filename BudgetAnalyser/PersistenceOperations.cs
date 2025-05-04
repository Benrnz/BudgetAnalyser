using System.Text;
using System.Windows.Input;
using BudgetAnalyser.Dashboard;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Services;
using CommunityToolkit.Mvvm.Messaging;

namespace BudgetAnalyser;

[AutoRegisterWithIoC(SingleInstance = true)]
public class PersistenceOperations
{
    private static readonly SemaphoreSlim OneSaveAtATime = new(1, 1);

    private readonly IApplicationDatabaseFacade applicationDatabaseService;
    private readonly DemoFileHelper demoFileHelper;
    private readonly IUiContext uiContext;
    private DateTime lastSave = DateTime.Now;

    public PersistenceOperations(
        IApplicationDatabaseFacade applicationDatabaseService,
        DemoFileHelper demoFileHelper,
        IUiContext uiContext)
    {
        this.uiContext = uiContext ?? throw new ArgumentNullException(nameof(uiContext));
        this.applicationDatabaseService = applicationDatabaseService ?? throw new ArgumentNullException(nameof(applicationDatabaseService));
        this.demoFileHelper = demoFileHelper ?? throw new ArgumentNullException(nameof(demoFileHelper));

        this.uiContext.Messenger.Register<PersistenceOperations, PasswordSetMessage>(this, static (r, m) => r.OnPasswordSetMessageReceived(m));
    }

    public bool HasUnsavedChanges => this.applicationDatabaseService.HasUnsavedChanges;

    public async Task LoadDatabase(string fileName)
    {
        await LoadDatabase(() => fileName);
    }

    public async void OnCreateNewDatabaseCommandExecute()
    {
        if (!await PromptToSaveIfNecessary())
        {
            // User cancelled or model is invalid and preventing save.
            return;
        }

        var fileDialog = this.uiContext.UserPrompts.SaveFileFactory();
        fileDialog.CheckPathExists = true;
        fileDialog.DefaultExt = "*.bax";
        fileDialog.AddExtension = true;
        fileDialog.Filter = "Budget Analyser files (*.bax)|*.bax";
        fileDialog.Title = "Select a folder and filename";
        var response = fileDialog.ShowDialog();
        if (response is null || response == false || fileDialog.FileName.IsNothing())
        {
            return;
        }

        var fileName = fileDialog.FileName;

        this.applicationDatabaseService.Close();
        await this.applicationDatabaseService.CreateNewDatabaseAsync(fileName);
    }

    public async void OnLoadDatabaseCommandExecute()
    {
        var fileName = PromptUserForFileName();
        if (fileName is null)
        {
            return;
        }

        await LoadDatabase(fileName);
    }

    public async void OnLoadDemoDatabaseCommandExecute()
    {
        var fileName = this.demoFileHelper.FindDemoFile();
        await LoadDatabase(() => fileName);
    }

    public async void OnSaveDatabaseCommandExecute()
    {
        // This is to stop excessive clicking on the save button crashing the app due to locked file from many threads trying to save at the same time.
        await OneSaveAtATime.WaitAsync();
        try
        {
            if (!this.applicationDatabaseService.HasUnsavedChanges)
            {
                return;
            }

            if (DateTime.Now.Subtract(this.lastSave).TotalSeconds < 2)
            {
                return; // No need to save repeatedly.
            }

            await SaveDatabase();
            this.lastSave = DateTime.Now;
        }
        finally
        {
            OneSaveAtATime.Release();
        }
    }

    public void OnValidateModelsCommandExecute()
    {
        ValidateModel("Validate Budget Analyser Data");
    }

    public MainApplicationState PreparePersistentStateData()
    {
        return this.applicationDatabaseService.PreparePersistentStateData();
    }

    public async Task SaveDatabase()
    {
        await SaveDatabase("Save Budget Analyser Data");
        CommandManager.InvalidateRequerySuggested();
    }

    private async Task LoadDatabase(Func<string> getFileName)
    {
        if (!await PromptToSaveIfNecessary())
        {
            // User cancelled or model is invalid and preventing save.
            return;
        }

        var fileName = getFileName();
        if (string.IsNullOrWhiteSpace(fileName))
        {
            // User cancelled.
            return;
        }

        this.applicationDatabaseService.Close();

        try
        {
            await this.applicationDatabaseService.LoadAsync(fileName);
        }
        catch (EncryptionKeyIncorrectException)
        {
            // Recover by prompting user for the password.
            this.uiContext.Logger.LogWarning(_ => $"Attempt to open an encrypted file {fileName} with an incorrect password.");
            this.uiContext.Controller<EncryptFileController>().ShowEnterPasswordDialog(fileName, "Incorrect password");
        }
        catch (KeyNotFoundException)
        {
            this.uiContext.UserPrompts.MessageBox.Show("Budget Analyser", "The previously loaded Budget Analyser file ({0}) no longer exists.", fileName);
            this.uiContext.Logger.LogWarning(_ => $"The previously loaded Budget Analyser file ({fileName}) no longer exists.");
        }
        catch (EncryptionKeyNotProvidedException)
        {
            // Recover by prompting user for the password.
            this.uiContext.Logger.LogWarning(_ => "Attempt to open an encrypted file with no password set. (Ok if only happens once).");
            this.uiContext.Controller<EncryptFileController>().ShowEnterPasswordDialog(fileName);
        }
    }

    private async void OnPasswordSetMessageReceived(PasswordSetMessage message)
    {
        // Reload the database file - treat it as a normal file open scenario.
        await LoadDatabase(message.DatabaseStorageKey);
    }

    private async Task<bool> PromptToSaveIfNecessary()
    {
        if (this.applicationDatabaseService.HasUnsavedChanges)
        {
            const string messageBoxHeading = "Open Budget Analyser File";
            var response = this.uiContext.UserPrompts.YesNoBox.Show("Save changes before loading a different file?", messageBoxHeading);
            if (response is not null && response.Value)
            {
                return await SaveDatabase(messageBoxHeading);
            }

            // User Cancelled.
            return false;
        }

        return true;
    }

    private string? PromptUserForFileName()
    {
        var openDialog = this.uiContext.UserPrompts.OpenFileFactory();
        openDialog.CheckFileExists = true;
        openDialog.AddExtension = true;
        openDialog.DefaultExt = "*.bax";
        openDialog.Filter = "Budget Analyser files (*.bax)|*.bax|Xml files (*.xml, *.xaml)|*.xml;*.xaml";
        openDialog.Title = "Select Budget Analyser file to open";
        var response = openDialog.ShowDialog();
        return response is null || response.Value == false ? null : openDialog.FileName;
    }

    private async Task<bool> SaveDatabase(string title)
    {
        if (ValidateModel(title))
        {
            await this.applicationDatabaseService.SaveAsync();
            return true;
        }

        return false;
    }

    private bool ValidateModel(string title)
    {
        var messages = new StringBuilder();
        if (this.applicationDatabaseService.ValidateAll(messages))
        {
            return true;
        }

        this.uiContext.UserPrompts.MessageBox.Show("Can't continue, some data is invalid, see messages below:\n" + messages, title);
        return false;
    }
}
