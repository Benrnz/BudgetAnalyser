using System.Windows.Input;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Statement;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;
using Rees.Wpf.Contracts;

namespace BudgetAnalyser.LedgerBook;

[AutoRegisterWithIoC(SingleInstance = true)]
public class LedgerBookController : ControllerBase, IShowableController
{
    private readonly IUserInputBox inputBox;
    private readonly ILedgerService ledgerService;
    private readonly IUserMessageBox messageBox;
    private readonly NewWindowViewLoader newWindowViewLoader;
    private readonly IUserQuestionBoxYesNo questionBox;
    private readonly IReconciliationService reconService;
    private readonly LedgerBookGridBuilderFactory uiBuilder;
    private readonly UiContext uiContext;
    private int doNotUseNumberOfPeriodsToShow;
    private bool doNotUseShown;

    public LedgerBookController(
        [NotNull] UiContext uiContext,
        [NotNull] LedgerBookControllerFileOperations fileOperations,
        [NotNull] LedgerBookGridBuilderFactory uiBuilder,
        [NotNull] ILedgerService ledgerService,
        [NotNull] IReconciliationService reconService,
        [NotNull] NewWindowViewLoader newWindowViewLoader)
        : base(uiContext.Messenger)
    {
        if (uiContext == null)
        {
            throw new ArgumentNullException(nameof(uiContext));
        }

        this.uiBuilder = uiBuilder ?? throw new ArgumentNullException(nameof(uiBuilder));
        this.ledgerService = ledgerService ?? throw new ArgumentNullException(nameof(ledgerService));
        this.reconService = reconService ?? throw new ArgumentNullException(nameof(reconService));
        this.newWindowViewLoader = newWindowViewLoader ?? throw new ArgumentNullException(nameof(newWindowViewLoader));
        this.messageBox = uiContext.UserPrompts.MessageBox;
        this.questionBox = uiContext.UserPrompts.YesNoBox;
        this.inputBox = uiContext.UserPrompts.InputBox;
        FileOperations = fileOperations ?? throw new ArgumentNullException(nameof(fileOperations));
        FileOperations.LedgerService = this.ledgerService;
        this.uiContext = uiContext;
        this.doNotUseNumberOfPeriodsToShow = 6;

        Messenger.Register<LedgerBookController, BudgetReadyMessage>(this, static (r, m) => r.OnBudgetReadyMessageReceived(m));
        Messenger.Register<LedgerBookController, StatementReadyMessage>(this, static (r, m) => r.OnStatementReadyMessageReceived(m));

        this.ledgerService.Saved += OnSaveNotificationReceived;
        this.ledgerService.Closed += OnClosedNotificationReceived;
        this.ledgerService.NewDataSourceAvailable += OnNewDataSourceAvailableNotificationReceived;
    }

    public event EventHandler LedgerBookUpdated;

    public LedgerBookControllerFileOperations FileOperations { get; }

    public int NumberOfPeriodsToShow
    {
        get => this.doNotUseNumberOfPeriodsToShow;
        set
        {
            if (value == this.doNotUseNumberOfPeriodsToShow) return;
            this.doNotUseNumberOfPeriodsToShow = value;
            OnPropertyChanged();
        }
    }

    public ICommand ShowBankBalancesCommand => new RelayCommand<LedgerEntryLine>(OnShowBankBalancesCommandExecuted, param => param != null);
    public ICommand ShowHidePeriodsCommand => new RelayCommand<int>(OnShowHidePeriodsCommandExecuted);
    public ICommand ShowLedgerBucketDetailsCommand => new RelayCommand<LedgerBucket>(OnShowLedgerBucketDetailsCommand, param => param != null);

