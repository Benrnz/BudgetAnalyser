using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Statement;
using GalaSoft.MvvmLight.Command;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;
using Rees.Wpf.ApplicationState;
using FileFormatException = BudgetAnalyser.Engine.FileFormatException;

namespace BudgetAnalyser.LedgerBook
{
    public class LedgerBookController : ControllerBase, IShowableController
    {
        private readonly DemoFileHelper demoFileHelper;
        private readonly IUserInputBox inputBox;
        private readonly ILedgerBookRepository ledgerRepository;
        private readonly IUserMessageBox messageBox;
        private readonly Func<IUserPromptOpenFile> openFileDialogFactory;
        private readonly IUserQuestionBoxYesNo questionBox;
        private readonly Func<IUserPromptSaveFile> saveFileDialogFactory;
        private readonly Func<IWaitCursor> waitCursorFactory;

        private bool dirty;
        private BudgetCurrencyContext doNotUseCurrentBudget;
        private StatementModel doNotUseCurrentStatement;
        private bool doNotUseShown;
        private Engine.Ledger.LedgerBook ledgerBook;

        private string ledgerBookFileName;

        /// <summary>
        ///     This variable is used to contain the newly added ledger line when doing a new reconciliation. When this is non-null
        ///     it also indicates the ledger row can be edited.
        /// </summary>
        private LedgerEntryLine newLedgerLine;

        public LedgerBookController(
            [NotNull] UiContext uiContext,
            [NotNull] ILedgerBookRepository ledgerRepository,
            [NotNull] DemoFileHelper demoFileHelper)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            if (ledgerRepository == null)
            {
                throw new ArgumentNullException("ledgerRepository");
            }

            if (demoFileHelper == null)
            {
                throw new ArgumentNullException("demoFileHelper");
            }

            this.openFileDialogFactory = uiContext.UserPrompts.OpenFileFactory;
            this.saveFileDialogFactory = uiContext.UserPrompts.SaveFileFactory;
            this.messageBox = uiContext.UserPrompts.MessageBox;
            this.waitCursorFactory = uiContext.WaitCursorFactory;
            this.questionBox = uiContext.UserPrompts.YesNoBox;
            this.inputBox = uiContext.UserPrompts.InputBox;
            this.ledgerRepository = ledgerRepository;
            this.demoFileHelper = demoFileHelper;
            ChooseBudgetBucketController = uiContext.ChooseBudgetBucketController;
            AddLedgerReconciliationController = uiContext.AddLedgerReconciliationController;
            LedgerTransactionsController = uiContext.LedgerTransactionsController;
            LedgerRemarksController = uiContext.LedgerRemarksController;

            MessengerInstance = uiContext.Messenger;
            MessengerInstance.Register<ApplicationStateRequestedMessage>(this, OnApplicationStateRequested);
            MessengerInstance.Register<ApplicationStateLoadedMessage>(this, OnApplicationStateLoaded);
            MessengerInstance.Register<BudgetReadyMessage>(this, OnBudgetReadyMessageReceived);
            MessengerInstance.Register<StatementReadyMessage>(this, OnStatementReadyMessageReceived);
        }

        public event EventHandler LedgerBookUpdated;

        public AddLedgerReconciliationController AddLedgerReconciliationController { get; private set; }

        public ICommand AddNewLedgerBookCommand
        {
            get { return new RelayCommand(OnAddNewLedgerBookCommandExecuted, CanExecuteNewLedgerBookCommand); }
        }

        public ICommand AddNewLedgerCommand
        {
            get { return new RelayCommand(OnAddNewLedgerCommandExecuted, CanExecuteAddNewLedgerCommand); }
        }

        public ICommand AddNewReconciliationCommand
        {
            get { return new RelayCommand(OnAddNewReconciliationCommandExecuted, CanExecuteAddNewReconciliationCommand); }
        }

        public ChooseBudgetBucketController ChooseBudgetBucketController { get; private set; }

        public ICommand CloseLedgerBookCommand
        {
            get { return new RelayCommand(OnCloseLedgerBookCommandExecuted, CanExecuteCloseLedgerBookCommand); }
        }

        public ICommand DemoLedgerBookCommand
        {
            get { return new RelayCommand(OnDemoLedgerBookCommandExecute); }
        }

        public Engine.Ledger.LedgerBook LedgerBook
        {
            get { return this.ledgerBook; }

            private set
            {
                this.ledgerBook = value;
                RaisePropertyChanged(() => LedgerBook);
                RaisePropertyChanged(() => NoLedgerBookLoaded);
            }
        }

        public LedgerRemarksController LedgerRemarksController { get; private set; }

        public LedgerTransactionsController LedgerTransactionsController { get; private set; }

        public ICommand LoadLedgerBookCommand
        {
            get { return new RelayCommand(OnLoadLedgerBookCommandExecute); }
        }

        public bool NoBudgetLoaded
        {
            get { return CurrentBudget == null; }
        }

        public bool NoLedgerBookLoaded
        {
            get { return LedgerBook == null; }
        }

        public bool NoStatementLoaded
        {
            get { return CurrentStatement == null; }
        }

