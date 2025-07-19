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
// ReSharper disable once ClassNeverInstantiated.Global // Instantiated by IoC Container
public class TabLedgerBookController : ControllerBase, IShowableController
{
    private readonly AddLedgerReconciliationController addLedgerReconciliationController;
    private readonly ChooseBudgetBucketController chooseBudgetBucketController;
    private readonly IUserInputBox inputBox;
    private readonly LedgerBucketViewController ledgerBucketViewController;
    private readonly LedgerRemarksController ledgerRemarksController;
    private readonly ILedgerService ledgerService;
    private readonly LedgerTransactionsController ledgerTransactionsController;
    private readonly IUserMessageBox messageBox;
    private readonly NewWindowViewLoader newWindowViewLoader;
    private readonly IUserQuestionBoxYesNo questionBox;
    private readonly IReconciliationService reconService;
    private readonly ShowSurplusBalancesController showSurplusBalancesController;
    private readonly TransferFundsController transferFundsController;
    private readonly LedgerBookGridBuilderFactory uiBuilder;
    private readonly IUiContext uiContext;
    private int doNotUseNumberOfPeriodsToShow;
    private bool doNotUseShown;

    public TabLedgerBookController(
        IUiContext uiContext,
        LedgerBookControllerFileOperations fileOperations,
        LedgerBookGridBuilderFactory uiBuilder,
        ILedgerService ledgerService,
        IReconciliationService reconService,
        NewWindowViewLoader newWindowViewLoader)
        : base(uiContext.Messenger)
    {
        this.uiContext = uiContext ?? throw new ArgumentNullException(nameof(uiContext));
        this.uiBuilder = uiBuilder ?? throw new ArgumentNullException(nameof(uiBuilder));
        this.ledgerService = ledgerService ?? throw new ArgumentNullException(nameof(ledgerService));
        this.reconService = reconService ?? throw new ArgumentNullException(nameof(reconService));
        this.newWindowViewLoader = newWindowViewLoader ?? throw new ArgumentNullException(nameof(newWindowViewLoader));
        FileOperations = fileOperations ?? throw new ArgumentNullException(nameof(fileOperations));

        this.messageBox = uiContext.UserPrompts.MessageBox;
        this.questionBox = uiContext.UserPrompts.YesNoBox;
        this.inputBox = uiContext.UserPrompts.InputBox;
        FileOperations.LedgerService = this.ledgerService;
        this.doNotUseNumberOfPeriodsToShow = 6;
        this.addLedgerReconciliationController = uiContext.Controller<AddLedgerReconciliationController>();
        this.chooseBudgetBucketController = uiContext.Controller<ChooseBudgetBucketController>();
        this.ledgerBucketViewController = uiContext.Controller<LedgerBucketViewController>();
        this.ledgerRemarksController = uiContext.Controller<LedgerRemarksController>();
        this.ledgerTransactionsController = uiContext.Controller<LedgerTransactionsController>();
        this.showSurplusBalancesController = uiContext.Controller<ShowSurplusBalancesController>();
        this.transferFundsController = uiContext.Controller<TransferFundsController>();

        Messenger.Register<TabLedgerBookController, BudgetReadyMessage>(this, static (r, m) => r.OnBudgetReadyMessageReceived(m));
        Messenger.Register<TabLedgerBookController, TransactionSetModelReadyMessage>(this, static (r, m) => r.OnTransactionsReadyMessageReceived(m));
        Messenger.Register<TabLedgerBookController, LedgerBookReadyMessage>(this, (_, _) => ShowRemarksCommand.NotifyCanExecuteChanged());
        Messenger.Register<TabLedgerBookController, LedgerBucketTransferCommandMessage>(this, static (r, m) => r.OnTransferFundsCommandReceived(m));

        this.ledgerService.Saved += OnSaveNotificationReceived;
        this.ledgerService.Closed += OnClosedNotificationReceived;
        this.ledgerService.NewDataSourceAvailable += OnNewDataSourceAvailableNotificationReceived;
    }

    public event EventHandler? LedgerBookUpdated;

    public LedgerBookControllerFileOperations FileOperations { get; }

    public int NumberOfPeriodsToShow
    {
        get => this.doNotUseNumberOfPeriodsToShow;
        set
        {
            if (value == this.doNotUseNumberOfPeriodsToShow)
            {
                return;
            }

            this.doNotUseNumberOfPeriodsToShow = value;
            OnPropertyChanged();
        }
    }

    public ICommand ShowBankBalancesCommand => new RelayCommand<LedgerEntryLine>(OnShowBankBalancesCommandExecuted, param => param is not null);
    public ICommand ShowHidePeriodsCommand => new RelayCommand<int>(OnShowHidePeriodsCommandExecuted);
    public ICommand ShowLedgerBucketDetailsCommand => new RelayCommand<LedgerBucket>(OnShowLedgerBucketDetailsCommand, param => param is not null);

