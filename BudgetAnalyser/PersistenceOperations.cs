﻿using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Annotations;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Services;
using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser
{
    [AutoRegisterWithIoC]
    public class PersistenceOperations
    {
        private readonly IApplicationDatabaseService applicationDatabaseService;
        private readonly DemoFileHelper demoFileHelper;
        private readonly UserPrompts userPrompts;

        public PersistenceOperations(
            [NotNull] IApplicationDatabaseService applicationDatabaseService,
            [NotNull] DemoFileHelper demoFileHelper,
            [NotNull] UserPrompts userPrompts)
        {
            if (applicationDatabaseService == null)
            {
                throw new ArgumentNullException(nameof(applicationDatabaseService));
            }

            if (demoFileHelper == null)
            {
                throw new ArgumentNullException(nameof(demoFileHelper));
            }

            if (userPrompts == null)
            {
                throw new ArgumentNullException(nameof(userPrompts));
            }

            this.applicationDatabaseService = applicationDatabaseService;
            this.demoFileHelper = demoFileHelper;
            this.userPrompts = userPrompts;
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

            IUserPromptSaveFile fileDialog = this.userPrompts.SaveFileFactory();
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
            string fileName = fileDialog.FileName;

            ApplicationDatabase appDb = this.applicationDatabaseService.Close();
            appDb = await this.applicationDatabaseService.CreateNewDatabaseAsync(fileName);
        }

        public async void OnLoadDatabaseCommandExecute()
        {
            await LoadDatabase(PromptUserForFileName);
        }

        public async void OnLoadDemoDatabaseCommandExecute()
        {
            string fileName = this.demoFileHelper.FindDemoFile();
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

            string fileName = getFileName();
            if (string.IsNullOrWhiteSpace(fileName))
            {
                // User cancelled.
                return;
            }

            ApplicationDatabase appDb = this.applicationDatabaseService.Close();
            appDb = await this.applicationDatabaseService.LoadAsync(fileName);
        }

        private async Task<bool> PromptToSaveIfNecessary()
        {
            if (this.applicationDatabaseService.HasUnsavedChanges)
            {
                const string messageBoxHeading = "Open Budget Analyser File";
                bool? response = this.userPrompts.YesNoBox.Show("Save changes before loading a different file?", messageBoxHeading);
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
            IUserPromptOpenFile openDialog = this.userPrompts.OpenFileFactory();
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

            this.userPrompts.MessageBox.Show("Can't continue, some data is invalid, see messages below:\n" + messages, title);
            return false;
        }
    }
}