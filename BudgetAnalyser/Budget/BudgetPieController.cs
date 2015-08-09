using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Input;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Services;
using GalaSoft.MvvmLight.CommandWpf;
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

        public BudgetPieController([NotNull] IBudgetPieGraphService service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            this.budgetPieService = service;
        }

        public  ICommand CloseCommand => new RelayCommand(Close);

        public Expense CurrentExpense
        {
            get { return this.doNotUseCurrentExpense; }

            private set
            {
                this.doNotUseCurrentExpense = value;
                RaisePropertyChanged();
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
            get { return this.doNotUseCurrentIncome; }

            private set
            {
                this.doNotUseCurrentIncome = value;
                RaisePropertyChanged();
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

                RaisePropertyChanged();
                RaisePropertyChanged(() => CurrentExpensePercent);
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
                RaisePropertyChanged();
                RaisePropertyChanged(() => CurrentIncomePercent);
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
                RaisePropertyChanged();
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for data binding")]
        public  string Title => "Budget Pie Charts";

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
            RaisePropertyChanged(() => ExpensePieChartValues);
            RaisePropertyChanged(() => IncomePieChartValues);
        }
    }
}