        public ICommand SaveLedgerBookCommand
        {
            get { return new RelayCommand(OnSaveLedgerBookCommandExecute, CanExecuteSaveCommand); }
        }

        public ICommand ShowBankBalancesCommand
        {
            get { return new RelayCommand<LedgerEntryLine>(OnShowBankBalancesCommandExecute, param => param != null); }
        }

        public ICommand ShowRemarksCommand
        {
            get { return new RelayCommand<LedgerEntryLine>(OnShowRemarksCommandExecuted, CanExecuteShowRemarksCommand); }
        }

        public ICommand ShowTransactionsCommand
        {
            get { return new RelayCommand<object>(OnShowTransactionsCommandExecuted); }
        }

        public bool Shown
        {
            get { return this.doNotUseShown; }

            set
            {
                if (value == this.doNotUseShown)
                {
                    return;
                }
                this.doNotUseShown = value;
                RaisePropertyChanged(() => Shown);
            }
        }

        public ICommand UnlockLedgerLineCommand
        {
            get { return new RelayCommand(OnUnlockLedgerLineCommandExecuted, CanExecuteUnlockLedgerLineCommand); }
        }

        private BudgetCurrencyContext CurrentBudget
        {
            get { return this.doNotUseCurrentBudget; }
            set
            {
                this.doNotUseCurrentBudget = value;
                RaisePropertyChanged(() => NoBudgetLoaded);
            }
        }

        private StatementModel CurrentStatement
        {
            get { return this.doNotUseCurrentStatement; }

            set
            {
                this.doNotUseCurrentStatement = value;
                RaisePropertyChanged(() => NoStatementLoaded);
            }
        }

        public void EditLedgerBookName()
        {
            if (LedgerBook == null)
            {
                return;
            }

            string result = this.inputBox.Show("Edit Ledger Book Name", "Enter a new name", LedgerBook.Name);
            if (string.IsNullOrWhiteSpace(result))
            {
                return;
            }

            LedgerBook.Name = result;
            this.dirty = true;
            RaisePropertyChanged(() => LedgerBook);
        }

        public void NotifyOfClosing()
        {
            CheckIfSaveRequired();
        }

        private bool CanExecuteAddNewLedgerCommand()
        {
            return LedgerBook != null;
        }

        private bool CanExecuteAddNewReconciliationCommand()
        {
            return CurrentBudget != null && CurrentStatement != null && LedgerBook != null;
        }

        private bool CanExecuteCloseLedgerBookCommand()
        {
            return LedgerBook != null || !string.IsNullOrWhiteSpace(this.ledgerBookFileName);
        }

        private bool CanExecuteNewLedgerBookCommand()
        {
            return LedgerBook == null && string.IsNullOrWhiteSpace(this.ledgerBookFileName);
        }

        private bool CanExecuteSaveCommand()
        {
            return LedgerBook != null && this.dirty;
        }

        private bool CanExecuteShowRemarksCommand(LedgerEntryLine parameter)
        {
            return parameter != null
                   && (!string.IsNullOrWhiteSpace(parameter.Remarks) || parameter == this.newLedgerLine);
        }

        private bool CanExecuteUnlockLedgerLineCommand()
        {
            return this.newLedgerLine == null && LedgerBook != null;
        }

        private void CheckIfSaveRequired()
        {
            if (this.dirty)
            {
                bool? result = this.questionBox.Show("Save changes?", "Ledger Book");
                if (result != null && result.Value)
                {
                    this.ledgerRepository.Save(LedgerBook);
                }

                this.dirty = false;
            }
        }

        private void FinaliseAddingReconciliation(bool ignoreWarnings = false)
        {
            try
            {
                this.newLedgerLine = LedgerBook.Reconcile(
                    AddLedgerReconciliationController.Date,
                    AddLedgerReconciliationController.BankBalances,
                    CurrentBudget.Model,
                    CurrentStatement,
                    ignoreWarnings);
                this.dirty = true;
                EventHandler handler = LedgerBookUpdated;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }
            catch (ValidationWarningException ex)
            {
                bool? result = this.questionBox.Show("Warning", "Warning: {0}\nDo you wish to proceed?", ex.Message);
                if (result == null || !result.Value)
                {
                    return;
                }

                FinaliseAddingReconciliation(true);
            }
            catch (InvalidOperationException ex)
            {
                this.messageBox.Show(ex, "Unable to add new reconciliation.");
            }
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

                    LedgerBook = this.ledgerRepository.Load(fileName);
                    MessengerInstance.Send(new LedgerBookReadyMessage(LedgerBook));
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

        private void OnAddNewLedgerBookCommandExecuted()
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

            LedgerBook = this.ledgerRepository.CreateNew("New LedgerBook, give me a proper name :-(", fileName);
            this.dirty = true;
            MessengerInstance.Send(new LedgerBookReadyMessage(LedgerBook));
        }

        private void OnAddNewLedgerCommandExecuted()
        {
            ChooseBudgetBucketController.Chosen += OnBudgetBucketChosen;
            ChooseBudgetBucketController.Filter(bucket => bucket is ExpenseBudgetBucket, "Choose an Expense Budget Bucket");
            ChooseBudgetBucketController.ShowDialog(BudgetAnalyserFeature.LedgerBook);
        }

