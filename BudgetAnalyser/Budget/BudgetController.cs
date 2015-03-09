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
        private decimal surplus;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "OnPropertyChange is ok to call here")]
        public BudgetController(
            [NotNull] UiContext uiContext,
            [NotNull] IBudgetMaintenanceService maintenanceService,
            [NotNull] IApplicationDatabaseService applicationDatabaseService)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            if (maintenanceService == null)
            {
                throw new ArgumentNullException("maintenanceService");
            }

            if (applicationDatabaseService == null)
            {
                throw new ArgumentNullException("applicationDatabaseService");
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

            CurrentBudget = this.maintenanceService.CreateNewBudgetCollection();
        }

        [Engine.Annotations.UsedImplicitly]
        public ICommand AddNewExpenseCommand
        {
            get { return new RelayCommand<ExpenseBucket>(OnAddNewExpenseExecute); }
        }

        [Engine.Annotations.UsedImplicitly]
        public ICommand AddNewIncomeCommand
        {
            get { return new RelayCommand(OnAddNewIncomeExecute); }
        }

        public string BudgetMenuItemName
        {
            get { return this.budgetMenuItemName; }

            set
            {
                this.budgetMenuItemName = value;
                RaisePropertyChanged();
            }
        }

        public BudgetPieController BudgetPieController { get; private set; }
        public BudgetCollection Budgets { get; private set; }

        public BudgetCurrencyContext CurrentBudget
        {
            get { return this.doNotUseModel; }

            private set
            {
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
        }

        [Engine.Annotations.UsedImplicitly]
        public ICommand DeleteBudgetItemCommand
        {
            get { return new RelayCommand<object>(OnDeleteBudgetItemCommandExecute); }
        }

        [Engine.Annotations.UsedImplicitly]
        public ICommand DetailsCommand
        {
            get { return new RelayCommand(OnDetailsCommandExecute); }
        }

        public bool Dirty
        {
            get { return this.doNotUseDirty; }
            private set
            {
                this.doNotUseDirty = value;
                RaisePropertyChanged();
                CurrentBudget.Model.LastModified = DateTime.Now;
                if (Dirty)
                {
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

        [Engine.Annotations.UsedImplicitly]
        public ICommand NewBudgetCommand
        {
            get { return new RelayCommand(OnAddNewBudgetCommandExecuted, () => CurrentBudget != null); }
        }

        public NewBudgetModelController NewBudgetController { get; private set; }

        [Engine.Annotations.UsedImplicitly]
        public ICommand ShowAllCommand
        {
            get { return new RelayCommand(OnShowAllCommandExecuted); }
        }

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

        [Engine.Annotations.UsedImplicitly]
        public ICommand ShowPieCommand
        {
            get { return new RelayCommand(OnShowPieCommandExecuted, CanExecuteShowPieCommand); }
        }

        public decimal Surplus
        {
            get { return this.surplus; }
            private set
            {
                this.surplus = value;
                RaisePropertyChanged();
            }
        }

        public string TruncatedFileName
        {
            get { return Budgets.FileName.TruncateLeft(100, true); }
        }

        protected virtual string PromptUserForLastModifiedComment()
        {
            string comment = this.inputBox.Show("Budget Maintenance", "Enter an optional comment to describe what you changed.");
            return comment ?? string.Empty;
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
            DateTime proposedDate = CurrentBudget.Model.EffectiveFrom.AddMonths(1);
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
                BudgetModel budget = this.maintenanceService.CloneBudgetModel(CurrentBudget.Model, NewBudgetController.EffectiveFrom);
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
            Expense newExpense = Expenses.AddNew();
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
            CurrentBudget = this.maintenanceService.CreateNewBudgetCollection();
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
            if (ExpenseTotal != 0)
            {
                Dirty = true;
            }

            ExpenseTotal = Expenses.Sum(x => x.Amount);
            Surplus = IncomeTotal - ExpenseTotal;
        }

        private void OnIncomeAmountPropertyChanged(object sender, EventArgs propertyChangedEventArgs)
        {
            if (IncomeTotal != 0)
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

            var viewModel = (BudgetSelectionViewModel)message.Content;
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
            args.ModificationComment = PromptUserForLastModifiedComment();
            Dirty = false;
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
            SyncDataToBudgetService();
        }

        private void ReleaseListBindingEvents()
        {
            if (Incomes != null)
            {
                foreach (Income item in Incomes)
                {
                    item.PropertyChanged -= OnIncomeAmountPropertyChanged;
                    item.Bucket.PropertyChanged -= OnIncomeAmountPropertyChanged;
                }
            }

            if (Expenses != null)
            {
                foreach (Expense item in Expenses)
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
            Incomes = new BindingList<Income>(this.doNotUseModel.Model.Incomes.ToList());
            Incomes.ToList().ForEach(
                i =>
                {
                    i.PropertyChanged += OnIncomeAmountPropertyChanged;
                    i.Bucket.PropertyChanged += OnIncomeAmountPropertyChanged;
                });
            Expenses = new BindingList<Expense>(this.doNotUseModel.Model.Expenses.ToList());
            Expenses.ToList().ForEach(
                e =>
                {
                    e.PropertyChanged += OnExpenseAmountPropertyChanged;
                    e.Bucket.PropertyChanged += OnExpenseAmountPropertyChanged;
                });
        }

        private void SyncDataFromBudgetService()
        {
            // TODO Consider sending out a loading new data source preview event so loading indicators could be displayed on each tab.
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