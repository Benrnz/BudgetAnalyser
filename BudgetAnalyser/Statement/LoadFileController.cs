using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.ShellDialog;
using GalaSoft.MvvmLight.Command;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;

namespace BudgetAnalyser.Statement
{
    public sealed class LoadFileController : ControllerBase, IShellDialogInteractivity, IShellDialogToolTips, IDisposable
    {
        // Track whether Dispose has been called. 

        private readonly IAccountTypeRepository accountTypeRepository;
        private readonly IUserMessageBox messageBox;
        private readonly IVersionedStatementModelRepository statementModelRepository;
        private readonly Func<IUserPromptOpenFile> userPromptOpenFileFactory;
        private bool actionButtonReady;
        private bool disposed;
        private string doNotUseAccountName;
        private bool doNotUseExistingAccountName;
        private string doNotUseFileName;
        private bool doNotUseFileTypeSelectionReady;
        private bool doNotUseNewAccountName;
        private string doNotUseSelectedExistingAccountName;
        private string doNotUseTitle;
        private Task fileSelectionTask;
        private Guid dialogCorrelationId;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "OnPropertyChange is ok to call here")]
        public LoadFileController(
            [NotNull] UiContext uiContext,
            [NotNull] IAccountTypeRepository accountTypeRepository,
            [NotNull] IVersionedStatementModelRepository statementModelRepository)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            if (accountTypeRepository == null)
            {
                throw new ArgumentNullException("accountTypeRepository");
            }

            if (statementModelRepository == null)
            {
                throw new ArgumentNullException("statementModelRepository");
            }

            this.messageBox = uiContext.UserPrompts.MessageBox;
            this.statementModelRepository = statementModelRepository;
            this.userPromptOpenFileFactory = uiContext.UserPrompts.OpenFileFactory;
            this.accountTypeRepository = accountTypeRepository;
            UseExistingAccountName = true;

            MessengerInstance = uiContext.Messenger;
            MessengerInstance.Register<ShellDialogResponseMessage>(this, OnShellDialogResponseReceived);
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="LoadFileController" /> class.
        ///     Use C# destructor syntax for finalization code.
        ///     This destructor will run only if the Dispose method
        ///     does not get called.
        ///     It gives your base class the opportunity to finalize.
        ///     Do not provide destructors in types derived from this class.
        /// </summary>
        ~LoadFileController()
        {
            // Do not re-create Dispose clean-up code here. 
            // Calling Dispose(false) is optimal in terms of 
            // readability and maintainability. 
            Dispose(false);
        }

        public string AccountName
        {
            get { return this.doNotUseAccountName; }

            set
            {
                this.doNotUseAccountName = value;
                RaisePropertyChanged(() => AccountName);
                CheckAccountName();
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Used by data binding")]
        public string AccountNameHelp
        {
            get { return "When importing a new bank statement file, you must select what account type the statement comes from.\nThis allows merging of multiple accounts into one file."; }
        }

        public AccountType AccountType
        {
            get { return this.accountTypeRepository.GetOrCreateNew(AccountName); }
        }

        public string ActionButtonToolTip { get; private set; }

        public ICommand BrowseForFileCommand
        {
            get { return new RelayCommand(OnBrowseForFileCommandExecute); }
        }

        public bool CanExecuteCancelButton
        {
            get { return true; }
        }

        public bool CanExecuteOkButton
        {
            get { return this.actionButtonReady; }
        }

        public bool CanExecuteSaveButton
        {
            get { return false; }
        }

        public string CloseButtonToolTip
        {
            get { return "Cancel"; }
        }

        public IEnumerable<string> ExistingAccountNames { get; private set; }

        public string FileName
        {
            get { return this.doNotUseFileName; }

            set
            {
                this.doNotUseFileName = value;
                RaisePropertyChanged(() => FileName);
                if (!string.IsNullOrWhiteSpace(FileName))
                {
                    CheckFileName();
                }
                else
                {
                    FileTypeSelectionReady = false;
                }
            }
        }

        public bool FileTypeSelectionReady
        {
            get { return this.doNotUseFileTypeSelectionReady; }
            private set
            {
                this.doNotUseFileTypeSelectionReady = value;
                RaisePropertyChanged(() => FileTypeSelectionReady);
            }
        }

        public bool? LastFileWasBudgetAnalyserStatementFile { get; private set; }

        public string SelectedExistingAccountName
        {
            get { return this.doNotUseSelectedExistingAccountName; }

            set
            {
                this.doNotUseSelectedExistingAccountName = value;
                RaisePropertyChanged(() => SelectedExistingAccountName);
                CheckAccountName();
            }
        }

        public string SuggestedDateRange { get; private set; }

        public string Title
        {
            get { return this.doNotUseTitle; }
            private set
            {
                this.doNotUseTitle = value;
                RaisePropertyChanged(() => Title);
            }
        }

        public bool UseExistingAccountName
        {
            get { return this.doNotUseExistingAccountName; }
            set
            {
                this.doNotUseExistingAccountName = value;
                RaisePropertyChanged(() => UseExistingAccountName);
                CheckAccountName();
            }
        }

        public bool UseNewAccountName
        {
            get { return this.doNotUseNewAccountName; }
            set
            {
                this.doNotUseNewAccountName = value;
                RaisePropertyChanged(() => UseNewAccountName);
                CheckAccountName();
            }
        }

        /// <summary>
        ///     Implement IDisposable.
        ///     Do not make this method virtual.
        ///     A derived class should not be able to override this method
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            // Take yourself off the Finalization queue 
            // to prevent finalization code for this object 
            // from executing a second time. 
            GC.SuppressFinalize(this);
        }

        public Task RequestUserInputForMerging(StatementModel currentStatement)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("LoadFileController.RequestUserInputForMerging");
            } 

            LastFileWasBudgetAnalyserStatementFile = null;
            SuggestedDateRange = null;
            Title = "Merge Statement";
            ActionButtonToolTip = "Merge transactions from the selected file into the current statement file.";
            if (currentStatement != null)
            {
                DateTime lastTransactionDate = currentStatement.AllTransactions.Max(t => t.Date).Date.AddDays(1);
                DateTime maxDate = DateTime.Today.AddDays(-1); // Never import up to today. Sometimes today's transactions don't appear until tomorrow.
                if (maxDate.DayOfWeek == DayOfWeek.Monday)
                {
                    // Monday is not an ideal day to end a date range as some banks may back date weekend transactions after Monday night processing.
                    maxDate = maxDate.AddDays(-3);
                }
                else if (maxDate.DayOfWeek == DayOfWeek.Saturday)
                {
                    // Weekends may have back dated transactions after Monday processing.
                    maxDate = maxDate.AddDays(-1);
                }
                else if (maxDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    // Weekends may have back dated transactions after Monday processing.
                    maxDate = maxDate.AddDays(-2);
                }

                SuggestedDateRange = string.Format(CultureInfo.CurrentCulture, "{0:d} to {1:d}", lastTransactionDate, maxDate);
            }

            return RequestUserInputCommomPreparation(this.accountTypeRepository.ListCurrentlyUsedAccountTypes());
        }

        public Task RequestUserInputForOpenFile()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("LoadFileController.RequestUserInputForOpenFile");
            }
            
            ActionButtonToolTip = "Open the selected file. Any statement file already open will be closed first.";
            LastFileWasBudgetAnalyserStatementFile = null;
            SuggestedDateRange = null;
            Title = "Open Statement";
            return RequestUserInputCommomPreparation(this.accountTypeRepository.ListCurrentlyUsedAccountTypes());
        }

        public void Reset()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("LoadFileController.Reset");
            } 

            LastFileWasBudgetAnalyserStatementFile = null;
            FileName = null;
            AccountName = null;
        }

        /// <summary>
        ///     Dispose(bool disposing) executes in two distinct scenarios.
        ///     If disposing equals true, the method has been called directly
        ///     or indirectly by a user's code. Managed and unmanaged resources
        ///     can be disposed.
        ///     If disposing equals false, the method has been called by the
        ///     runtime from inside the finalizer and you should not reference
        ///     other objects. Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing">
        ///     <c>true</c> to release both managed and unmanaged
        ///     resources; <c>false</c> to release only unmanaged resources.
        /// </param>
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called. 
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources. 
                if (disposing)
                {
                    // Dispose managed resources. 
                    if (this.fileSelectionTask != null)
                    {
                        this.fileSelectionTask.Dispose();
                    }
                }
            }

            this.disposed = true;
        }

        private List<string> PrepareAccountNames(IEnumerable<string> existingAccountNames)
        {
            var commonAccountNames = this.accountTypeRepository.ListCurrentlyUsedAccountTypes()
                .Select(a => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(a.Name.ToLowerInvariant()));
            List<string> listOfNames = existingAccountNames.ToList();
            listOfNames.AddRange(commonAccountNames);
            listOfNames.Insert(0, string.Empty);
            return listOfNames.Distinct().OrderBy(n => n).ToList();
        }

        private bool ActionCommandCanExecute()
        {
            return this.actionButtonReady;
        }

        private void CheckAccountName()
        {
            if (!FileTypeSelectionReady)
            {
                return;
            }

            if (UseExistingAccountName)
            {
                if (string.IsNullOrWhiteSpace(SelectedExistingAccountName))
                {
                    this.actionButtonReady = false;
                    return;
                }

                this.actionButtonReady = true;
                return;
            }

            if (UseNewAccountName)
            {
                if (string.IsNullOrWhiteSpace(AccountName))
                {
                    this.actionButtonReady = false;
                    return;
                }

                this.actionButtonReady = true;
            }
        }

        private void CheckFileName()
        {
            LastFileWasBudgetAnalyserStatementFile = this.statementModelRepository.IsValidFile(FileName);
            if (LastFileWasBudgetAnalyserStatementFile ?? false)
            {
                FileTypeSelectionReady = false;
                this.actionButtonReady = true;
            }
            else
            {
                // Appears to be a new statement that has never been loaded before.
                FileTypeSelectionReady = true;
                this.actionButtonReady = false;
            }
        }

        private void OnBrowseForFileCommandExecute()
        {
            IUserPromptOpenFile dialog = this.userPromptOpenFileFactory();
            dialog.DefaultExt = "*.CSV";
            dialog.Title = "Select a CSV file of transactions to load.";
            dialog.Filter = "Comma Separated Values (*.CSV)|*.CSV";
            bool? result = dialog.ShowDialog();
            if (result == null || result == false)
            {
                FileName = null;
                return;
            }

            if (!File.Exists(dialog.FileName))
            {
                this.messageBox.Show("File not found.\n" + dialog.FileName, "Open file");
                FileName = null;
                return;
            }

            FileName = dialog.FileName;
        }

        private void OnShellDialogResponseReceived(ShellDialogResponseMessage message)
        {
            if (!message.IsItForMe(this.dialogCorrelationId))
            {
                return;
            }

            if (message.Response == ShellDialogButton.Cancel)
            {
                Reset();
                this.fileSelectionTask.Start();
                return;
            }

            if (string.IsNullOrWhiteSpace(AccountName))
            {
                AccountName = SelectedExistingAccountName;
            }

            // FileName is already set by data binding.

            // Use the task to signal completion.
            this.fileSelectionTask.Start();
        }

        private Task RequestUserInputCommomPreparation(IEnumerable<AccountType> existingAccountNames)
        {
            UseExistingAccountName = true;
            UseNewAccountName = false;
            FileName = null;
            AccountName = null;
            List<string> existingAccountNamesCopy = existingAccountNames.Select(a => a.Name).OrderBy(n => n).ToList();
            List<string> listOfNames = PrepareAccountNames(existingAccountNamesCopy);
            ExistingAccountNames = listOfNames;
            SelectedExistingAccountName = listOfNames.First();

            this.dialogCorrelationId = Guid.NewGuid();
            var popRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.Transactions, this, ShellDialogType.OkCancel)
            {
                CorrelationId = this.dialogCorrelationId,
                Title = Title
            };

            if (this.fileSelectionTask != null)
            {
                this.fileSelectionTask.Dispose();
            }

            this.fileSelectionTask = new Task(() => { });
            MessengerInstance.Send(popRequest);
            return this.fileSelectionTask;
        }
    }
}