    public ICommand ShowRemarksCommand => new RelayCommand<LedgerEntryLine>(OnShowRemarksCommandExecuted, CanExecuteShowRemarksCommand);
    public ICommand ShowSurplusBalancesCommand => new RelayCommand<LedgerEntryLine>(OnShowSurplusBalancesCommandExecuted, param => param != null);
    public ICommand ShowTransactionsCommand => new RelayCommand<object>(OnShowTransactionsCommandExecuted);
    public ReconciliationToDoListController ToDoListController => this.uiContext.ReconciliationToDoListController;

    public LedgerBookViewModel ViewModel => FileOperations.ViewModel;

    public bool Shown
    {
        get => this.doNotUseShown;

        set
        {
            if (value == this.doNotUseShown)
            {
                return;
            }

            this.doNotUseShown = value;
            OnPropertyChanged();
        }
    }

    public void DeregisterListener(LedgerBookUserControl recipient)
    {
        Messenger.Unregister<LedgerBookReadyMessage>(recipient);
    }

    public void EditLedgerBookName()
    {
        if (ViewModel.LedgerBook == null)
        {
            return;
        }

        var result = this.inputBox.Show("Edit Ledger Book Name", "Enter a new name", ViewModel.LedgerBook.Name);
        if (string.IsNullOrWhiteSpace(result))
        {
            return;
        }

        this.ledgerService.RenameLedgerBook(result);
        FileOperations.Dirty = true;
    }

    public void OnAddNewLedgerCommandExecuted()
    {
        // TODO Change this to an event.
        this.uiContext.ChooseBudgetBucketController.Chosen += OnAddNewLedgerComplete;
        this.uiContext.ChooseBudgetBucketController.Filter(bucket => bucket is ExpenseBucket, "Choose an Expense Budget Bucket");
        this.uiContext.ChooseBudgetBucketController.ShowDialog(BudgetAnalyserFeature.LedgerBook, "Add New Ledger to Ledger Book", Guid.NewGuid(), true);
    }

    public void OnAddNewReconciliationCommandExecuted()
    {
        try
        {
            this.reconService.BeforeReconciliationValidation(ViewModel.LedgerBook, ViewModel.CurrentStatement);
        }
        catch (ValidationWarningException ex)
        {
            if (!ProceedAfterReconciliationValidationWarning(ex))
            {
                return;
            }
        }

        // TODO Change this to an event.
        this.uiContext.AddLedgerReconciliationController.Complete += OnAddReconciliationDialogClose;
        this.uiContext.AddLedgerReconciliationController.ShowCreateDialog(ViewModel.LedgerBook);
    }

    public void OnTransferFundsCommandExecuted()
    {
        // TODO Change this to a message
        this.uiContext.TransferFundsController.TransferFundsRequested += OnTransferFundsRequested;
        this.uiContext.TransferFundsController.ShowDialog(ViewModel.LedgerBook.LedgersAvailableForTransfer());
    }

    public void OnUnlockLedgerLineCommandExecuted()
    {
        var response = this.questionBox.Show(
            "Unlock Ledger Entry Line",
            "Are you sure you want to unlock the Ledger Entry Line dated {0:d} for editing?",
            ViewModel.LedgerBook.Reconciliations.First().Date);

        if (response == null || response.Value == false)
        {
            return;
        }

        ViewModel.NewLedgerLine = this.reconService.UnlockCurrentPeriod(ViewModel.LedgerBook);
        FileOperations.Dirty = true;
    }

    public void RegisterListener(LedgerBookUserControl recipient, MessageHandler<LedgerBookUserControl, LedgerBookReadyMessage> handler)
    {
        Messenger.Register(recipient, handler);
    }

    internal ILedgerBookGridBuilder GridBuilder()
    {
        return this.uiBuilder.GridBuilderV2(this);
    }

    private bool CanExecuteShowRemarksCommand(LedgerEntryLine parameter)
    {
        return parameter != null
               && (!string.IsNullOrWhiteSpace(parameter.Remarks) || parameter == ViewModel.NewLedgerLine);
    }

