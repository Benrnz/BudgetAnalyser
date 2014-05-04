using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget
{
    public class BudgetCollection : List<BudgetModel>, IModelValidate
    {
        public BudgetCollection()
        {
        }

        public BudgetCollection(IEnumerable<BudgetModel> initialBudgets) : base(initialBudgets.OrderByDescending(b => b.EffectiveFrom))
        {
        }

        public event EventHandler Validating;

        public BudgetModel CurrentActiveBudget
        {
            get
            {
                return this.OrderByDescending(b => b.EffectiveFrom)
                    .FirstOrDefault(b => b.EffectiveFrom <= DateTime.Now);
            }
        }

        public string FileName { get; set; }

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Better for consistency with other methods here")]
        public bool IsFutureBudget([NotNull] BudgetModel budget)
        {
            if (budget == null)
            {
                throw new ArgumentNullException("budget");
            }

            return budget.EffectiveFrom > DateTime.Now;
        }

        public bool Validate(StringBuilder validationMessages)
        {
            bool allValid = this.All(budget => budget.Validate(validationMessages));
            List<IGrouping<DateTime, BudgetModel>> duplicateEffectiveDates = this.GroupBy(b => b.EffectiveFrom, b => b).ToList();
            if (duplicateEffectiveDates.Any(group => group.Count() > 1))
            {
                // Loop all duplicate dates found
                foreach (var duplicateGroups in duplicateEffectiveDates)
                {
                    // Arbitrarily change the effective dates to ensure a sure later effective date.
                    int index = 0;
                    foreach (BudgetModel budget in duplicateGroups)
                    {
                        budget.EffectiveFrom = budget.EffectiveFrom.AddSeconds(index);
                        index++;
                    }
                }
            }

            EventHandler handler = Validating;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }

            return allValid;
        }
    }
}