using System;
using System.IO;
using System.Windows.Input;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Services;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser.LedgerBook
{
    public class LedgerBookControllerFileOperations
    {
        private readonly DemoFileHelper demoFileHelper;
        private readonly IUserMessageBox messageBox;
        private readonly Func<IUserPromptOpenFile> openFileDialogFactory;
        private readonly IUserQuestionBoxYesNo questionBox;
        private readonly Func<IUserPromptSaveFile> saveFileDialogFactory;

        public LedgerBookControllerFileOperations(
            [NotNull] UiContext uiContext,
            [NotNull] IMessenger messenger,
            [NotNull] DemoFileHelper demoFileHelper)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }
            if (messenger == null)
            {
                throw new ArgumentNullException("messenger");
            }
            if (demoFileHelper == null)
            {
                throw new ArgumentNullException("demoFileHelper");
            }

            this.saveFileDialogFactory = uiContext.UserPrompts.SaveFileFactory;
            this.openFileDialogFactory = uiContext.UserPrompts.OpenFileFactory;
            this.questionBox = uiContext.UserPrompts.YesNoBox;
            this.messageBox = uiContext.UserPrompts.MessageBox;

            this.demoFileHelper = demoFileHelper;
            MessengerInstance = messenger;

            ViewModel = new LedgerBookViewModel();
        }

        internal bool Dirty { get; set; }

        /// <summary>
        ///     Gets or sets the ledger service. Will be set by the <see cref="LedgerBookController" /> during its initialisation.
        /// </summary>
        internal ILedgerService LedgerService { get; set; }

        public IMessenger MessengerInstance { get; set; }

        public ICommand SaveLedgerBookCommand
        {
            get { return new RelayCommand(OnSaveLedgerBookCommandExecute, CanExecuteSaveCommand); }
        }

        internal LedgerBookViewModel ViewModel { get; set; }

        public void Close()
        {
            ViewModel.LedgerBook = null;
            MessengerInstance.Send(new LedgerBookReadyMessage(null));
        }

        internal void CheckIfSaveRequired()
        {
            if (Dirty)
            {
                bool? result = this.questionBox.Show("Save changes?", "Ledger Book");
                if (result != null && result.Value)
                {
                    SaveLedgerBook();
                }
            }
        }

        internal void SyncWithService()
        {
            ViewModel.LedgerBook = LedgerService.LedgerBook;
            MessengerInstance.Send(new LedgerBookReadyMessage(ViewModel.LedgerBook) { ForceUiRefresh = true });
        }

        internal void SaveLedgerBook()
        {
            try
            {
                LedgerService.Save(ViewModel.LedgerBook);
            }
            catch (ValidationWarningException ex)
            {
                this.messageBox.Show("Unable to save Ledger Book, some data is invalid\n" + ex.Message, "Ledger Book Validation");
                return;
            }

            Dirty = false;
            ViewModel.NewLedgerLine = null;
        }

        private bool CanExecuteCloseLedgerBookCommand()
        {
            // TODO Temporarily disabled while introducing ApplicationDatabaseService
            return ViewModel.LedgerBook != null;
        }

        private bool CanExecuteNewLedgerBookCommand()
        {
            // TODO Temporarily disabled while introducing ApplicationDatabaseService
            return ViewModel.LedgerBook == null;
        }

        private bool CanExecuteSaveCommand()
        {
            // TODO Temporarily disabled while introducing ApplicationDatabaseService
            return ViewModel.LedgerBook != null && Dirty;
        }

        //private void OnDemoLedgerBookCommandExecute()
        //{
        //    // TODO Temporarily disabled while introducing ApplicationDatabaseService
        //    try
        //    {
        //        SyncWithService(this.demoFileHelper.FindDemoFile(@"DemoLedgerBook.xml"));
        //    }
        //    catch (IOException)
        //    {
        //        this.messageBox.Show("Unable to find the demo Ledger-Book file.");
        //    }
        //}

        //private void OnLoadLedgerBookCommandExecute()
        //{
        //    // TODO Temporarily disabled while introducing ApplicationDatabaseService
        //    IUserPromptOpenFile openFileDialog = this.openFileDialogFactory();
        //    openFileDialog.AddExtension = true;
        //    openFileDialog.CheckPathExists = true;
        //    openFileDialog.DefaultExt = ".xml";
        //    openFileDialog.Filter = "LedgerBook files (*.xml, *.xaml)|*.xml;*.xaml|All files (*.*)|*.*";
        //    openFileDialog.Title = "Choose a LedgerBook xml file to load.";
        //    bool? result = openFileDialog.ShowDialog();
        //    if (result == null || !result.Value)
        //    {
        //        return;
        //    }
        //    string fileName = openFileDialog.FileName;

        //    SyncWithService(fileName);
        //}

        //private void OnNewLedgerBookCommandExecuted()
        //{
        //    // TODO Temporarily disabled while introducing ApplicationDatabaseService
        //    IUserPromptSaveFile saveFileDialog = this.saveFileDialogFactory();
        //    saveFileDialog.AddExtension = true;
        //    saveFileDialog.CheckPathExists = true;
        //    saveFileDialog.DefaultExt = ".xml";
        //    saveFileDialog.Filter = "LedgerBook files (*.xml, *.xaml)|*.xml;*.xaml|All files (*.*)|*.*";
        //    saveFileDialog.Title = "Choose a LedgerBook xml file name.";
        //    bool? result = saveFileDialog.ShowDialog();
        //    if (result == null || !result.Value)
        //    {
        //        return;
        //    }

        //    string fileName = saveFileDialog.FileName;
        //    if (string.IsNullOrWhiteSpace(fileName))
        //    {
        //        return;
        //    }

        //    Close();

        //    ViewModel.LedgerBook = LedgerService.CreateNew(fileName);
        //    Dirty = true;
        //    MessengerInstance.Send(new LedgerBookReadyMessage(ViewModel.LedgerBook));
        //}

        private void OnSaveLedgerBookCommandExecute()
        {
            // TODO Temporarily disabled while introducing ApplicationDatabaseService
            SaveLedgerBook();
        }
    }
}