using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Input;
using BudgetAnalyser.Annotations;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.ShellDialog;
using GalaSoft.MvvmLight.CommandWpf;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;

namespace BudgetAnalyser.Budget
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class BudgetController : ControllerBase, IShowableController
    {
        private const string CloseBudgetMenuName = "Close _Budget";
        private const string EditBudgetMenuName = "Edit Current _Budget";
        private readonly IApplicationDatabaseService applicationDatabaseService;
        private readonly IUserInputBox inputBox;
        private readonly IBudgetMaintenanceService maintenanceService;
        private readonly IUserMessageBox messageBox;
        private readonly IUserQuestionBoxYesNo questionBox;
        private string budgetMenuItemName;
        private Guid dialogCorrelationId;
        private bool doNotUseDirty;
        private BudgetCurrencyContext doNotUseModel;
        private bool doNotUseShownBudget;
        private decimal expenseTotal;
        private decimal incomeTotal;

        private bool isLoadingBudgetModel;
        private decimal surplus;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "OnPropertyChange is ok to call here")]
        public BudgetController(
            [NotNull] UiContext uiContext,
            [NotNull] IBudgetMaintenanceService maintenanceService,
            [NotNull] IApplicationDatabaseService applicationDatabaseService)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException(nameof(uiContext));
            }

            if (maintenanceService == null)
            {
                throw new ArgumentNullException(nameof(maintenanceService));
            }

            if (applicationDatabaseService == null)
            {
                throw new ArgumentNullException(nameof(applicationDatabaseService));
            }

            this.maintenanceService = maintenanceService;
            this.applicationDatabaseService = applicationDatabaseService;
            this.questionBox = uiContext.UserPrompts.YesNoBox;
            this.messageBox = uiContext.UserPrompts.MessageBox;
            this.inputBox = uiContext.UserPrompts.InputBox;
            BudgetPieController = uiContext.BudgetPieController;
            NewBudgetController = uiContext.NewBudgetModelController;
            NewBudgetController.Ready += OnAddNewBudgetReady;
            Shown = false;

            MessengerInstance = uiContext.Messenger;
            MessengerInstance.Register<ShellDialogResponseMessage>(this, OnPopUpResponseReceived);
            this.maintenanceService.Closed += OnClosedNotificationReceived;
            this.maintenanceService.NewDataSourceAvailable += OnNewDataSourceAvailableNotificationReceived;
            this.maintenanceService.Saving += OnSavingNotificationReceived;
            this.maintenanceService.Validating += OnValidatingNotificationReceived;
            this.maintenanceService.Saved += OnSavedNotificationReceived;

            CurrentBudget = new BudgetCurrencyContext(this.maintenanceService.Budgets, this.maintenanceService.Budgets.CurrentActiveBudget);
        }

        public ICommand AddNewExpenseCommand => new RelayCommand<ExpenseBucket>(OnAddNewExpenseExecute);

        public ICommand AddNewIncomeCommand => new RelayCommand(OnAddNewIncomeExecute);

        [UsedImplicitly]
        public string BudgetMenuItemName
        {
            [UsedImplicitly] get { return this.budgetMenuItemName; }

            set
            {
                this.budgetMenuItemName = value;
                RaisePropertyChanged();
            }
        }

        public BudgetPieController BudgetPieController { get; }
        public BudgetCollection Budgets { get; private set; }

        public BudgetCurrencyContext CurrentBudget
        {
            get { return this.doNotUseModel; }

            private set
            {
                try
                {
                    this.isLoadingBudgetModel = true;
                    this.doNotUseModel = value;
                    ReleaseListBindingEvents();
                    if (this.doNotUseModel == null)
                    {
                        Incomes = null;
                        Expenses = null;
                    }
                    else
                    {
                        SubscribeListBindingEvents();
                    }

                    RaisePropertyChanged(() => Incomes);
                    RaisePropertyChanged(() => Expenses);
                    OnExpenseAmountPropertyChanged(null, EventArgs.Empty);
                    OnIncomeAmountPropertyChanged(null, EventArgs.Empty);
                    RaisePropertyChanged(() => CurrentBudget);
                }
                finally
                {
                    this.isLoadingBudgetModel = false;
                }
            }
        }

        [UsedImplicitly]
        public ICommand DeleteBudgetItemCommand => new RelayCommand<object>(OnDeleteBudgetItemCommandExecute);

        [UsedImplicitly]
        public ICommand DetailsCommand => new RelayCommand(OnDetailsCommandExecute);

        public bool Dirty
        {
            get { return this.doNotUseDirty; }
            private set
            {
                this.doNotUseDirty = value;
                RaisePropertyChanged();
                if (Dirty)
                {
                    CurrentBudget.Model.LastModified = DateTime.Now;
                    this.applicationDatabaseService.NotifyOfChange(ApplicationDataType.Budget);
                }
            }
        }

        public BindingList<Expense> Expenses { get; private set; }

        public decimal ExpenseTotal
        {
            get { return this.expenseTotal; }

            private set
            {
                this.expenseTotal = value;
                RaisePropertyChanged();
            }
        }

        public BindingList<Income> Incomes { get; private set; }

        public decimal IncomeTotal
        {
            get { return this.incomeTotal; }

            private set
            {
                this.incomeTotal = value;
                RaisePropertyChanged();
            }
        }

        [UsedImplicitly]
        public ICommand NewBudgetCommand => new RelayCommand(OnAddNewBudgetCommandExecuted, () => CurrentBudget != null);

        public NewBudgetModelController NewBudgetController { get; }

        [UsedImplicitly]
        public ICommand ShowAllCommand => new RelayCommand(OnShowAllCommandExecuted);

        public bool Shown
        {
            get { return this.doNotUseShownBudget; }

            set
            {
                if (value == this.doNotUseShownBudget)
                {
                    return;
                }

                this.doNotUseShownBudget = value;
                RaisePropertyChanged();
                BudgetMenuItemName = this.doNotUseShownBudget ? CloseBudgetMenuName : EditBudgetMenuName;
            }
        }

        [UsedImplicitly]
        public ICommand ShowPieCommand => new RelayCommand(OnShowPieCommandExecuted, CanExecuteShowPieCommand);

        public decimal Surplus
        {
            [UsedImplicitly] get { return this.surplus; }
            private set
            {
                this.surplus = value;
                RaisePropertyChanged();
            }
        }

        public string TruncatedFileName => Budgets.StorageKey.TruncateLeft(100, true);

        protected virtual string PromptUserForLastModifiedComment()
        {
            var comment = this.inputBox.Show("Budget Maintenance", "Enter an optional comment to describe what you changed.");
            return comment ?? string.Empty;
        }

        private void BudgetModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (this.isLoadingBudgetModel) return;
            Dirty = true;
        }

        private bool CanExecuteShowPieCommand()
        {
            if (Expenses == null || Incomes == null || CurrentBudget == null)
            {
                return false;
            }

            return Expenses.Any() || Incomes.Any();
        }

        private void OnAddNewBudgetCommandExecuted()
        {
            var proposedDate = CurrentBudget.Model.EffectiveFrom.AddMonths(1);
            while (proposedDate < DateTime.Today)
            {
                proposedDate = proposedDate.AddMonths(1);
            }
            NewBudgetController.ShowDialog(proposedDate);
        }

        private void OnAddNewBudgetReady(object sender, EventArgs e)
        {
            try
            {
                var budget = this.maintenanceService.CloneBudgetModel(CurrentBudget.Model, NewBudgetController.EffectiveFrom);
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

        private void OnAddNewExpenseExecute(ExpenseBucket expense)
        {
            Dirty = true;
            var newExpense = Expenses.AddNew();
            Debug.Assert(newExpense != null);
            newExpense.Amount = 0;

            // New buckets must be created because the one passed in, is a single command parameter instance to be used as a type indicator only.
            // If it was used, the same instance would overwritten each time an expense is created.
            if (expense is SpentMonthlyExpenseBucket)
            {
                newExpense.Bucket = new SpentMonthlyExpenseBucket(string.Empty, string.Empty);
            }
            else if (expense is SavedUpForExpenseBucket)
            {
                newExpense.Bucket = new SavedUpForExpenseBucket(string.Empty, string.Empty);
            }
            else if (expense is SavingsCommitmentBucket)
            {
                newExpense.Bucket = new SavingsCommitmentBucket(string.Empty, string.Empty);
            }
            else
            {
                throw new InvalidCastException("Invalid type passed to Add New Expense: " + expense);
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

        private void OnClosedNotificationReceived(object sender, EventArgs eventArgs)
        {
            CurrentBudget = new BudgetCurrencyContext(this.maintenanceService.Budgets, this.maintenanceService.Budgets.CurrentActiveBudget);
            Budgets = CurrentBudget.BudgetCollection;
            BudgetBucketBindingSource.BucketRepository = this.maintenanceService.BudgetBucketRepository;
            RaisePropertyChanged(() => TruncatedFileName);
            MessengerInstance.Send(new BudgetReadyMessage(CurrentBudget, Budgets));
        }

        private void OnDeleteBudgetItemCommandExecute(object budgetItem)
        {
            bool? response = this.questionBox.Show(
                "Are you sure you want to delete this budget bucket?\nAnalysis may not work correctly if transactions are allocated to this bucket.",
                "Delete Budget Bucket");
            if (response == null || response.Value == false)
            {
                return;
            }

            Dirty = true;
            var expenseItem = budgetItem as Expense;
            if (expenseItem != null)
            {
                expenseItem.PropertyChanged -= OnExpenseAmountPropertyChanged;
                expenseItem.Bucket.PropertyChanged -= OnExpenseAmountPropertyChanged;
                Expenses.Remove(expenseItem);
                return;
            }

            var incomeItem = budgetItem as Income;
            if (incomeItem != null)
            {
                incomeItem.PropertyChanged -= OnIncomeAmountPropertyChanged;
                incomeItem.Bucket.PropertyChanged -= OnIncomeAmountPropertyChanged;
                Incomes.Remove(incomeItem);
            }
        }

        private void OnDetailsCommandExecute()
        {
            var popUpRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.Budget, CurrentBudget, ShellDialogType.Ok);
            MessengerInstance.Send(popUpRequest);
        }

        private void OnExpenseAmountPropertyChanged(object sender, EventArgs propertyChangedEventArgs)
        {
            if (!this.isLoadingBudgetModel && ExpenseTotal != 0)
            {
                Dirty = true;
            }

            ExpenseTotal = Expenses.Sum(x => x.Amount);
            Surplus = IncomeTotal - ExpenseTotal;
        }

        private void OnIncomeAmountPropertyChanged(object sender, EventArgs propertyChangedEventArgs)
        {
            if (!this.isLoadingBudgetModel && IncomeTotal != 0)
            {
                Dirty = true;
            }

            IncomeTotal = Incomes.Sum(x => x.Amount);
            Surplus = IncomeTotal - ExpenseTotal;
        }

        private void OnNewDataSourceAvailableNotificationReceived(object sender, EventArgs eventArgs)
        {
            SyncDataFromBudgetService();
        }

        private void OnPopUpResponseReceived(ShellDialogResponseMessage message)
        {
            if (!message.IsItForMe(this.dialogCorrelationId))
            {
                return;
            }

            var viewModel = (BudgetSelectionViewModel) message.Content;
            if (viewModel.Selected == null || viewModel.Selected == CurrentBudget.Model)
            {
                return;
            }

            ShowOtherBudget(viewModel.Selected);
        }

        private void OnSavedNotificationReceived(object sender, EventArgs eventArgs)
        {
            RaisePropertyChanged(() => TruncatedFileName);
            Dirty = false;
        }

        private void OnSavingNotificationReceived(object sender, AdditionalInformationRequestedEventArgs args)
        {
            SyncDataToBudgetService();
            args.Context = CurrentBudget.Model;
        }

        private void OnShowAllCommandExecuted()
        {
            SelectOtherBudget();
        }

        private void OnShowPieCommandExecuted()
        {
            BudgetPieController.Load(CurrentBudget.Model);
        }

        private void OnValidatingNotificationReceived(object sender, ValidatingEventArgs eventArgs)
        {
            if (Dirty) SyncDataToBudgetService();
        }

        private void ReleaseListBindingEvents()
        {
            CurrentBudget.Model.PropertyChanged -= BudgetModelOnPropertyChanged;
            if (Incomes != null)
            {
                foreach (var item in Incomes)
                {
                    item.PropertyChanged -= OnIncomeAmountPropertyChanged;
                    item.Bucket.PropertyChanged -= OnIncomeAmountPropertyChanged;
                }
            }

            if (Expenses != null)
            {
                foreach (var item in Expenses)
                {
                    item.PropertyChanged -= OnExpenseAmountPropertyChanged;
                    item.Bucket.PropertyChanged -= OnExpenseAmountPropertyChanged;
                }
            }
        }

        private void SelectOtherBudget()
        {
            this.dialogCorrelationId = Guid.NewGuid();
            var popUpRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.Budget, new BudgetSelectionViewModel(Budgets), ShellDialogType.Ok)
            {
                CorrelationId = this.dialogCorrelationId
            };
            MessengerInstance.Send(popUpRequest);
        }

        private void ShowOtherBudget(BudgetModel budgetToShow)
        {
            CurrentBudget = new BudgetCurrencyContext(Budgets, budgetToShow);
            Shown = true;
            Dirty = false; // Need to reset this because events fire needlessly (in this case) as a result of setting the CurrentBudget.
        }

        private void SubscribeListBindingEvents()
        {
            CurrentBudget.Model.PropertyChanged += BudgetModelOnPropertyChanged;
            Incomes = new BindingList<Income>(CurrentBudget.Model.Incomes.ToList());
            Incomes.ToList().ForEach(
                i =>
                {
                    i.PropertyChanged += OnIncomeAmountPropertyChanged;
                    i.Bucket.PropertyChanged += OnIncomeAmountPropertyChanged;
                });
            Expenses = new BindingList<Expense>(CurrentBudget.Model.Expenses.ToList());
            Expenses.ToList().ForEach(
                e =>
                {
                    e.PropertyChanged += OnExpenseAmountPropertyChanged;
                    e.Bucket.PropertyChanged += OnExpenseAmountPropertyChanged;
                });
        }

        private void SyncDataFromBudgetService()
        {
            Budgets = this.maintenanceService.Budgets;
            CurrentBudget = new BudgetCurrencyContext(Budgets, Budgets.CurrentActiveBudget);
            BudgetBucketBindingSource.BucketRepository = this.maintenanceService.BudgetBucketRepository;
            RaisePropertyChanged(() => TruncatedFileName);
            if (CurrentBudget != null)
            {
                MessengerInstance.Send(new BudgetReadyMessage(CurrentBudget, Budgets));
            }
        }

        private void SyncDataToBudgetService()
        {
            this.maintenanceService.UpdateIncomesAndExpenses(CurrentBudget.Model, Incomes, Expenses);
        }
    }
}