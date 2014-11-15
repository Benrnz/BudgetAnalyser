using System;
using System.Collections.Generic;
using System.Text;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Services
{
    /// <summary>
    ///     A service to provide maintenance support for budget models and collections.
    ///     This class is stateful and is intended to be used as a single instance.
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class BudgetMaintenanceService : IBudgetMaintenanceService
    {
        private readonly IBudgetRepository budgetRepository;
        private readonly ILogger logger;
        private BudgetCollection budgetsCollection;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BudgetMaintenanceService" /> class.
        /// </summary>
        /// <param name="budgetRepository">The budget repository.</param>
        /// <param name="bucketRepo">The budget bucket repository.</param>
        /// <param name="logger">The diagnostic logger</param>
        /// <exception cref="System.ArgumentNullException">budgetRepository</exception>
        public BudgetMaintenanceService(
            [NotNull] IBudgetRepository budgetRepository,
            [NotNull] IBudgetBucketRepository bucketRepo, 
            [NotNull] ILogger logger)
        {
            if (budgetRepository == null)
            {
                throw new ArgumentNullException("budgetRepository");
            }

            if (bucketRepo == null)
            {
                throw new ArgumentNullException("bucketRepo");
            }
            
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            this.budgetRepository = budgetRepository;
            this.logger = logger;
            BudgetBucketRepository = bucketRepo;
        }

        /// <summary>
        ///     Gets the budget bucket repository.
        ///     Allows the UI to set up a static reference to the Bucket repository for binding, converters and templates.
        /// </summary>
        public IBudgetBucketRepository BudgetBucketRepository { get; private set; }

        /// <summary>
        ///     Creates a new budget collection with one new empty budget model.
        /// </summary>
        /// <returns>
        ///     An object that contains a collection of one new budget.
        /// </returns>
        public BudgetCurrencyContext CreateNewBudgetCollection()
        {
            var budget = new BudgetModel();
            this.budgetsCollection = new BudgetCollection(new[] { budget });
            return new BudgetCurrencyContext(this.budgetsCollection, budget);
        }

        /// <summary>
        ///     Loads the collection of budgets from persistent storage.
        /// </summary>
        /// <param name="storageKey">The storage key to identify the budget collection.</param>
        /// <returns>
        ///     An object that contains the full collection of available budgets as well as the most recent selected as the
        ///     currently selected budget.
        /// </returns>
        public BudgetCurrencyContext LoadBudgetsCollection(string storageKey)
        {
            this.budgetsCollection = this.budgetRepository.Load(storageKey);
            return new BudgetCurrencyContext(this.budgetsCollection, this.budgetsCollection.CurrentActiveBudget);
        }

        /// <summary>
        ///     Saves the budget collection after modifications in the UI.
        /// </summary>
        /// <param name="modifiedBudget">The modified budget.</param>
        /// <param name="comment">The optional comment, explaining what was changed.</param>
        /// <exception cref="System.InvalidOperationException">
        ///     Will be thrown when you haven't loaded a Budget Collection to save yet.
        /// </exception>
        public bool SaveBudget([NotNull] BudgetModel modifiedBudget, string comment = "")
        {
            if (modifiedBudget == null)
            {
                throw new ArgumentNullException("modifiedBudget");
            }
            if (this.budgetsCollection == null)
            {
                throw new InvalidOperationException("You haven't loaded a Budget Collection to save yet.");
            }

            modifiedBudget.LastModifiedComment = comment;
            modifiedBudget.LastModified = DateTime.Now;

            // Make sure all buckets are in the bucket repo.
            foreach (Income income in modifiedBudget.Incomes)
            {
                Income incomeCopy = income;
                BudgetBucketRepository.GetOrCreateNew(incomeCopy.Bucket.Code, () => incomeCopy.Bucket);
            }

            foreach (Expense expense in modifiedBudget.Expenses)
            {
                Expense expenseCopy = expense;
                BudgetBucketRepository.GetOrCreateNew(expenseCopy.Bucket.Code, () => expenseCopy.Bucket);
            }

            var messages = new StringBuilder();
            if (modifiedBudget.Validate(messages))
            {
                this.budgetRepository.Save(this.budgetsCollection);
                return true;
            }
            
            this.logger.LogWarning(l => l.Format("BudgetMaintenanceService.Save: unable to save due to validation errors:\n{0}", messages));
            return false;
        }

        /// <summary>
        ///     Updates the budget with the full collection of incomes and expenses and then validates the budget.
        /// </summary>
        /// <param name="model">The Budget model.</param>
        /// <param name="allIncomes">All income objects that may have changed in the UI.</param>
        /// <param name="allExpenses">All expense objects that may have changed in the UI.</param>
        /// <param name="validationMessages">The validation messages. Will be blank if no issues are found.</param>
        /// <returns>
        ///     True if valid, otherwise false.  If false is returned there will be addition information in the
        ///     <paramref name="validationMessages" /> string builder.
        /// </returns>
        public bool UpdateAndValidateBudget(
            [NotNull] BudgetModel model,
            IEnumerable<Income> allIncomes,
            IEnumerable<Expense> allExpenses,
            StringBuilder validationMessages)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            // Copy view model bound data back into model.
            model.Update(allIncomes, allExpenses);
            return this.budgetsCollection.Validate(validationMessages);
        }
    }
}