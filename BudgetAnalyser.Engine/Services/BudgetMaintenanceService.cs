using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.Services
{
    /// <summary>
    ///     A service to provide maintenance support for budget models and collections.
    ///     This class is stateful and is intended to be used as a single instance.
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class BudgetMaintenanceService : IBudgetMaintenanceService, ISupportsModelPersistence
    {
        private readonly IBudgetRepository budgetRepository;
        private readonly IDashboardService dashboardService;
        private readonly ILogger logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BudgetMaintenanceService" /> class.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">budgetRepository</exception>
        public BudgetMaintenanceService(
            [NotNull] IBudgetRepository budgetRepository,
            [NotNull] IBudgetBucketRepository bucketRepo,
            [NotNull] ILogger logger,
            [NotNull] IDashboardService dashboardService)
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

            if (dashboardService == null)
            {
                throw new ArgumentNullException("dashboardService");
            }

            this.budgetRepository = budgetRepository;
            this.logger = logger;
            this.dashboardService = dashboardService;
            BudgetBucketRepository = bucketRepo;
        }

        public event EventHandler Closed;
        public event EventHandler NewDataSourceAvailable;
        public event EventHandler Saved;
        public event EventHandler<AdditionalInformationRequestedEventArgs> Saving;
        public event EventHandler<ValidatingEventArgs> Validating;

        /// <summary>
        ///     Gets the budget bucket repository.
        ///     Allows the UI to set up a static reference to the Bucket repository for binding, converters and templates.
        /// </summary>
        public IBudgetBucketRepository BudgetBucketRepository { get; private set; }

        public BudgetCollection Budgets { get; private set; }

        /// <summary>
        /// Gets the type of the data the implementation deals with.
        /// </summary>
        public ApplicationDataType DataType
        {
            get { return ApplicationDataType.Budget; }
        }

        /// <summary>
        ///     Gets the initialisation sequence number. Set this to a low number for important data that needs to be loaded first.
        ///     Defaults to 50.
        /// </summary>
        public int LoadSequence
        {
            get { return 5; }
        }

        /// <summary>
        ///     Clones the given <see cref="BudgetModel" /> to create a new budget with a future effective date.
        /// </summary>
        /// <param name="sourceBudget">The source budget to clone from.</param>
        /// <param name="newBudgetEffectiveFrom">This date will be used as the new budget's effective date.</param>
        /// <returns>The newly created budget.</returns>
        /// <exception cref="ArgumentNullException">Will be thrown if source budget is null.</exception>
        /// <exception cref="ValidationWarningException">Will be thrown if the source budget is in an invalid state.</exception>
        /// <exception cref="ArgumentException">
        ///     Will be thrown if the effective date of the new budget is not after the provided
        ///     budget.
        /// </exception>
        /// <exception cref="ArgumentException">Will be thrown if the effective date is not a future date.</exception>
        public BudgetModel CloneBudgetModel(BudgetModel sourceBudget, DateTime newBudgetEffectiveFrom)
        {
            if (sourceBudget == null)
            {
                throw new ArgumentNullException("sourceBudget");
            }

            if (newBudgetEffectiveFrom <= sourceBudget.EffectiveFrom)
            {
                throw new ArgumentException("The effective date of the new budget must be later than the other budget.", "newBudgetEffectiveFrom");
            }

            if (newBudgetEffectiveFrom <= DateTime.Today)
            {
                throw new ArgumentException("The effective date of the new budget must be a future date.", "newBudgetEffectiveFrom");
            }

            var validationMessages = new StringBuilder();
            if (!sourceBudget.Validate(validationMessages))
            {
                throw new ValidationWarningException(
                    string.Format(CultureInfo.CurrentCulture, "The source budget is currently in an invalid state, unable to clone it at this time.\n{0}", validationMessages));
            }

            var newBudget = new BudgetModel
            {
                EffectiveFrom = newBudgetEffectiveFrom,
                Name = string.Format(CultureInfo.CurrentCulture, "Copy of {0}", sourceBudget.Name)
            };
            newBudget.Update(CloneBudgetIncomes(sourceBudget), CloneBudgetExpenses(sourceBudget));

            if (!newBudget.Validate(validationMessages))
            {
                throw new InvalidOperationException("New cloned budget is invalid and the source budget is not. Code Error.\n" + validationMessages);
            }

            Budgets.Add(newBudget);
            this.budgetRepository.SaveAsync();
            return newBudget;
        }

        /// <summary>
        ///     Closes the currently loaded file.  No warnings will be raised if there is unsaved data.
        /// </summary>
        public void Close()
        {
            CreateNewBudgetCollection();
            EventHandler handler = Closed;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        ///     Creates a new budget collection with one new empty budget model.
        /// </summary>
        /// <returns>
        ///     An object that contains a collection of one new budget.
        /// </returns>
        public BudgetCurrencyContext CreateNewBudgetCollection()
        {
            Budgets = this.budgetRepository.CreateNew();
            return new BudgetCurrencyContext(Budgets, Budgets.First());
        }

        /// <summary>
        ///     Loads a data source with the provided database reference data asynchronously.
        /// </summary>
        public async Task LoadAsync(ApplicationDatabase applicationDatabase)
        {
            if (applicationDatabase == null)
            {
                throw new ArgumentNullException("applicationDatabase");
            }

            Budgets = await this.budgetRepository.LoadAsync(applicationDatabase.FullPath(applicationDatabase.BudgetCollectionStorageKey));
            EventHandler handler = NewDataSourceAvailable;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }

            this.dashboardService.NotifyOfDependencyChange<IBudgetBucketRepository>(BudgetBucketRepository);
        }

        /// <summary>
        ///     Saves the application database asynchronously.
        /// </summary>
        public async Task SaveAsync()
        {
            EventHandler<AdditionalInformationRequestedEventArgs> handler = Saving;
            var args = new AdditionalInformationRequestedEventArgs();
            if (handler != null)
            {
                handler(this, args);
            }

            if (args.ModificationComment.IsNothing())
            {
                args.ModificationComment = "[No comment]";
            }

            var budgetModel = args.Context as BudgetModel;
            if (budgetModel != null)
            {
                budgetModel.LastModifiedComment = args.ModificationComment;
            }

            EnsureAllBucketsUsedAreInBucketRepo();

            var messages = new StringBuilder();
            if (Budgets.Validate(messages))
            {
                await this.budgetRepository.SaveAsync();
                var savedHandler = Saved;
                if (savedHandler != null) savedHandler(this, EventArgs.Empty);
                return;
            }

            this.logger.LogWarning(l => l.Format("BudgetMaintenanceService.Save: unable to save due to validation errors:\n{0}", messages));
            throw new ValidationWarningException("Unable to save Budget:\n" + messages);
        }

        public void UpdateIncomesAndExpenses(
            [NotNull] BudgetModel model,
            IEnumerable<Income> allIncomes,
            IEnumerable<Expense> allExpenses)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            // Copy view model bound data back into model.
            model.Update(allIncomes, allExpenses);
        }

        /// <summary>
        ///     Validates the model owned by the service.
        /// </summary>
        public bool ValidateModel(StringBuilder messages)
        {
            EventHandler<ValidatingEventArgs> handler = Validating;
            var args = new ValidatingEventArgs();
            if (handler != null)
            {
                handler(this, args);
            }

            return Budgets.Validate(messages);
        }

        private void EnsureAllBucketsUsedAreInBucketRepo()
        {
            // Make sure all buckets are in the bucket repo.
            IEnumerable<BudgetBucket> buckets = Budgets.SelectMany(b => b.Expenses.Select(e => e.Bucket))
                .Union(Budgets.SelectMany(b => b.Incomes.Select(i => i.Bucket)))
                .Distinct();

            foreach (BudgetBucket budgetBucket in buckets)
            {
                BudgetBucket copyOfBucket = budgetBucket;
                BudgetBucketRepository.GetOrCreateNew(copyOfBucket.Code, () => copyOfBucket);
            }
        }

        private static IEnumerable<Expense> CloneBudgetExpenses(BudgetModel source)
        {
            return source.Expenses.Select(
                sourceExpense => new Expense
                {
                    Amount = sourceExpense.Amount,
                    Bucket = sourceExpense.Bucket
                }).ToList();
        }

        private static IEnumerable<Income> CloneBudgetIncomes(BudgetModel source)
        {
            return source.Incomes.Select(
                sourceExpense => new Income
                {
                    Amount = sourceExpense.Amount,
                    Bucket = sourceExpense.Bucket
                }).ToList();
        }
    }
}