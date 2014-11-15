using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Services
{
    /// <summary>
    /// A service to prepare and present data ready for convenient consumption by the Budget Pie Graph.
    /// </summary>
    [AutoRegisterWithIoC]
    public class BudgetPieGraphService : IBudgetPieGraphService
    {
        private readonly IBudgetBucketRepository budgetBucketRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="BudgetPieGraphService"/> class.
        /// </summary>
        /// <param name="budgetBucketRepository">The budget bucket repository.</param>
        /// <exception cref="System.ArgumentNullException">budgetBucketRepository</exception>
        public BudgetPieGraphService([NotNull] IBudgetBucketRepository budgetBucketRepository)
        {
            if (budgetBucketRepository == null)
            {
                throw new ArgumentNullException("budgetBucketRepository");
            }

            this.budgetBucketRepository = budgetBucketRepository;
        }

        /// <summary>
        /// Prepares the income graph data.
        /// </summary>
        public IDictionary<string, decimal> PrepareIncomeGraphData([NotNull] BudgetModel budget)
        {
            if (budget == null)
            {
                throw new ArgumentNullException("budget");
            }

            var list = budget.Incomes
                .Select(income => new KeyValuePair<string, decimal>(income.Bucket.Code, income.Amount))
                .ToList();
            return list.OrderByDescending(x => x.Value)
                .ToDictionary(i => i.Key, i => i.Value);
        }

        /// <summary>
        /// Prepares the expense graph data.
        /// </summary>
        public IDictionary<string, decimal> PrepareExpenseGraphData([NotNull] BudgetModel budget)
        {
            if (budget == null)
            {
                throw new ArgumentNullException("budget");
            }
            
            var surplus = new Expense { Amount = budget.Surplus, Bucket = this.budgetBucketRepository.SurplusBucket };
            var interim = budget.Expenses.Select(expense => new KeyValuePair<string, decimal>(expense.Bucket.Code, expense.Amount)).ToList();
            interim.Add(new KeyValuePair<string, decimal>(surplus.Bucket.Code, surplus.Amount));
            return interim.OrderByDescending(x => x.Value).ToDictionary(e => e.Key, e => e.Value);
        }

        /// <summary>
        /// A model Surplus expense object for the UI to bind to.
        /// </summary>
        public Expense SurplusExpense([NotNull] BudgetModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            return new Expense { Amount = model.Surplus, Bucket = this.budgetBucketRepository.SurplusBucket };
        }
    }
}