    private void CreatePeriodEndReconciliation(bool ignoreWarnings = false)
    {
        try
        {
            var reconciliationDate = this.uiContext.AddLedgerReconciliationController.Date;
            var budgetCollection = this.uiContext.BudgetController.Budgets;
            ViewModel.NewLedgerLine = this.reconService.PeriodEndReconciliation(ViewModel.LedgerBook,
                reconciliationDate,
                budgetCollection,
                ViewModel.CurrentStatement,
                ignoreWarnings,
                this.uiContext.AddLedgerReconciliationController.BankBalances.Cast<BankBalance>().ToArray());

            FileOperations.ReconciliationChangesWillNeedToBeSaved();
            NumberOfPeriodsToShow++;
            RaiseLedgerBookUpdated();
            if (this.reconService.ReconciliationToDoList.Any())
            {
                ToDoListController.Load(this.reconService.ReconciliationToDoList);
                this.newWindowViewLoader.Show(ToDoListController);
            }
        }
        catch (ValidationWarningException ex)
        {
            if (!ProceedAfterReconciliationValidationWarning(ex))
            {
                return;
            }

            CreatePeriodEndReconciliation(true);
        }
        catch (InvalidOperationException ex)
        {
            this.messageBox.Show(ex, "Unable to add new reconciliation.");
        }
    }

    private void OnAddNewLedgerComplete(object sender, BudgetBucketChosenEventArgs e)
    {
        this.uiContext.ChooseBudgetBucketController.Chosen -= OnAddNewLedgerComplete;
        if (e.Canceled)
        {
            return;
        }

        var expenseBucket = e.SelectedBucket as ExpenseBucket;
        if (expenseBucket == null)
        {
            this.messageBox.Show("You must select an expense budget bucket to track when adding a new Ledger Column.");
            return;
        }

        var newBucket = this.ledgerService.TrackNewBudgetBucket(expenseBucket, e.StoreInThisAccount);
        if (newBucket == null)
        {
            this.uiContext.UserPrompts.MessageBox.Show("Leader Bucket is already tracked in the LedgerBook. If its not shown, there are no transactions for that bucket yet.", "LedgerBook");
        }
        else
        {
            this.uiContext.UserPrompts.MessageBox.Show("Ledger Bucket added successfully to the LedgerBook. It will be tracked and shown only when there are new transactions added for that Bucket.",
                "LedgerBook");
        }
    }

    private void OnAddReconciliationDialogClose(object sender, EditBankBalancesEventArgs e)
    {
        this.uiContext.AddLedgerReconciliationController.Complete -= OnAddReconciliationDialogClose;

        if (e.Canceled)
        {
            return;
        }

        CreatePeriodEndReconciliation();
    }

    private void OnBudgetReadyMessageReceived(BudgetReadyMessage message)
    {
        // CurrentBudget is not used for reconciliation purposes, for recon purposes this needs to find the effective budget for the recon date, NOT the current budget.
        // CurrentBudget should only be used for UI purposes such as an indication of current budgeted amount for something etc. 
        if (message.ActiveBudget == null)
        {
            ViewModel.CurrentBudget = null;
            return;
        }

        if (message.ActiveBudget.BudgetActive)
        {
            ViewModel.CurrentBudget = message.ActiveBudget;
        }
    }

    private void OnClosedNotificationReceived(object sender, EventArgs eventArgs)
    {
        FileOperations.Close();
    }

    private void OnLedgerBucketUpdated(object sender, EventArgs e)
    {
        this.uiContext.LedgerBucketViewController.Updated -= OnLedgerBucketUpdated;
        RaiseLedgerBookUpdated();
        FileOperations.Dirty = true;
    }

    private void OnNewDataSourceAvailableNotificationReceived(object sender, EventArgs eventArgs)
    {
        FileOperations.SyncDataFromLedgerService();
    }

    private void OnSaveNotificationReceived(object sender, EventArgs eventArgs)
    {
        ViewModel.NewLedgerLine = null;
    }

