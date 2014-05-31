using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using GalaSoft.MvvmLight.Command;
using Rees.Wpf;

namespace BudgetAnalyser.Budget
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class BudgetPieController : ControllerBase, IShowableController
    {
        private readonly IBudgetBucketRepository budgetBucketRepository;
        private BudgetModel budgetModel;
        private Expense doNotUseCurrentExpense;
        private Income doNotUseCurrentIncome;
        private KeyValuePair<string, decimal> doNotUseExpenseSelectedItem;
        private KeyValuePair<string, decimal> doNotUseIncomeSelectedItem;
        private bool doNotUseShown;
        private Expense surplus;

        public BudgetPieController([NotNull] IBudgetBucketRepository budgetBucketRepository)
        {
            if (budgetBucketRepository == null)
            {
                throw new ArgumentNullException("budgetBucketRepository");
            }

            this.budgetBucketRepository = budgetBucketRepository;
        }

        public ICommand CloseCommand
        {
            get { return new RelayCommand(Close); }
        }

        public Expense CurrentExpense
        {
            get { return this.doNotUseCurrentExpense; }

            private set
            {
                this.doNotUseCurrentExpense = value;
                RaisePropertyChanged(() => CurrentExpense);
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
                RaisePropertyChanged(() => CurrentIncome);
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
                if (this.doNotUseExpenseSelectedItem.Key == this.budgetBucketRepository.SurplusBucket.Code)
                {
                    CurrentExpense = this.surplus;
                }
                else
                {
                    CurrentExpense =
                        this.budgetModel.Expenses.SingleOrDefault(
                            x => x.Bucket.Code == this.doNotUseExpenseSelectedItem.Key);
                }

                RaisePropertyChanged(() => ExpenseSelectedItem);
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
                CurrentIncome =
                    this.budgetModel.Incomes.SingleOrDefault(x => x.Bucket.Code == this.doNotUseIncomeSelectedItem.Key);
                RaisePropertyChanged(() => IncomeSelectedItem);
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
                RaisePropertyChanged(() => Shown);
            }
        }

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
                throw new ArgumentNullException("model");
            }

            this.budgetModel = model;
            this.surplus = new Expense { Amount = model.Surplus, Bucket = this.budgetBucketRepository.SurplusBucket };
            List<KeyValuePair<string, decimal>> list =
                model.Expenses.Select(expense => new KeyValuePair<string, decimal>(expense.Bucket.Code, expense.Amount))
                    .ToList();
            list.Add(new KeyValuePair<string, decimal>(this.budgetBucketRepository.SurplusBucket.Code, model.Surplus));
            ExpensePieChartValues = list.OrderByDescending(x => x.Value).ToList();

            list =
                model.Incomes.Select(income => new KeyValuePair<string, decimal>(income.Bucket.Code, income.Amount))
                    .ToList();
            IncomePieChartValues = list.OrderByDescending(x => x.Value).ToList();

            Shown = true;
            RaisePropertyChanged(() => ExpensePieChartValues);
            RaisePropertyChanged(() => IncomePieChartValues);
        }
    }
}