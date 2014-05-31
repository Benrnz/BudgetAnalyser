using System;
using System.IO;
using System.Windows.Input;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Ledger;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser.LedgerBook
{
    public class LedgerBookControllerFileOperations
    {
        private readonly DemoFileHelper demoFileHelper;
        private readonly ILedgerBookRepository ledgerRepository;
        private readonly IUserMessageBox messageBox;
        private readonly Func<IUserPromptOpenFile> openFileDialogFactory;
        private readonly IUserQuestionBoxYesNo questionBox;
        private readonly Func<IUserPromptSaveFile> saveFileDialogFactory;
        private readonly Func<IWaitCursor> waitCursorFactory;

        private string ledgerBookFileName;

        public LedgerBookControllerFileOperations(
            [NotNull] UiContext uiContext,
            [NotNull] IMessenger messenger,
            [NotNull] ILedgerBookRepository ledgerBookRepository,
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

            if (ledgerBookRepository == null)
            {
                throw new ArgumentNullException("ledgerBookRepository");
            }

            if (demoFileHelper == null)
            {
                throw new ArgumentNullException("demoFileHelper");
            }

            this.saveFileDialogFactory = uiContext.UserPrompts.SaveFileFactory;
            this.openFileDialogFactory = uiContext.UserPrompts.OpenFileFactory;
            this.questionBox = uiContext.UserPrompts.YesNoBox;
            this.messageBox = uiContext.UserPrompts.MessageBox;

            this.waitCursorFactory = uiContext.WaitCursorFactory;
            this.ledgerRepository = ledgerBookRepository;
            this.demoFileHelper = demoFileHelper;
            MessengerInstance = messenger;

            ViewModel = new LedgerBookViewModel();
        }

        public ICommand CloseLedgerBookCommand
        {
            get { return new RelayCommand(OnCloseLedgerBookCommandExecuted, CanExecuteCloseLedgerBookCommand); }
        }

        public ICommand DemoLedgerBookCommand
        {
            get { return new RelayCommand(OnDemoLedgerBookCommandExecute); }
        }

        public ICommand LoadLedgerBookCommand
        {
            get { return new RelayCommand(OnLoadLedgerBookCommandExecute); }
        }

        public IMessenger MessengerInstance { get; set; }

        public ICommand NewLedgerBookCommand
        {
            get { return new RelayCommand(OnNewLedgerBookCommandExecuted, CanExecuteNewLedgerBookCommand); }
        }

        public ICommand SaveLedgerBookCommand
        {
            get { return new RelayCommand(OnSaveLedgerBookCommandExecute, CanExecuteSaveCommand); }
        }

        internal bool Dirty { get; set; }

        internal LedgerBookViewModel ViewModel { get; set; }

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

        internal void ExtractDataFromApplicationState(string lastLedgerBookFileName)
        {
            this.ledgerBookFileName = lastLedgerBookFileName;

            if (!string.IsNullOrWhiteSpace(this.ledgerBookFileName))
            {
                LoadLedgerBookFromFile(this.ledgerBookFileName);
                this.ledgerBookFileName = null;
            }
        }

        internal void ReloadCurrentLedgerBook()
        {
            string fileName = ViewModel.LedgerBook.FileName;
            OnCloseLedgerBookCommandExecuted();
            LoadLedgerBookFromFile(fileName);
        }

        internal void SaveLedgerBook()
        {
            this.ledgerRepository.Save(ViewModel.LedgerBook);
            Dirty = false;
            ViewModel.NewLedgerLine = null;
        }

        internal LastLedgerBookLoadedV1 StateDataForPersistence()
        {
            var lastLedgerBook = new LastLedgerBookLoadedV1
            {
                Model = ViewModel.LedgerBook == null ? this.ledgerBookFileName : ViewModel.LedgerBook.FileName,
            };
            return lastLedgerBook;
        }

        private bool CanExecuteCloseLedgerBookCommand()
        {
            return ViewModel.LedgerBook != null || !string.IsNullOrWhiteSpace(this.ledgerBookFileName);
        }

        private bool CanExecuteNewLedgerBookCommand()
        {
            return ViewModel.LedgerBook == null && string.IsNullOrWhiteSpace(this.ledgerBookFileName);
        }

        private bool CanExecuteSaveCommand()
        {
            return ViewModel.LedgerBook != null && Dirty;
        }

        private void LoadLedgerBookFromFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            using (this.waitCursorFactory())
            {
                try
                {
                    if (!this.ledgerRepository.Exists(fileName))
                    {
                        throw new FileNotFoundException("The requested file, or the previously loaded file, cannot be located.\n" + fileName, fileName);
                    }

                    ViewModel.LedgerBook = this.ledgerRepository.Load(fileName);
                    MessengerInstance.Send(new LedgerBookReadyMessage(ViewModel.LedgerBook));
                }
                catch (FileFormatException ex)
                {
                    this.messageBox.Show(ex, "Unable to load the requested Ledger-Book file");
                }
                catch (FileNotFoundException ex)
                {
                    this.messageBox.Show(ex, "Unable to load the requested Ledger-Book file");
                }
            }
        }

        private void OnCloseLedgerBookCommandExecuted()
        {
            CheckIfSaveRequired();
            ViewModel.LedgerBook = null;
            this.ledgerBookFileName = null;
            MessengerInstance.Send(new LedgerBookReadyMessage(null));
        }

        private void OnDemoLedgerBookCommandExecute()
        {
            try
            {
                LoadLedgerBookFromFile(this.demoFileHelper.FindDemoFile(@"DemoLedgerBook.xml"));
            }
            catch (IOException)
            {
                this.messageBox.Show("Unable to find the demo Ledger-Book file.");
            }
        }

        private void OnLoadLedgerBookCommandExecute()
        {
            IUserPromptOpenFile openFileDialog = this.openFileDialogFactory();
            openFileDialog.AddExtension = true;
            openFileDialog.CheckPathExists = true;
            openFileDialog.DefaultExt = ".xml";
            openFileDialog.Filter = "LedgerBook files (*.xml, *.xaml)|*.xml;*.xaml|All files (*.*)|*.*";
            openFileDialog.Title = "Choose a LedgerBook xml file to load.";
            bool? result = openFileDialog.ShowDialog();
            if (result == null || !result.Value)
            {
                return;
            }
            string fileName = openFileDialog.FileName;

            LoadLedgerBookFromFile(fileName);
        }

        private void OnNewLedgerBookCommandExecuted()
        {
            IUserPromptSaveFile saveFileDialog = this.saveFileDialogFactory();
            saveFileDialog.AddExtension = true;
            saveFileDialog.CheckPathExists = true;
            saveFileDialog.DefaultExt = ".xml";
            saveFileDialog.Filter = "LedgerBook files (*.xml, *.xaml)|*.xml;*.xaml|All files (*.*)|*.*";
            saveFileDialog.Title = "Choose a LedgerBook xml file name.";
            bool? result = saveFileDialog.ShowDialog();
            if (result == null || !result.Value)
            {
                return;
            }

            string fileName = saveFileDialog.FileName;
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            OnCloseLedgerBookCommandExecuted();

            ViewModel.LedgerBook = this.ledgerRepository.CreateNew("New LedgerBook, give me a proper name :-(", fileName);
            Dirty = true;
            MessengerInstance.Send(new LedgerBookReadyMessage(ViewModel.LedgerBook));
        }

        private void OnSaveLedgerBookCommandExecute()
        {
            SaveLedgerBook();
        }
    }
}