        private void OnAddNewReconciliationCommandExecuted()
        {
            AddLedgerReconciliationController.Complete += OnAddReconciliationComplete;
            AddLedgerReconciliationController.ShowCreateDialog();
        }

        private void OnAddReconciliationComplete(object sender, EventArgs e)
        {
            AddLedgerReconciliationController.Complete -= OnAddReconciliationComplete;

            if (AddLedgerReconciliationController.Canceled)
            {
                return;
            }

            FinaliseAddingReconciliation();
        }

        private void OnApplicationStateLoaded(ApplicationStateLoadedMessage message)
        {
            if (!message.RehydratedModels.ContainsKey(typeof(LastLedgerBookLoadedV1)))
            {
                return;
            }

            var lastLedgerBookFileName = message.RehydratedModels[typeof(LastLedgerBookLoadedV1)].AdaptModel<string>();
            this.ledgerBookFileName = lastLedgerBookFileName;

            if (!string.IsNullOrWhiteSpace(this.ledgerBookFileName))
            {
                LoadLedgerBookFromFile(this.ledgerBookFileName);
                this.ledgerBookFileName = null;
            }
        }

        private void OnApplicationStateRequested(ApplicationStateRequestedMessage message)
        {
            var lastLedgerBook = new LastLedgerBookLoadedV1
            {
                Model = LedgerBook == null ? this.ledgerBookFileName : LedgerBook.FileName,
            };
            message.PersistThisModel(lastLedgerBook);
        }

        private void OnBudgetBucketChosen(object sender, EventArgs e)
        {
            ChooseBudgetBucketController.Chosen -= OnBudgetBucketChosen;
            if (ChooseBudgetBucketController.Selected == null)
            {
                return;
            }

            var selectedBucket = ChooseBudgetBucketController.Selected as ExpenseBudgetBucket;
            if (selectedBucket == null)
            {
                return;
            }

            LedgerBook.AddLedger(selectedBucket);
        }

        private void OnBudgetReadyMessageReceived(BudgetReadyMessage message)
        {
            if (message.ActiveBudget.BudgetActive)
            {
                CurrentBudget = message.ActiveBudget;
            }
        }

        private void OnCloseLedgerBookCommandExecuted()
        {
            CheckIfSaveRequired();
            LedgerBook = null;
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

        private void OnSaveLedgerBookCommandExecute()
        {
            this.ledgerRepository.Save(LedgerBook);
            this.dirty = false;
        }

        private void OnShowBankBalancesCommandExecute(LedgerEntryLine line)
        {
            AddLedgerReconciliationController.ShowEditDialog(line, line == this.newLedgerLine);
        }

        private void OnShowRemarksCommandExecuted(LedgerEntryLine parameter)
        {
            LedgerRemarksController.Completed += OnShowRemarksCompleted;
            LedgerRemarksController.Show(parameter, parameter == this.newLedgerLine);
        }

        private void OnShowRemarksCompleted(object sender, EventArgs e)
        {
            LedgerRemarksController.Completed -= OnShowRemarksCompleted;
        }

        private void OnShowTransactionsCommandExecuted(object parameter)
        {
            if (parameter == null)
            {
                return;
            }

            LedgerTransactionsController.Complete += OnShowTransactionsCompleted;

            var ledgerEntry = parameter as LedgerEntry; // used when legder entry transactions are being displayed
            var bankBalanceAdjustments = parameter as LedgerEntryLine; // used when balance adjustments are being displayed

            if (ledgerEntry != null)
            {
                bool isNew = this.newLedgerLine != null && this.newLedgerLine.Entries.Any(e => e == ledgerEntry);
                LedgerTransactionsController.ShowDialog(ledgerEntry, isNew);
            }
            else if (bankBalanceAdjustments != null)
            {
                LedgerTransactionsController.ShowDialog(bankBalanceAdjustments, bankBalanceAdjustments == this.newLedgerLine);
            }
            else
            {
                throw new ArgumentException("Invalid paramter passed to ShowTransactionsCommand: " + parameter);
            }
        }

        private void OnShowTransactionsCompleted(object sender, LedgerTransactionEventArgs args)
        {
            LedgerTransactionsController.Complete -= OnShowTransactionsCompleted;

            if (args.WasModified)
            {
                EventHandler handler = LedgerBookUpdated;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }
        }

        private void OnStatementReadyMessageReceived(StatementReadyMessage message)
        {
            CurrentStatement = message.StatementModel;
        }

        private void OnUnlockLedgerLineCommandExecuted()
        {
            bool? response = this.questionBox.Show(
                "Unlock Ledger Entry Line",
                "Are you sure you want to unlock the Ledger Entry Line dated {0:d} for editing?",
                LedgerBook.DatedEntries.First().Date);

            if (response == null || response.Value == false)
            {
                return;
            }

            this.newLedgerLine = LedgerBook.UnlockMostRecentLine();
            this.dirty = true;
        }
    }
}