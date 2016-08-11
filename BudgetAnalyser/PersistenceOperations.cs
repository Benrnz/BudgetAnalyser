using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetAnalyser.Annotations;
using BudgetAnalyser.Dashboard;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Services;

namespace BudgetAnalyser
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class PersistenceOperations
    {
        private readonly IApplicationDatabaseService applicationDatabaseService;
        private readonly DemoFileHelper demoFileHelper;
        private readonly IUiContext uiContext;

        public PersistenceOperations(
            [NotNull] IApplicationDatabaseService applicationDatabaseService,
            [NotNull] DemoFileHelper demoFileHelper,
            [NotNull] IUiContext uiContext)
        {
            if (applicationDatabaseService == null)
            {
                throw new ArgumentNullException(nameof(applicationDatabaseService));
            }

            if (demoFileHelper == null)
            {
                throw new ArgumentNullException(nameof(demoFileHelper));
            }

            if (uiContext == null)
            {
                throw new ArgumentNullException(nameof(uiContext));
            }

            this.uiContext = uiContext;
            this.applicationDatabaseService = applicationDatabaseService;
            this.demoFileHelper = demoFileHelper;

            this.uiContext.Messenger.Register<PasswordSetMessage>(this, OnPasswordSetMessageReceived);
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
            bool? response = fileDialog.ShowDialog();
            if (response == null || response == false || fileDialog.FileName.IsNothing())
            {
                return;
            }
            var fileName = fileDialog.FileName;

            var appDb = this.applicationDatabaseService.Close();
            appDb = await this.applicationDatabaseService.CreateNewDatabaseAsync(fileName);
        }

        public async void OnLoadDatabaseCommandExecute()
        {
            await LoadDatabase(PromptUserForFileName);
        }

        public async void OnLoadDemoDatabaseCommandExecute()
        {
            var fileName = this.demoFileHelper.FindDemoFile();
            await LoadDatabase(() => fileName);
        }

        public async void OnSaveDatabaseCommandExecute()
        {
            if (!this.applicationDatabaseService.HasUnsavedChanges)
            {
                return;
            }

            await SaveDatabase();
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
                this.uiContext.Logger.LogWarning(l => $"Attempt to open an encrypted file {fileName} with an incorrect password.");
                this.uiContext.EncryptFileController.ShowEnterPasswordDialog(fileName, "Incorrect password");
            }
            catch (KeyNotFoundException)
            {
                this.uiContext.UserPrompts.MessageBox.Show("Budget Analyser", "The previously loaded Budget Analyser file ({0}) no longer exists.", fileName);
                this.uiContext.Logger.LogWarning(l => $"The previously loaded Budget Analyser file ({fileName}) no longer exists.");
            }
            catch (EncryptionKeyNotProvidedException)
            {
                // Recover by prompting user for the password.
                this.uiContext.Logger.LogWarning(l => "Attempt to open an encrypted file with no password set. (Ok if only happens once).");
                this.uiContext.EncryptFileController.ShowEnterPasswordDialog(fileName);
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
                bool? response = this.uiContext.UserPrompts.YesNoBox.Show("Save changes before loading a different file?", messageBoxHeading);
                if (response != null && response.Value)
                {
                    return await SaveDatabase(messageBoxHeading);
                }

                // User Cancelled.
                return false;
            }

            return true;
        }

        private string PromptUserForFileName()
        {
            var openDialog = this.uiContext.UserPrompts.OpenFileFactory();
            openDialog.CheckFileExists = true;
            openDialog.AddExtension = true;
            openDialog.DefaultExt = "*.bax";
            openDialog.Filter = "Budget Analyser files (*.bax)|*.bax|Xml files (*.xml, *.xaml)|*.xml;*.xaml";
            openDialog.Title = "Select Budget Analyser file to open";
            bool? response = openDialog.ShowDialog();
            if (response == null || response.Value == false)
            {
                return null;
            }

            return openDialog.FileName;
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
}