    private void OnShowBankBalancesCommandExecuted(LedgerEntryLine line)
    {
        this.uiContext.AddLedgerReconciliationController.ShowViewDialog(ViewModel.LedgerBook, line);
    }

    private void OnShowHidePeriodsCommandExecuted(int increment)
    {
        NumberOfPeriodsToShow += increment;
        Messenger.Send(new LedgerBookReadyMessage(ViewModel.LedgerBook) { ForceUiRefresh = true });
    }

    private void OnShowLedgerBucketDetailsCommand(LedgerBucket ledgerBucket)
    {
        this.uiContext.LedgerBucketViewController.Updated += OnLedgerBucketUpdated;
        this.uiContext.LedgerBucketViewController.ShowDialog(ViewModel.LedgerBook, ledgerBucket, ViewModel.CurrentBudget.Model);
    }

    private void OnShowRemarksCommandExecuted(LedgerEntryLine parameter)
    {
        this.uiContext.LedgerRemarksController.Completed += OnShowRemarksCompleted;
        this.uiContext.LedgerRemarksController.Show(parameter, parameter == ViewModel.NewLedgerLine);
    }

    private void OnShowRemarksCompleted(object sender, EventArgs e)
    {
        this.uiContext.LedgerRemarksController.Completed -= OnShowRemarksCompleted;
    }

    private void OnShowSurplusBalancesCommandExecuted(LedgerEntryLine line)
    {
        this.uiContext.ShowSurplusBalancesController.ShowDialog(line);
    }

    private void OnShowTransactionsCommandExecuted(object parameter)
    {
        if (parameter == null)
        {
            return;
        }

        this.uiContext.LedgerTransactionsController.Complete += OnShowTransactionsCompleted;

        if (parameter is LedgerEntry ledgerEntry)
        {
            var isNew = ViewModel.NewLedgerLine != null && ViewModel.NewLedgerLine.Entries.Any(e => e == ledgerEntry);
            this.uiContext.LedgerTransactionsController.ShowLedgerTransactionsDialog(ViewModel.NewLedgerLine, ledgerEntry, isNew);
        }
        else if (parameter is LedgerEntryLine bankBalanceAdjustments)
        {
            this.uiContext.LedgerTransactionsController.ShowBankBalanceAdjustmentsDialog(bankBalanceAdjustments, bankBalanceAdjustments == ViewModel.NewLedgerLine);
        }
        else
        {
            throw new ArgumentException("Invalid parameter passed to ShowTransactionsCommand: " + parameter);
        }
    }

    private void OnShowTransactionsCompleted(object sender, LedgerTransactionEventArgs args)
    {
        this.uiContext.LedgerTransactionsController.Complete -= OnShowTransactionsCompleted;

        if (args.WasModified)
        {
            RaiseLedgerBookUpdated();
        }
    }

    private void OnStatementReadyMessageReceived(StatementReadyMessage message)
    {
        ViewModel.CurrentStatement = message.StatementModel;
    }

    private void OnTransferFundsRequested(object sender, EventArgs eventArgs)
    {
        this.uiContext.TransferFundsController.TransferFundsRequested -= OnTransferFundsRequested;
        this.reconService.TransferFunds(ViewModel.LedgerBook, ViewModel.NewLedgerLine, this.uiContext.TransferFundsController.TransferFundsDto);
        RaiseLedgerBookUpdated();
        FileOperations.ReconciliationChangesWillNeedToBeSaved();
    }

    private bool ProceedAfterReconciliationValidationWarning(ValidationWarningException ex)
    {
        var result = this.questionBox.Show("Warning", "Warning: {0}\nDo you wish to proceed?", ex.Message);
        return result != null && result.Value;
    }

    private void RaiseLedgerBookUpdated()
    {
        var handler = LedgerBookUpdated;
        handler?.Invoke(this, EventArgs.Empty);
    }
}