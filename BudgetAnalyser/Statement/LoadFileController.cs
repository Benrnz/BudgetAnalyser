using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetAnalyser.Annotations;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.ShellDialog;
using GalaSoft.MvvmLight.CommandWpf;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;

namespace BudgetAnalyser.Statement
{
    public class LoadFileController : ControllerBase, IShellDialogInteractivity, IShellDialogToolTips, IDisposable
    {
        private readonly IAccountTypeRepository accountTypeRepository;
        private readonly IUserMessageBox messageBox;
        private readonly Func<IUserPromptOpenFile> userPromptOpenFileFactory;
        private Guid dialogCorrelationId;
        private bool disposed;
        private string doNotUseFileName;
        private bool doNotUseFileTypeSelectionReady;
        private Account doNotUseSelectedExistingAccountName;
        private string doNotUseTitle;
        private Task fileSelectionTask;
        private bool showingDialog;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "OnPropertyChange is ok to call here")]
        public LoadFileController(
            [NotNull] IUiContext uiContext,
            [NotNull] IAccountTypeRepository accountTypeRepository)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException(nameof(uiContext));
            }

            if (accountTypeRepository == null)
            {
                throw new ArgumentNullException(nameof(accountTypeRepository));
            }

            this.messageBox = uiContext.UserPrompts.MessageBox;
            this.userPromptOpenFileFactory = uiContext.UserPrompts.OpenFileFactory;
            this.accountTypeRepository = accountTypeRepository;

            MessengerInstance = uiContext.Messenger;
            MessengerInstance.Register<ShellDialogResponseMessage>(this, OnShellDialogResponseReceived);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Used by data binding")]
        [UsedImplicitly]
        public string AccountNameHelp => "When importing a new bank statement file, you must select the account the statement comes from.\nThis allows merging of multiple accounts into one file.";

        public string ActionButtonToolTip { get; private set; }
        public ICommand BrowseForFileCommand => new RelayCommand(OnBrowseForFileCommandExecute);
        public bool CanExecuteCancelButton => true;
        public bool CanExecuteOkButton { get; private set; }
        public bool CanExecuteSaveButton => false;
        public string CloseButtonToolTip => "Cancel";
        public IEnumerable<Account> ExistingAccountNames { get; private set; }

        public string FileName
        {
            get { return this.doNotUseFileName; }

            set
            {
                this.doNotUseFileName = value;
                RaisePropertyChanged();
                if (!string.IsNullOrWhiteSpace(FileName))
                {
                    CheckFileName();
                    CheckAccountName();
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
                RaisePropertyChanged();
            }
        }

        public bool MergeMode { [UsedImplicitly] get; private set; }

        public Account SelectedExistingAccountName
        {
            get { return this.doNotUseSelectedExistingAccountName; }

            set
            {
                this.doNotUseSelectedExistingAccountName = value;
                RaisePropertyChanged();
                CheckAccountName();
                CheckFileName();
            }
        }

        public string SuggestedDateRange { [UsedImplicitly] get; private set; }

        public string Title
        {
            get { return this.doNotUseTitle; }
            private set
            {
                this.doNotUseTitle = value;
                RaisePropertyChanged();
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

            MergeMode = true;
            SuggestedDateRange = null;
            Title = "Merge Statement";
            ActionButtonToolTip = "Merge transactions from the selected file into the current statement file.";
            if (currentStatement != null)
            {
                CalculateSuggestedDateRange(currentStatement);
            }

            return RequestUserInputCommomPreparation();
        }

        public Task RequestUserInputForOpenFile()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("LoadFileController.RequestUserInputForOpenFile");
            }

            MergeMode = false;
            ActionButtonToolTip = "Open the selected file. Any statement file already open will be closed first.";
            SuggestedDateRange = null;
            Title = "Open Statement";
            return RequestUserInputCommomPreparation();
        }

        public void Reset()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("LoadFileController.Reset");
            }

            FileName = null;
        }

        private void CalculateSuggestedDateRange(StatementModel currentStatement)
        {
            var lastTransactionDate = currentStatement.AllTransactions.Max(t => t.Date).Date.AddDays(1);
            var maxDate = DateTime.Today;
            if (maxDate.DayOfWeek == DayOfWeek.Monday)
            {
                // Monday is not an ideal day to end a date range as some banks may back date weekend transactions after Monday night processing.
                maxDate = maxDate.AddDays(-3); // Set to Friday.
            }
            else if (maxDate.DayOfWeek == DayOfWeek.Saturday)
            {
                // Weekends may have back dated transactions after Monday processing.
                maxDate = maxDate.AddDays(-1); // Set to Friday.
            }
            else if (maxDate.DayOfWeek == DayOfWeek.Sunday)
            {
                // Weekends may have back dated transactions after Monday processing.
                maxDate = maxDate.AddDays(-2);
            }
            else
            {
                // Any other day of the week just import up to yesterday.
                // Never import up to today. Sometimes today's transactions don't appear until tomorrow.
                maxDate = maxDate.AddDays(-1);
            }

            SuggestedDateRange = string.Format(CultureInfo.CurrentCulture, "{0:d} to {1:d}", lastTransactionDate, maxDate);
        }

        private void CheckAccountName()
        {
            if (!FileTypeSelectionReady)
            {
                return;
            }

            if (SelectedExistingAccountName == null)
            {
                CanExecuteOkButton = false;
                return;
            }

            CanExecuteOkButton = true;
        }

        private void CheckFileName()
        {
            if (string.IsNullOrEmpty(FileName))
            {
                CanExecuteOkButton = false;
                FileTypeSelectionReady = false;
                return;
            }

            FileTypeSelectionReady = true;
            CanExecuteOkButton = true;
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
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "fileSelectionTask", Justification = "R# bug")]
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
                    this.fileSelectionTask?.Dispose();
                }
            }

            this.disposed = true;
        }

        private void OnBrowseForFileCommandExecute()
        {
            if (this.showingDialog)
            {
                return;
            }

            try
            {
                this.showingDialog = true;
                var dialog = this.userPromptOpenFileFactory();
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
            finally
            {
                this.showingDialog = false;
            }
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

            // FileName is already set by data binding.

            // Use the task to signal completion.
            this.fileSelectionTask.Start();
        }

        private Task RequestUserInputCommomPreparation()
        {
            FileName = null;
            ExistingAccountNames = this.accountTypeRepository.ListCurrentlyUsedAccountTypes().ToList();
            SelectedExistingAccountName = ExistingAccountNames.First(a => a.IsSalaryAccount);

            this.dialogCorrelationId = Guid.NewGuid();
            var popRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.Transactions, this, ShellDialogType.OkCancel)
            {
                CorrelationId = this.dialogCorrelationId,
                Title = Title
            };

            this.fileSelectionTask?.Dispose();

            this.fileSelectionTask = new Task(() => { });
            MessengerInstance.Send(popRequest);
            return this.fileSelectionTask;
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
    }
}