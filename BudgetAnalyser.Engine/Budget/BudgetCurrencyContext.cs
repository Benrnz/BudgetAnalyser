using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget
{
    /// <summary>
    ///     A transient wrapper class to indicate a budgets active state in relation to other budgets in the collection.
    ///     This class will tell you if a budget is active, future dated, or has past and is archived.
    /// </summary>
    public class BudgetCurrencyContext : INotifyPropertyChanged 
    {
        private readonly BudgetCollection budgets;

        public BudgetCurrencyContext([NotNull] BudgetCollection budgets, [NotNull] BudgetModel budget)
        {
            if (budgets == null)
            {
                throw new ArgumentNullException("budgets");
            }

            if (budget == null)
            {
                throw new ArgumentNullException("budget");
            }

            this.budgets = budgets;
            this.Model = budget;

            if (budgets.All(b => b != budget))
            {
                throw new System.Collections.Generic.KeyNotFoundException("The given budget is not found in the given collection.");
            }
        }

        public bool BudgetActive
        {
            get { return this.budgets.IsCurrentBudget(this.Model); }
        }

        public bool BudgetArchived
        {
            get { return this.budgets.IsArchivedBudget(this.Model); }
        }

        public bool BudgetInFuture
        {
            get { return this.budgets.IsFutureBudget(this.Model); }
        }

        public DateTime? EffectiveUntil
        {
            get { 
                var orderedBudgets = this.budgets.OrderByDescending(b => b.EffectiveFrom).ToList();
                var myIndex = orderedBudgets.IndexOf(this.Model);
                if (myIndex == 0)
                {
                    return null;
                }

                var nextBudget = orderedBudgets[myIndex - 1];
                return nextBudget.EffectiveFrom;
            }
        }

        public string FileName
        {
            get { return this.budgets.FileName; }
        }

        public BudgetModel Model { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}