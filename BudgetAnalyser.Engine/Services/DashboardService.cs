using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.Widgets;

namespace BudgetAnalyser.Engine.Services
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class DashboardService : IDashboardService
    {
        private readonly IAccountTypeRepository accountTypeRepository;
        private readonly IBudgetBucketRepository bucketRepository;
        private readonly IBudgetRepository budgetRepository;
        private readonly Dictionary<Type, long> changesHashes = new Dictionary<Type, long>();
        private readonly LedgerCalculation ledgerCalculator;
        private readonly ILogger logger;
        private readonly ITransactionManagerService transactionManagerService;
        private readonly IWidgetRepository widgetRepository;
        private readonly IWidgetService widgetService;
        private IDictionary<Type, object> availableDependencies;

        public DashboardService(
            [NotNull] IWidgetService widgetService,
            [NotNull] IWidgetRepository widgetRepository,
            [NotNull] IBudgetBucketRepository bucketRepository,
            [NotNull] IBudgetRepository budgetRepository,
            [NotNull] LedgerCalculation ledgerCalculator,
            [NotNull] IAccountTypeRepository accountTypeRepository,
            [NotNull] ITransactionManagerService transactionManagerService,
            [NotNull] ILogger logger)
        {
            if (widgetService == null)
            {
                throw new ArgumentNullException("widgetService");
            }

            if (widgetRepository == null)
            {
                throw new ArgumentNullException("widgetRepository");
            }

            if (bucketRepository == null)
            {
                throw new ArgumentNullException("bucketRepository");
            }

            if (budgetRepository == null)
            {
                throw new ArgumentNullException("budgetRepository");
            }

            if (ledgerCalculator == null)
            {
                throw new ArgumentNullException("ledgerCalculator");
            }

            if (accountTypeRepository == null)
            {
                throw new ArgumentNullException("accountTypeRepository");
            }

            if (transactionManagerService == null)
            {
                throw new ArgumentNullException("transactionManagerService");
            }

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            this.widgetService = widgetService;
            this.widgetRepository = widgetRepository;
            this.bucketRepository = bucketRepository;
            this.budgetRepository = budgetRepository;
            this.ledgerCalculator = ledgerCalculator;
            this.accountTypeRepository = accountTypeRepository;
            this.transactionManagerService = transactionManagerService;
            this.logger = logger;
        }

        protected ObservableCollection<WidgetGroup> WidgetGroups { get; private set; }

        /// <summary>
        ///     Creates a new bucket monitor widget and adds it to the tracked widgetGroups collection.
        ///     Duplicates are not allowed in the collection and will not be added.
        /// </summary>
        /// <param name="bucketCode">The bucket code to create a new monitor widget for.</param>
        /// <returns>
        ///     Will return a reference to the newly created widget, or null if the widget was not created because a duplicate
        ///     already exists.
        /// </returns>
        public Widget CreateNewBucketMonitorWidget(string bucketCode)
        {
            if (WidgetGroups.SelectMany(group => group.Widgets).OfType<BudgetBucketMonitorWidget>().Any(w => w.BucketCode == bucketCode))
            {
                // Bucket code already exists - so already has a bucket monitor widget.
                return null;
            }

            IUserDefinedWidget widget = this.widgetRepository.Create(typeof(BudgetBucketMonitorWidget).FullName, bucketCode);
            return UpdateWidgetCollectionWithNewAddition((Widget)widget);
        }

        public Widget CreateNewFixedBudgetMonitorWidget(string bucketCode, string description, decimal fixedBudgetAmount)
        {
            if (string.IsNullOrWhiteSpace(bucketCode))
            {
                throw new ArgumentNullException("bucketCode");
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentNullException("description");
            }

            if (fixedBudgetAmount <= 0)
            {
                throw new ArgumentException("Fixed Budget amount must be greater than zero.", "fixedBudgetAmount");
            }

            FixedBudgetProjectBucket bucket = this.bucketRepository.CreateNewFixedBudgetProject(bucketCode, description, fixedBudgetAmount);
            this.budgetRepository.Save();
            IUserDefinedWidget widget = this.widgetRepository.Create(typeof(FixedBudgetMonitorWidget).FullName, bucket.Code);
            return UpdateWidgetCollectionWithNewAddition((Widget)widget);
        }

        public Widget CreateNewSurprisePaymentMonitorWidget(string bucketCode, DateTime paymentDate, WeeklyOrFortnightly frequency)
        {
            if (string.IsNullOrWhiteSpace(bucketCode))
            {
                throw new ArgumentNullException("bucketCode");
            }

            if (paymentDate == DateTime.MinValue)
            {
                throw new ArgumentException("Payment date is not set.", "paymentDate");
            }

            BudgetBucket bucket = this.bucketRepository.GetByCode(bucketCode);
            if (bucket == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "No Bucket with code {0} exists", bucketCode), "bucketCode");
            }

            IUserDefinedWidget widget = this.widgetRepository.Create(typeof(SurprisePaymentWidget).FullName, bucket.Code);
            var paymentWidget = (SurprisePaymentWidget)widget;
            paymentWidget.StartPaymentDate = paymentDate;
            paymentWidget.Frequency = frequency;
            return UpdateWidgetCollectionWithNewAddition((Widget)widget);
        }

        public IEnumerable<AccountType> FilterableAccountTypes()
        {
            List<AccountType> accountTypeList = this.accountTypeRepository.ListCurrentlyUsedAccountTypes().ToList();
            accountTypeList.Insert(0, null);
            return accountTypeList;
        }

        public ObservableCollection<WidgetGroup> LoadPersistedStateData(WidgetsApplicationStateV1 storedState)
        {
            if (storedState == null)
            {
                throw new ArgumentNullException("storedState");
            }

            if (this.availableDependencies == null)
            {
                this.availableDependencies = InitialiseSupportedDependenciesArray();
            }

            WidgetGroups = new ObservableCollection<WidgetGroup>(this.widgetService.PrepareWidgets(storedState.WidgetStates));
            UpdateAllWidgets();
            foreach (WidgetGroup group in WidgetGroups)
            {
                foreach (Widget widget in @group.Widgets.Where(widget => widget.RecommendedTimeIntervalUpdate != null))
                {
                    ScheduledWidgetUpdate(widget);
                }
            }

            return WidgetGroups;
        }

        public void NotifyOfDependencyChange<T>(object dependency)
        {
            NotifyOfDependencyChangeInternal(dependency, typeof(T));
        }

        public void NotifyOfDependencyChange(object dependency)
        {
            if (dependency == null)
            {
                return;
            }
            NotifyOfDependencyChangeInternal(dependency, dependency.GetType());
        }

        public WidgetsApplicationStateV1 PreparePersistentStateData()
        {
            return new WidgetsApplicationStateV1
            {
                WidgetStates = WidgetGroups.SelectMany(group => group.Widgets).Select(CreateWidgetState).ToList()
            };
        }

        public void RemoveUserDefinedWidget(IUserDefinedWidget widgetToRemove)
        {
            var fixedProjectWidget = widgetToRemove as FixedBudgetMonitorWidget;
            if (fixedProjectWidget != null)
            {
                // Reassign transactions to Surplus
                var projectBucket = this.bucketRepository.GetByCode(fixedProjectWidget.BucketCode) as FixedBudgetProjectBucket;
                if (projectBucket == null)
                {
                    throw new InvalidOperationException("The fixed project bucket provided doesn't actually appear to be a Fixed Budget Project Bucket");
                }

                fixedProjectWidget.Statement.ReassignFixedProjectTransactions(projectBucket, this.bucketRepository.SurplusBucket);
                this.transactionManagerService.SaveAsync(false);

                // No need to remove it from the Budget, the Budget is actually not aware of these fixed project buckets in any way.
                // Remove from Bucket Repo
                this.bucketRepository.RemoveFixedBudgetProject(projectBucket);
                this.budgetRepository.Save(); // Bucket Repo data is stored in the budget repo however.
            }

            this.widgetRepository.Remove(widgetToRemove);

            var baseWidget = (Widget)widgetToRemove;
            WidgetGroup widgetGroup = WidgetGroups.FirstOrDefault(group => group.Heading == baseWidget.Category);
            if (widgetGroup == null)
            {
                return;
            }

            widgetGroup.Widgets.Remove(baseWidget);
        }

        public void ShowAllWidgets()
        {
            if (WidgetGroups == null)
            {
                return;
            }

            WidgetGroups.ToList().ForEach(g => g.Widgets.ToList().ForEach(w => w.Visibility = true));
        }

        private bool HasDependencySignificantlyChanged(object dependency, Type typeKey)
        {
            var supportsDataChangeDetection = dependency as IDataChangeDetection;
            if (supportsDataChangeDetection == null)
            {
                // Dependency doesn't support change hashes so every change is deemed worthy to trigger an update the UI.
                return true;
            }

            long newHash = supportsDataChangeDetection.SignificantDataChangeHash();
            if (!this.changesHashes.ContainsKey(typeKey))
            {
                this.changesHashes.Add(typeKey, newHash);
                return true;
            }

            bool result = this.changesHashes[typeKey] != newHash;
            this.changesHashes[typeKey] = newHash;
            return result;
        }

        private IDictionary<Type, object> InitialiseSupportedDependenciesArray()
        {
            this.availableDependencies = new Dictionary<Type, object>();
            this.availableDependencies[typeof(StatementModel)] = null;
            this.availableDependencies[typeof(BudgetCollection)] = null;
            this.availableDependencies[typeof(IBudgetCurrencyContext)] = null;
            this.availableDependencies[typeof(LedgerBook)] = null;
            this.availableDependencies[typeof(IBudgetBucketRepository)] = this.bucketRepository;
            this.availableDependencies[typeof(GlobalFilterCriteria)] = null;
            this.availableDependencies[typeof(LedgerCalculation)] = this.ledgerCalculator;
            return this.availableDependencies;
        }

        private void NotifyOfDependencyChangeInternal(object dependency, Type typeKey)
        {
            if (this.availableDependencies == null)
            {
                this.availableDependencies = InitialiseSupportedDependenciesArray();
            }
            this.availableDependencies[typeKey] = dependency;

            if (HasDependencySignificantlyChanged(dependency, typeKey))
            {
                UpdateAllWidgets(typeKey);
            }
        }

        private async void ScheduledWidgetUpdate(Widget widget)
        {
            Debug.Assert(widget.RecommendedTimeIntervalUpdate != null);
            this.logger.LogInfo(
                l => l.Format(
                    "Scheduling \"{0}\" widget to update every {1} minutes.",
                    widget.Name,
                    widget.RecommendedTimeIntervalUpdate.Value.TotalMinutes));

            // Run the scheduling on a different thread.
            await Task.Run(
                async () =>
                {
                    while (true)
                    {
                        await Task.Delay(widget.RecommendedTimeIntervalUpdate.Value);
                        this.logger.LogInfo(
                            l => l.Format(
                                "Scheduled Update for \"{0}\" widget. Will run again after {1} minutes. ThreadId: {2}",
                                widget.Name,
                                widget.RecommendedTimeIntervalUpdate.Value.TotalMinutes,
                                Thread.CurrentThread.ManagedThreadId));
                        UpdateWidget(widget);
                    }
                });
        }

        private void UpdateAllWidgets(params Type[] filterDependencyTypes)
        {
            if (WidgetGroups == null || WidgetGroups.None())
            {
                // Widget Groups have not yet been initialised and persistent state has not yet been loaded.
                return;
            }

            if (filterDependencyTypes != null && filterDependencyTypes.Length > 0)
            {
                // targeted update
                List<Widget> affectedWidgets = WidgetGroups.SelectMany(group => group.Widgets)
                    .Where(w => w.Dependencies.Any(filterDependencyTypes.Contains))
                    .ToList();
                affectedWidgets.ForEach(UpdateWidget);
            }
            else
            {
                // update all
                WidgetGroups.SelectMany(group => group.Widgets).ToList().ForEach(UpdateWidget);
            }
        }

        private void UpdateWidget(Widget widget)
        {
            if (widget.Dependencies == null || widget.Dependencies.None())
            {
                widget.Update();
                return;
            }

            var parameters = new object[widget.Dependencies.Count()];
            var index = 0;
            foreach (Type dependencyType in widget.Dependencies)
            {
                if (!this.availableDependencies.ContainsKey(dependencyType))
                {
                    // If you get an exception here first check the InitialiseSupportedDependenciesArray method.
                    throw new NotSupportedException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "The requested dependency {0} for the widget {1} is not supported.",
                            dependencyType.Name,
                            widget.Name));
                }

                parameters[index++] = this.availableDependencies[dependencyType];
            }

            widget.Update(parameters);
        }

        private Widget UpdateWidgetCollectionWithNewAddition(Widget baseWidget)
        {
            WidgetGroup widgetGroup = WidgetGroups.FirstOrDefault(group => @group.Heading == baseWidget.Category);
            if (widgetGroup == null)
            {
                widgetGroup = new WidgetGroup { Heading = baseWidget.Category, Widgets = new ObservableCollection<Widget>() };
                WidgetGroups.Add(widgetGroup);
            }

            widgetGroup.Widgets.Add(baseWidget);
            UpdateAllWidgets();
            return baseWidget;
        }

        private static WidgetPersistentState CreateWidgetState(Widget widget)
        {
            var multiInstanceWidget = widget as IUserDefinedWidget;
            if (multiInstanceWidget != null)
            {
                var surprisePaymentWidget = multiInstanceWidget as SurprisePaymentWidget;
                if (surprisePaymentWidget == null)
                {
                    return new MultiInstanceWidgetState
                    {
                        Id = multiInstanceWidget.Id,
                        Visible = multiInstanceWidget.Visibility,
                        WidgetType = multiInstanceWidget.WidgetType.FullName
                    };
                }

                return new SurprisePaymentWidgetPersistentState
                {
                    Id = surprisePaymentWidget.Id,
                    Visible = surprisePaymentWidget.Visibility,
                    WidgetType = surprisePaymentWidget.WidgetType.FullName,
                    PaymentStartDate = surprisePaymentWidget.StartPaymentDate,
                    Frequency = surprisePaymentWidget.Frequency
                };
            }

            return new WidgetPersistentState
            {
                Visible = widget.Visibility,
                WidgetType = widget.GetType().FullName
            };
        }
    }
}