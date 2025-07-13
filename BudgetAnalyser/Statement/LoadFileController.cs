using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Windows.Input;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;
using Rees.Wpf.Contracts;

namespace BudgetAnalyser.Statement;

[AutoRegisterWithIoC(SingleInstance = true)]
public class LoadFileController : ControllerBase, IShellDialogInteractivity, IShellDialogToolTips, IDisposable
{
    private readonly IAccountTypeRepository accountTypeRepository;
    private readonly IUserMessageBox messageBox;
    private readonly Func<IUserPromptOpenFile> userPromptOpenFileFactory;
    private Guid dialogCorrelationId;
    private bool disposed;
    private bool doNotUseCanExecuteOkButton;
    private string? doNotUseFileName;
    private bool doNotUseFileTypeSelectionReady;
    private Account? doNotUseSelectedExistingAccountName;
    private string doNotUseTitle = string.Empty;
    private Task fileSelectionTask = Task.CompletedTask;
    private bool showingDialog;

    [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "OnPropertyChange is ok to call here")]
    public LoadFileController(IUiContext uiContext, IAccountTypeRepository accountTypeRepository) : base(uiContext.Messenger)
    {
        if (uiContext is null)
        {
            throw new ArgumentNullException(nameof(uiContext));
        }

        this.messageBox = uiContext.UserPrompts.MessageBox;
        this.userPromptOpenFileFactory = uiContext.UserPrompts.OpenFileFactory;
        this.accountTypeRepository = accountTypeRepository ?? throw new ArgumentNullException(nameof(accountTypeRepository));

        Messenger.Register<LoadFileController, ShellDialogResponseMessage>(this, static (r, m) => r.OnShellDialogResponseReceived(m));
    }

    [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Used by data binding")]
    public string AccountNameHelp => "When importing a new bank statement file, you must select the account the statement comes from.\nThis allows merging of multiple accounts into one file.";

    public ICommand BrowseForFileCommand => new RelayCommand(OnBrowseForFileCommandExecute);
    public IEnumerable<Account> ExistingAccountNames { get; private set; } = Array.Empty<Account>();

    public string? FileName
    {
        get => this.doNotUseFileName;

        set
        {
            if (value == this.doNotUseFileName)
            {
                return;
            }

            this.doNotUseFileName = value;
            OnPropertyChanged();
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
        get => this.doNotUseFileTypeSelectionReady;
        private set
        {
            if (value == this.doNotUseFileTypeSelectionReady)
            {
                return;
            }

            this.doNotUseFileTypeSelectionReady = value;
            OnPropertyChanged();
        }
    }

    public bool MergeMode
    {
        get;
        private set;
    }

    public Account? SelectedExistingAccountName
    {
        get => this.doNotUseSelectedExistingAccountName;

        set
        {
            if (Equals(value, this.doNotUseSelectedExistingAccountName))
            {
                return;
            }

            this.doNotUseSelectedExistingAccountName = value;
            OnPropertyChanged();
            CheckAccountName();
            CheckFileName();
        }
    }

    public string SuggestedDateRange { get; private set; } = string.Empty;

    public string Title
    {
        get => this.doNotUseTitle;
        private set
        {
            if (value == this.doNotUseTitle)
            {
                return;
            }

            this.doNotUseTitle = value;
            OnPropertyChanged();
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

    public bool CanExecuteCancelButton => true;

    public bool CanExecuteOkButton
    {
        get => this.doNotUseCanExecuteOkButton;
        private set
        {
            if (value == this.doNotUseCanExecuteOkButton)
            {
                return;
            }

            this.doNotUseCanExecuteOkButton = value;
            OnPropertyChanged();
            Messenger.Send<ShellDialogCommandRequerySuggestedMessage>();
        }
    }

    public bool CanExecuteSaveButton => false;

    public string ActionButtonToolTip { get; private set; } = string.Empty;

    public string CloseButtonToolTip => "Cancel";

    public Task RequestUserInputForMerging(TransactionSetModel currentTransactionSet)
    {
        if (this.disposed)
        {
            throw new ObjectDisposedException("LoadFileController.RequestUserInputForMerging");
        }

        MergeMode = true;
        SuggestedDateRange = string.Empty;
        Title = "Merge Statement";
        ActionButtonToolTip = "Merge transactions from the selected file into the current statement file.";
        CalculateSuggestedDateRange(currentTransactionSet);

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

    private void CalculateSuggestedDateRange(TransactionSetModel currentTransactionSet)
    {
        var lastTransactionDate = currentTransactionSet.AllTransactions.Max(t => t.Date).AddDays(1);
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

        if (SelectedExistingAccountName is null)
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
                this.fileSelectionTask.Dispose();
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
            var result = dialog.ShowDialog();
            if (result is null || result == false)
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
        var popRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.Transactions, this, ShellDialogType.OkCancel) { CorrelationId = this.dialogCorrelationId, Title = Title };

        this.fileSelectionTask.Dispose();

        this.fileSelectionTask = new Task(() => { });
        Messenger.Send(popRequest);
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