    public RelayCommand<LedgerEntryLine> ShowRemarksCommand => new(OnShowRemarksCommandExecuted, CanExecuteShowRemarksCommand);
    public ICommand ShowSurplusBalancesCommand => new RelayCommand<LedgerEntryLine>(OnShowSurplusBalancesCommandExecuted, param => param is not null);
    public ICommand ShowTransactionsCommand => new RelayCommand<object>(OnShowTransactionsCommandExecuted);
    public ReconciliationToDoListController ToDoListController => this.uiContext.Controller<ReconciliationToDoListController>();

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
        if (ViewModel.LedgerBook is null)
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
        this.chooseBudgetBucketController.Chosen += OnAddNewLedgerComplete;
        this.chooseBudgetBucketController.Filter(bucket => bucket is ExpenseBucket, "Choose an Expense Budget Bucket");
        this.chooseBudgetBucketController.ShowDialog(BudgetAnalyserFeature.LedgerBook, "Add New Ledger to Ledger Book", Guid.NewGuid(), true);
    }

    public void OnAddNewReconciliationCommandExecuted()
    {
        try
        {
            this.reconService.BeforeReconciliationValidation(ViewModel.LedgerBook!, ViewModel.CurrentTransactionSet!);
        }
        catch (ValidationWarningException ex)
        {
            if (!ProceedAfterReconciliationValidationWarning(ex))
            {
                return;
            }
        }

        this.addLedgerReconciliationController.Complete += OnAddReconciliationDialogClose;
        this.addLedgerReconciliationController.ShowCreateDialog(ViewModel.LedgerBook!);
    }

    public void OnTransferFundsInitiated()
    {
        if (ViewModel.NewLedgerLine is null)
        {
            return;
        }

        this.transferFundsController.ShowDialog(ViewModel.LedgerBook!.LedgersAvailableForTransfer(), ViewModel.NewLedgerLine);
    }

    public void OnUnlockLedgerLineCommandExecuted()
    {
        if (ViewModel.LedgerBook is null)
        {
            return;
        }

        var response = this.questionBox.Show(
            "Unlock Ledger Entry Line",
            "Are you sure you want to unlock the Ledger Entry Line dated {0:d} for editing?",
            ViewModel.LedgerBook.Reconciliations.First().Date);

        if (response is null or false)
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

    private bool CanExecuteShowRemarksCommand(LedgerEntryLine? parameter)
    {
        return parameter is not null && (!string.IsNullOrWhiteSpace(parameter.Remarks) || parameter == ViewModel.NewLedgerLine);
    }

    private void CreatePeriodEndReconciliation(bool ignoreWarnings = false)
    {
        try
        {
            var reconciliationDate = this.addLedgerReconciliationController.Date;
            var budgetCollection = this.uiContext.Controller<TabBudgetController>().Budgets ?? throw new InvalidOperationException("Budget collection is null.");
            ViewModel.NewLedgerLine = this.reconService.PeriodEndReconciliation(ViewModel.LedgerBook!,
                reconciliationDate,
                budgetCollection,
                ViewModel.CurrentTransactionSet!,
                ignoreWarnings,
                this.addLedgerReconciliationController.BankBalances.Cast<BankBalance>().ToArray());

            FileOperations.ReconciliationChangesWillNeedToBeSaved();
            NumberOfPeriodsToShow++;
            RaiseLedgerBookUpdated();
            if (this.reconService.ReconciliationToDoList.Any())
            {
                ToDoListController.Load(this.reconService.ReconciliationToDoList);
                this.newWindowViewLoader.Show(ToDoListController);
            }

            ShowRemarksCommand.NotifyCanExecuteChanged();
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

    private void OnAddNewLedgerComplete(object? sender, BudgetBucketChosenEventArgs? e)
    {
        this.chooseBudgetBucketController.Chosen -= OnAddNewLedgerComplete;
        if (e is null || e.Canceled)
        {
            return;
        }

        if (e.SelectedBucket is not ExpenseBucket expenseBucket)
        {
            this.messageBox.Show("You must select an expense budget bucket to track when adding a new Ledger Column.");
            return;
        }

        if (e.StoreInThisAccount is null)
        {
            this.messageBox.Show("You must select an account to track the new Ledger Column.");
            return;
        }

        this.ledgerService.TrackNewBudgetBucket(expenseBucket, e.StoreInThisAccount);
        this.uiContext.UserPrompts.MessageBox.Show(
            "Ledger Bucket added successfully to the LedgerBook. It will be tracked and shown only when there are new transactions added for that Bucket.",
            "LedgerBook");
        FileOperations.Dirty = true;
    }

    private void OnAddReconciliationDialogClose(object? sender, EditBankBalancesEventArgs? e)
    {
        this.addLedgerReconciliationController.Complete -= OnAddReconciliationDialogClose;

        if (e is null || e.Canceled)
        {
            return;
        }

        CreatePeriodEndReconciliation();
    }

    private void OnBudgetReadyMessageReceived(BudgetReadyMessage message)
    {
        // CurrentBudget is not used for reconciliation purposes, for recon purposes this needs to find the effective budget for the recon date, NOT the current budget.
        // CurrentBudget should only be used for UI purposes such as an indication of current budgeted amount for something etc.
        if (message.ActiveBudget.BudgetActive)
        {
            ViewModel.CurrentBudget = message.ActiveBudget;
        }
    }

    private void OnClosedNotificationReceived(object? sender, EventArgs? eventArgs)
    {
        FileOperations.Close();
    }

    private void OnLedgerBucketUpdated(object? sender, EventArgs? e)
    {
        this.ledgerBucketViewController.Updated -= OnLedgerBucketUpdated;
        RaiseLedgerBookUpdated();
        FileOperations.Dirty = true;
    }

    private void OnNewDataSourceAvailableNotificationReceived(object? sender, EventArgs? eventArgs)
    {
        FileOperations.SyncDataFromLedgerService();
    }

    private void OnSaveNotificationReceived(object? sender, EventArgs? eventArgs)
    {
        ViewModel.NewLedgerLine = null;
    }

    private void OnShowBankBalancesCommandExecuted(LedgerEntryLine? line)
    {
        if (line is null)
        {
            throw new ArgumentNullException(nameof(line), "Binding problem, command executed without required ledger entry line parameter.");
        }

        this.addLedgerReconciliationController.ShowViewDialog(ViewModel.LedgerBook!, line);
    }

    private void OnShowHidePeriodsCommandExecuted(int increment)
    {
        NumberOfPeriodsToShow += increment;
        Messenger.Send(new LedgerBookReadyMessage(ViewModel.LedgerBook!) { ForceUiRefresh = true });
    }

    private void OnShowLedgerBucketDetailsCommand(LedgerBucket? ledgerBucket)
    {
        if (ledgerBucket is null)
        {
            throw new ArgumentNullException(nameof(ledgerBucket), "Binding problem, command executed without required ledger bucket parameter.");
        }

        this.ledgerBucketViewController.Updated += OnLedgerBucketUpdated;
        this.ledgerBucketViewController.ShowDialog(ledgerBucket, ViewModel.CurrentBudget!.Model);
    }

    private void OnShowRemarksCommandExecuted(LedgerEntryLine? parameter)
    {
        if (parameter is null)
        {
            throw new ArgumentNullException(nameof(parameter), "Binding problem, command executed without required ledger entry line parameter.");
        }

        this.ledgerRemarksController.Completed += OnShowRemarksCompleted;
        this.ledgerRemarksController.Show(parameter, parameter == ViewModel.NewLedgerLine);
    }

    private void OnShowRemarksCompleted(object? sender, EventArgs? e)
    {
        this.ledgerRemarksController.Completed -= OnShowRemarksCompleted;
    }

    private void OnShowSurplusBalancesCommandExecuted(LedgerEntryLine? line)
    {
        if (line is null)
        {
            throw new ArgumentNullException(nameof(line), "Binding problem, command executed without required ledger entry line parameter.");
        }

        this.showSurplusBalancesController.ShowDialog(line);
    }

    private void OnShowTransactionsCommandExecuted(object? parameter)
    {
        if (parameter is null)
        {
            return;
        }

        this.ledgerTransactionsController.Complete += OnShowTransactionsCompleted;

        if (parameter is LedgerEntry ledgerEntry)
        {
            var isNew = ViewModel.NewLedgerLine is not null && ViewModel.NewLedgerLine.Entries.Any(e => e == ledgerEntry);
            this.ledgerTransactionsController.ShowLedgerTransactionsDialog(ViewModel.NewLedgerLine!, ledgerEntry, isNew);
        }
        else if (parameter is LedgerEntryLine bankBalanceAdjustments)
        {
            this.ledgerTransactionsController.ShowBankBalanceAdjustmentsDialog(bankBalanceAdjustments, bankBalanceAdjustments == ViewModel.NewLedgerLine);
        }
        else
        {
            throw new ArgumentException("Invalid parameter passed to ShowTransactionsCommand: " + parameter);
        }
    }

    private void OnShowTransactionsCompleted(object? sender, LedgerTransactionEventArgs? args)
    {
        this.ledgerTransactionsController.Complete -= OnShowTransactionsCompleted;
        if (args is null)
        {
            return;
        }

        if (args.WasModified)
        {
            RaiseLedgerBookUpdated();
        }
    }

    private void OnTransactionsReadyMessageReceived(TransactionSetModelReadyMessage message)
    {
        ViewModel.CurrentTransactionSet = message.TransactionsSetModel;
    }

    private void OnTransferFundsCommandReceived(LedgerBucketTransferCommandMessage message)
    {
        this.reconService.TransferFunds(ViewModel.LedgerBook!, ViewModel.NewLedgerLine!, message.TransferFundsCommand);
        RaiseLedgerBookUpdated();
        FileOperations.ReconciliationChangesWillNeedToBeSaved();
    }

    private bool ProceedAfterReconciliationValidationWarning(ValidationWarningException ex)
    {
        var result = this.questionBox.Show("Warning", "Warning: {0}\nDo you wish to proceed?", ex.Message);
        return result is not null && result.Value;
    }

    private void RaiseLedgerBookUpdated()
    {
        var handler = LedgerBookUpdated;
        handler?.Invoke(this, EventArgs.Empty);
    }
}
