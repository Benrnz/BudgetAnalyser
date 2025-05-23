﻿using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;
using Rees.Wpf.Contracts;

namespace BudgetAnalyser.Budget;

[AutoRegisterWithIoC(SingleInstance = true)]
// ReSharper disable once ClassNeverInstantiated.Global
public class BudgetController : ControllerBase, IShowableController
{
    private const string CloseBudgetMenuName = "Close _Budget";
    private const string EditBudgetMenuName = "Edit Current _Budget";
    private readonly IApplicationDatabaseFacade applicationDatabaseService;
    private readonly IBudgetMaintenanceService maintenanceService;
    private readonly IUserMessageBox messageBox;
    private readonly IUserQuestionBoxYesNo questionBox;
    private string budgetMenuItemName = string.Empty;
    private Guid dialogCorrelationId;
    private bool doNotUseDirty;
    private BudgetCurrencyContext? doNotUseModel;
    private bool doNotUseShownBudget;
    private decimal expenseTotal;
    private decimal incomeTotal;

    private bool isLoadingBudgetModel;
    private decimal surplus;

    [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "OnPropertyChange is ok to call here")]
    public BudgetController(
        IUiContext uiContext,
        IBudgetMaintenanceService maintenanceService,
        IApplicationDatabaseFacade applicationDatabaseService)
        : base(uiContext.Messenger)
    {
        if (uiContext is null)
        {
            throw new ArgumentNullException(nameof(uiContext));
        }

        this.maintenanceService = maintenanceService ?? throw new ArgumentNullException(nameof(maintenanceService));
        this.applicationDatabaseService = applicationDatabaseService ?? throw new ArgumentNullException(nameof(applicationDatabaseService));
        this.questionBox = uiContext.UserPrompts.YesNoBox;
        this.messageBox = uiContext.UserPrompts.MessageBox;
        NewBudgetController = uiContext.Controller<NewBudgetModelController>();
        NewBudgetController.Ready += OnAddNewBudgetReady;
        Shown = false;

        Messenger.Register<BudgetController, ShellDialogResponseMessage>(this, static (r, m) => r.OnPopUpResponseReceived(m));
        this.maintenanceService.Closed += OnClosedNotificationReceived;
        this.maintenanceService.NewDataSourceAvailable += OnNewDataSourceAvailableNotificationReceived;
        this.maintenanceService.Saving += OnSavingNotificationReceived;
        this.maintenanceService.Validating += OnValidatingNotificationReceived;
        this.maintenanceService.Saved += OnSavedNotificationReceived;

        CurrentBudget = new BudgetCurrencyContext(this.maintenanceService.Budgets ?? new BudgetCollection(), this.maintenanceService.Budgets?.CurrentActiveBudget ?? new BudgetModel());
    }

    public ICommand AddNewExpenseCommand => new RelayCommand<ExpenseBucket>(OnAddNewExpenseExecute);

    public ICommand AddNewIncomeCommand => new RelayCommand(OnAddNewIncomeExecute);

    public string BudgetMenuItemName
    {
        [UsedImplicitly]
        get => this.budgetMenuItemName;

        set
        {
            if (Equals(value, BudgetMenuItemName))
            {
                return;
            }

            this.budgetMenuItemName = value;
            OnPropertyChanged();
        }
    }

    public BudgetCollection? Budgets { get; private set; }

    // ReSharper disable once MemberCanBePrivate.Global
    public BudgetCurrencyContext? CurrentBudget
    {
        get => this.doNotUseModel;

        private set
        {
            try
            {
                this.isLoadingBudgetModel = true;
                ReleaseListBindingEvents();
                this.doNotUseModel = value;
                if (this.doNotUseModel is null)
                {
                    Incomes = new BindingList<Income>();
                    Expenses = new BindingList<Expense>();
                }
                else
                {
                    SubscribeListBindingEvents();
                }

                OnPropertyChanged(nameof(Incomes));
                OnPropertyChanged(nameof(Expenses));
                OnExpenseAmountPropertyChanged(null, EventArgs.Empty);
                OnIncomeAmountPropertyChanged(null, EventArgs.Empty);
                OnPropertyChanged();
            }
            finally
            {
                this.isLoadingBudgetModel = false;
            }
        }
    }

    public ICommand DeleteBudgetItemCommand => new RelayCommand<object>(OnDeleteBudgetItemCommandExecute);

    public ICommand DetailsCommand => new RelayCommand(OnDetailsCommandExecute);

    // ReSharper disable once MemberCanBePrivate.Global
    public bool Dirty
    {
        get => this.doNotUseDirty;
        private set
        {
            if (value == this.doNotUseDirty)
            {
                return;
            }

            this.doNotUseDirty = value;
            OnPropertyChanged();
            if (Dirty && CurrentBudget is not null)
            {
                CurrentBudget.Model.LastModified = DateTime.Now;
                this.applicationDatabaseService.NotifyOfChange(ApplicationDataType.Budget);
            }
        }
    }

    public BindingList<Expense> Expenses { get; private set; } = new();

    // ReSharper disable once MemberCanBePrivate.Global
    public decimal ExpenseTotal
    {
        get => this.expenseTotal;

        private set
        {
            if (value == this.expenseTotal)
            {
                return;
            }

            this.expenseTotal = value;
            OnPropertyChanged();
        }
    }

    public BindingList<Income> Incomes { get; private set; } = new();

    // ReSharper disable once MemberCanBePrivate.Global
    public decimal IncomeTotal
    {
        get => this.incomeTotal;

        private set
        {
            if (value == this.incomeTotal)
            {
                return;
            }

            this.incomeTotal = value;
            OnPropertyChanged();
        }
    }

    public ICommand NewBudgetCommand => new RelayCommand(OnAddNewBudgetCommandExecuted, () => CurrentBudget is not null);

    // ReSharper disable once MemberCanBePrivate.Global
    public NewBudgetModelController NewBudgetController { get; }

    public ICommand ShowAllCommand => new RelayCommand(OnShowAllCommandExecuted);

    // ReSharper disable once MemberCanBePrivate.Global
    public decimal Surplus
    {
        [UsedImplicitly]
        get => this.surplus;
        private set
        {
            if (value == this.surplus)
            {
                return;
            }

            this.surplus = value;
            OnPropertyChanged();
        }
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public string? TruncatedFileName => Budgets?.StorageKey.TruncateLeft(100, true);

    public bool Shown
    {
        get => this.doNotUseShownBudget;

        set
        {
            if (value == this.doNotUseShownBudget)
            {
                return;
            }

            this.doNotUseShownBudget = value;
            OnPropertyChanged();
            BudgetMenuItemName = this.doNotUseShownBudget ? CloseBudgetMenuName : EditBudgetMenuName;
        }
    }

    private void BudgetModelOnPropertyChanged(object? sender, PropertyChangedEventArgs propertyChangedEventArgs)
    {
        if (this.isLoadingBudgetModel)
        {
            return;
        }

        Dirty = true;
    }

    private void OnAddNewBudgetCommandExecuted()
    {
        if (CurrentBudget is null)
        {
            return;
        }

        var proposedDate = CurrentBudget.Model.EffectiveFrom.AddMonths(1);

        if (proposedDate < DateOnlyExt.Today())
        {
            var differenceInMonths = proposedDate.DurationInMonths(DateOnlyExt.Today());
            proposedDate = proposedDate.AddMonths(differenceInMonths);
        }

        NewBudgetController.ShowDialog(proposedDate);
    }

    private void OnAddNewBudgetReady(object? sender, EventArgs e)
    {
        if (CurrentBudget is null)
        {
            return;
        }

        try
        {
            var budget = this.maintenanceService.CloneBudgetModel(CurrentBudget.Model, NewBudgetController.EffectiveFrom, NewBudgetController.BudgetCycle);
            ShowOtherBudget(budget);
        }
        catch (ArgumentException ex)
        {
            this.messageBox.Show(ex.Message, "Unable to create new budget");
        }
        catch (ValidationWarningException ex)
        {
            this.messageBox.Show(ex.Message, "Unable to create new budget");
        }
    }

    private void OnAddNewExpenseExecute(ExpenseBucket? expense)
    {
        Dirty = true;
        var newExpense = Expenses.AddNew();
        Debug.Assert(newExpense is not null);
        newExpense.Amount = 0;

        // New buckets must be created because the one passed in, is a single command parameter instance to be used as a type indicator only.
        // If it was used, the same instance would overwrite each time an expense is created.
        if (expense is SpentPerPeriodExpenseBucket)
        {
            newExpense.Bucket = new SpentPerPeriodExpenseBucket(string.Empty, string.Empty);
        }
        else
        {
            newExpense.Bucket = expense is SavedUpForExpenseBucket
                ? (BudgetBucket)new SavedUpForExpenseBucket(string.Empty, string.Empty)
                : throw new InvalidCastException("Invalid type passed to Add New Expense: " + expense);
        }

        Expenses.RaiseListChangedEvents = true;
        newExpense.PropertyChanged += OnExpenseAmountPropertyChanged;
    }

    private void OnAddNewIncomeExecute()
    {
        Dirty = true;
        var newIncome = new Income { Bucket = new IncomeBudgetBucket(string.Empty, string.Empty), Amount = 0 };
        Incomes.Add(newIncome);
        newIncome.PropertyChanged += OnIncomeAmountPropertyChanged;
    }

    private void OnClosedNotificationReceived(object? sender, EventArgs eventArgs)
    {
        if (this.maintenanceService.Budgets is null)
        {
            CurrentBudget = null;
            Budgets = null;
        }
        else
        {
            CurrentBudget = new BudgetCurrencyContext(
                this.maintenanceService.Budgets,
                this.maintenanceService.Budgets.CurrentActiveBudget ?? this.maintenanceService.Budgets.ForDate(DateOnlyExt.Today()) ?? this.maintenanceService.Budgets.Last());
            Budgets = CurrentBudget.BudgetCollection;
            BudgetBucketBindingSource.BucketRepository = this.maintenanceService.BudgetBucketRepository;
            Messenger.Send(new BudgetReadyMessage(CurrentBudget, Budgets));
        }

        OnPropertyChanged(nameof(TruncatedFileName));
    }

    private void OnDeleteBudgetItemCommandExecute(object? budgetItem)
    {
        var response = this.questionBox.Show(
            "Are you sure you want to delete this budget bucket?\nAnalysis may not work correctly if transactions are allocated to this bucket.",
            "Delete Budget Bucket");
        if (response is null or false)
        {
            return;
        }

        Dirty = true;
        if (budgetItem is Expense expenseItem)
        {
            expenseItem.PropertyChanged -= OnExpenseAmountPropertyChanged;
            if (expenseItem.Bucket is not null)
            {
                expenseItem.Bucket.PropertyChanged -= OnExpenseAmountPropertyChanged;
            }

            Expenses.Remove(expenseItem);
            return;
        }

        if (budgetItem is Income incomeItem)
        {
            incomeItem.PropertyChanged -= OnIncomeAmountPropertyChanged;
            if (incomeItem.Bucket is not null)
            {
                incomeItem.Bucket.PropertyChanged -= OnIncomeAmountPropertyChanged;
            }

            Incomes.Remove(incomeItem);
        }
    }

    private void OnDetailsCommandExecute()
    {
        if (CurrentBudget is null)
        {
            return;
        }

        var popUpRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.Budget, CurrentBudget, ShellDialogType.Ok);
        Messenger.Send(popUpRequest);
    }

    private void OnExpenseAmountPropertyChanged(object? sender, EventArgs propertyChangedEventArgs)
    {
        if (!this.isLoadingBudgetModel && ExpenseTotal != 0)
        {
            Dirty = true;
        }

        ExpenseTotal = Expenses.Sum(x => x.Amount);
        Surplus = IncomeTotal - ExpenseTotal;
    }

    private void OnIncomeAmountPropertyChanged(object? sender, EventArgs propertyChangedEventArgs)
    {
        if (!this.isLoadingBudgetModel && IncomeTotal != 0)
        {
            Dirty = true;
        }

        IncomeTotal = Incomes.Sum(x => x.Amount);
        Surplus = IncomeTotal - ExpenseTotal;
    }

    private void OnNewDataSourceAvailableNotificationReceived(object? sender, EventArgs eventArgs)
    {
        SyncDataFromBudgetService();
    }

    private void OnPopUpResponseReceived(ShellDialogResponseMessage message)
    {
        if (!message.IsItForMe(this.dialogCorrelationId))
        {
            return;
        }

        var viewModel = (BudgetSelectionViewModel)message.Content;
        if (CurrentBudget is null || viewModel.Selected == CurrentBudget.Model || viewModel.Selected is null)
        {
            return;
        }

        ShowOtherBudget(viewModel.Selected);
    }

    private void OnSavedNotificationReceived(object? sender, EventArgs eventArgs)
    {
        OnPropertyChanged(nameof(TruncatedFileName));
        Dirty = false;
    }

    private void OnSavingNotificationReceived(object? sender, ValidatingEventArgs args)
    {
        SyncDataToBudgetService();
    }

    private void OnShowAllCommandExecuted()
    {
        SelectOtherBudget();
    }

    private void OnValidatingNotificationReceived(object? sender, ValidatingEventArgs eventArgs)
    {
        if (Dirty)
        {
            SyncDataToBudgetService();
        }
    }

    private void ReleaseListBindingEvents()
    {
        if (CurrentBudget is null)
        {
            return;
        }

        CurrentBudget.Model.PropertyChanged -= BudgetModelOnPropertyChanged;
        foreach (var item in Incomes)
        {
            item.PropertyChanged -= OnIncomeAmountPropertyChanged;
            if (item.Bucket is not null)
            {
                item.Bucket.PropertyChanged -= OnIncomeAmountPropertyChanged;
            }
        }

        foreach (var item in Expenses)
        {
            item.PropertyChanged -= OnExpenseAmountPropertyChanged;
            if (item.Bucket is not null)
            {
                item.Bucket.PropertyChanged -= OnExpenseAmountPropertyChanged;
            }
        }
    }

    private void SelectOtherBudget()
    {
        if (Budgets is null)
        {
            return;
        }

        this.dialogCorrelationId = Guid.NewGuid();
        var popUpRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.Budget, new BudgetSelectionViewModel(Budgets), ShellDialogType.Ok) { CorrelationId = this.dialogCorrelationId };
        Messenger.Send(popUpRequest);
    }

    private void ShowOtherBudget(BudgetModel budgetToShow)
    {
        if (Budgets is null)
        {
            return;
        }

        CurrentBudget = new BudgetCurrencyContext(Budgets, budgetToShow);
        Shown = true;
        Dirty = false; // Need to reset this because events fire needlessly (in this case) as a result of setting the CurrentBudget.
    }

    private void SubscribeListBindingEvents()
    {
        if (CurrentBudget is null)
        {
            return;
        }

        CurrentBudget.Model.PropertyChanged += BudgetModelOnPropertyChanged;
        Incomes = new BindingList<Income>(CurrentBudget.Model.Incomes.ToList());
        Incomes.ToList().ForEach(
            i =>
            {
                i.PropertyChanged += OnIncomeAmountPropertyChanged;
                if (i.Bucket is not null)
                {
                    i.Bucket.PropertyChanged += OnIncomeAmountPropertyChanged;
                }
            });
        Expenses = new BindingList<Expense>(CurrentBudget.Model.Expenses.ToList());
        Expenses.ToList().ForEach(
            e =>
            {
                e.PropertyChanged += OnExpenseAmountPropertyChanged;
                if (e.Bucket is not null)
                {
                    e.Bucket.PropertyChanged += OnExpenseAmountPropertyChanged;
                }
            });
    }

    private void SyncDataFromBudgetService()
    {
        Budgets = this.maintenanceService.Budgets;
        if (Budgets is null)
        {
            var budget = new BudgetModel();
            CurrentBudget = new BudgetCurrencyContext(new BudgetCollection(budget), budget);
        }
        else
        {
            CurrentBudget = new BudgetCurrencyContext(Budgets, Budgets.CurrentActiveBudget ?? Budgets.Last());
        }

        BudgetBucketBindingSource.BucketRepository = this.maintenanceService.BudgetBucketRepository;
        OnPropertyChanged(nameof(TruncatedFileName));
        if (CurrentBudget is not null)
        {
            Messenger.Send(new BudgetReadyMessage(CurrentBudget, Budgets));
        }
    }

    private void SyncDataToBudgetService()
    {
        if (CurrentBudget is null)
        {
            return;
        }

        this.maintenanceService.UpdateIncomesAndExpenses(CurrentBudget.Model, Incomes, Expenses);
    }
}
