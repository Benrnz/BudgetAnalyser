using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget
{
    /// <summary>
    ///     A collection of budgets.  The collection is always sorted in descending order on the
    ///     <see cref="BudgetModel.EffectiveFrom" /> date. Ie: Future budgets are on top, then the current budget then archived
    ///     budgets.
    /// </summary>
    public class BudgetCollection : IEnumerable<BudgetModel>, IModelValidate
    {
        private readonly SortedList<DateTime, BudgetModel> budgetStorage;

        public BudgetCollection(IEnumerable<BudgetModel> initialBudgets)
        {
            this.budgetStorage = new SortedList<DateTime, BudgetModel>(initialBudgets.OrderByDescending(b => b.EffectiveFrom).ToDictionary(model => model.EffectiveFrom), new DateTimeDescendingOrder());
        }

        public int Count => this.budgetStorage.Count;

        public BudgetModel CurrentActiveBudget
        {
            get
            {
                return this.OrderByDescending(b => b.EffectiveFrom)
                    .FirstOrDefault(b => b.EffectiveFrom <= DateTime.Now);
            }
        }

        public string FileName { get; set; }
        internal BudgetModel this[int index] => this.budgetStorage.ElementAt(index).Value;

        public void Add([NotNull] BudgetModel item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            DateTime key = item.EffectiveFrom;
            while (this.budgetStorage.ContainsKey(key))
            {
                // Arbitrarily change the effective from date to ensure no overlap between budgets.
                key = key.AddDays(1);
            }

            item.EffectiveFrom = key;
            this.budgetStorage.Add(item.EffectiveFrom, item);
        }

        public BudgetModel ForDate(DateTime date)
        {
            return this.OrderByDescending(b => b.EffectiveFrom).FirstOrDefault(b => b.EffectiveFrom <= date);
        }

        public IEnumerable<BudgetModel> ForDates(DateTime beginInclusive, DateTime endInclusive)
        {
            var budgets = new List<BudgetModel>();
            BudgetModel firstEffectiveBudget = ForDate(beginInclusive);
            if (firstEffectiveBudget == null)
            {
                throw new BudgetException("The period covered by the dates given overlaps a period where no budgets are available.");
            }

            budgets.Add(firstEffectiveBudget);
            budgets.AddRange(this.Where(b => b.EffectiveFrom >= beginInclusive && b.EffectiveFrom < endInclusive));
            return budgets;
        }

        public IEnumerator<BudgetModel> GetEnumerator()
        {
            return this.budgetStorage.Select(kvp => kvp.Value).GetEnumerator();
        }

        public bool IsArchivedBudget(BudgetModel budget)
        {
            if (IsFutureBudget(budget))
            {
                return false;
            }

            if (IsCurrentBudget(budget))
            {
                return false;
            }

            return this.Any(b => b.EffectiveFrom <= budget.EffectiveFrom);
        }

        public bool IsCurrentBudget(BudgetModel budget)
        {
            return CurrentActiveBudget == budget;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Better for consistency with other methods here")]
        public bool IsFutureBudget([NotNull] BudgetModel budget)
        {
            if (budget == null)
            {
                throw new ArgumentNullException(nameof(budget));
            }

            return budget.EffectiveFrom > DateTime.Now;
        }

        public bool Validate(StringBuilder validationMessages)
        {
            bool allValid = this.All(budget => budget.Validate(validationMessages));
            return allValid;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal virtual int IndexOf(BudgetModel budget)
        {
            return this.budgetStorage.IndexOfValue(budget);
        }
    }
}