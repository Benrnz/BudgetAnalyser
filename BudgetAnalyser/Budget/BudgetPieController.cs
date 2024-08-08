using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Services;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.Budget
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class BudgetPieController : ControllerBase, IShowableController
    {
        private readonly IBudgetPieGraphService budgetPieService;
        private BudgetModel budgetModel;
        private Expense doNotUseCurrentExpense;
        private Income doNotUseCurrentIncome;
        private KeyValuePair<string, decimal> doNotUseExpenseSelectedItem;
        private KeyValuePair<string, decimal> doNotUseIncomeSelectedItem;
        private bool doNotUseShown;
        private Expense surplus;

        public BudgetPieController([NotNull] IMessenger messenger, [NotNull] IBudgetPieGraphService service) : base(messenger)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            this.budgetPieService = service;
        }

        [UsedImplicitly]
        public ICommand CloseCommand => new RelayCommand(Close);

        public Expense CurrentExpense
        {
            [UsedImplicitly] get { return this.doNotUseCurrentExpense; }

            private set
            {
                this.doNotUseCurrentExpense = value;
                OnPropertyChanged();
            }
        }

        public double CurrentExpensePercent
        {
            get
            {
                if (ExpensePieChartValues == null)
                {
                    return 0;
                }

                return (double)ExpenseSelectedItem.Value / (double)ExpensePieChartValues.Sum(e => e.Value);
            }
        }

        public Income CurrentIncome
        {
            [UsedImplicitly] get { return this.doNotUseCurrentIncome; }

            private set
            {
                this.doNotUseCurrentIncome = value;
                OnPropertyChanged();
            }
        }

        public double CurrentIncomePercent
        {
            get
            {
                if (IncomePieChartValues == null)
                {
                    return 0;
                }

                return (double)IncomeSelectedItem.Value / (double)IncomePieChartValues.Sum(i => i.Value);
            }
        }

        public IEnumerable<KeyValuePair<string, decimal>> ExpensePieChartValues { get; private set; }

        public KeyValuePair<string, decimal> ExpenseSelectedItem
        {
            get { return this.doNotUseExpenseSelectedItem; }

            set
            {
                this.doNotUseExpenseSelectedItem = value;
                if (this.doNotUseExpenseSelectedItem.Key == SurplusBucket.SurplusCode)
                {
                    CurrentExpense = this.surplus;
                }
                else
                {
                    CurrentExpense = this.budgetModel.Expenses.SingleOrDefault(x => x.Bucket.Code == this.doNotUseExpenseSelectedItem.Key);
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentExpensePercent));
            }
        }

        public IEnumerable<KeyValuePair<string, decimal>> IncomePieChartValues { get; private set; }

        public KeyValuePair<string, decimal> IncomeSelectedItem
        {
            get { return this.doNotUseIncomeSelectedItem; }

            set
            {
                this.doNotUseIncomeSelectedItem = value;
                CurrentIncome = this.budgetModel.Incomes.SingleOrDefault(x => x.Bucket.Code == this.doNotUseIncomeSelectedItem.Key);
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentIncomePercent));
            }
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
                OnPropertyChanged();
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for data binding")]
        [UsedImplicitly]
        public string Title => "Budget Pie Charts";

        public void Close()
        {
            Shown = false;
            this.budgetModel = null;
            CurrentExpense = null;
            CurrentIncome = null;
            this.surplus = null;
            ExpensePieChartValues = null;
            IncomePieChartValues = null;
        }

        public void Load([NotNull] BudgetModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            this.surplus = this.budgetPieService.SurplusExpense(model);
            this.budgetModel = model;
            ExpensePieChartValues = this.budgetPieService.PrepareExpenseGraphData(model);
            IncomePieChartValues = this.budgetPieService.PrepareIncomeGraphData(model);

            Shown = true;
            OnPropertyChanged(nameof(ExpensePieChartValues));
            OnPropertyChanged(nameof(IncomePieChartValues));
        }
    }
}