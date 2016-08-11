using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Widgets;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Services
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    internal class DashboardService : IDashboardService
    {
        private readonly IBudgetBucketRepository bucketRepository;
        private readonly IBudgetRepository budgetRepository;
        private readonly ILogger logger;
        private readonly MonitorableDependencies monitoringServices;
        private readonly IWidgetService widgetService;

        public DashboardService(
            [NotNull] IWidgetService widgetService,
            [NotNull] IBudgetBucketRepository bucketRepository,
            [NotNull] IBudgetRepository budgetRepository,
            [NotNull] ILogger logger,
            [NotNull] MonitorableDependencies monitorableDependencies)
        {
            if (widgetService == null)
            {
                throw new ArgumentNullException(nameof(widgetService));
            }

            if (bucketRepository == null)
            {
                throw new ArgumentNullException(nameof(bucketRepository));
            }

            if (budgetRepository == null)
            {
                throw new ArgumentNullException(nameof(budgetRepository));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            if (monitorableDependencies == null) throw new ArgumentNullException(nameof(monitorableDependencies));

            this.widgetService = widgetService;
            this.bucketRepository = bucketRepository;
            this.budgetRepository = budgetRepository;
            this.logger = logger;
            this.monitoringServices = monitorableDependencies;
            this.monitoringServices.DependencyChanged += OnMonitoringServicesDependencyChanged;
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
            if (
                WidgetGroups.SelectMany(group => group.Widgets)
                    .OfType<BudgetBucketMonitorWidget>()
                    .Any(w => w.BucketCode == bucketCode))
            {
                // Bucket code already exists - so already has a bucket monitor widget.
                return null;
            }

            var widget = this.widgetService.Create(typeof(BudgetBucketMonitorWidget).FullName, bucketCode);
            return UpdateWidgetCollectionWithNewAddition((Widget) widget);
        }

        public Widget CreateNewFixedBudgetMonitorWidget(string bucketCode, string description, decimal fixedBudgetAmount)
        {
            if (string.IsNullOrWhiteSpace(bucketCode))
            {
                throw new ArgumentNullException(nameof(bucketCode));
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentNullException(nameof(description));
            }

            if (fixedBudgetAmount <= 0)
            {
                throw new ArgumentException("Fixed Budget amount must be greater than zero.", nameof(fixedBudgetAmount));
            }

            var bucket = this.bucketRepository.CreateNewFixedBudgetProject(bucketCode, description, fixedBudgetAmount);
            this.budgetRepository.SaveAsync();
            var widget = this.widgetService.Create(typeof(FixedBudgetMonitorWidget).FullName, bucket.Code);
            return UpdateWidgetCollectionWithNewAddition((Widget) widget);
        }

        public Widget CreateNewSurprisePaymentMonitorWidget(string bucketCode, DateTime paymentDate,
                                                            WeeklyOrFortnightly frequency)
        {
            if (string.IsNullOrWhiteSpace(bucketCode))
            {
                throw new ArgumentNullException(nameof(bucketCode));
            }

            if (paymentDate == DateTime.MinValue)
            {
                throw new ArgumentException("Payment date is not set.", nameof(paymentDate));
            }

            var bucket = this.bucketRepository.GetByCode(bucketCode);
            if (bucket == null)
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture, "No Bucket with code {0} exists", bucketCode),
                    nameof(bucketCode));
            }

            var widget = this.widgetService.Create(typeof(SurprisePaymentWidget).FullName, bucket.Code);
            var paymentWidget = (SurprisePaymentWidget) widget;
            paymentWidget.StartPaymentDate = paymentDate;
            paymentWidget.Frequency = frequency;
            return UpdateWidgetCollectionWithNewAddition((Widget) widget);
        }

        public ObservableCollection<WidgetGroup> LoadPersistedStateData(WidgetsApplicationState storedState)
        {
            if (storedState == null)
            {
                throw new ArgumentNullException(nameof(storedState));
            }

            WidgetGroups = new ObservableCollection<WidgetGroup>(this.widgetService.PrepareWidgets(storedState.WidgetStates));
            UpdateAllWidgets();
            foreach (var group in WidgetGroups)
            {
                foreach (var widget in @group.Widgets.Where(widget => widget.RecommendedTimeIntervalUpdate != null))
                {
                    ScheduledWidgetUpdate(widget);
                }
            }

            return WidgetGroups;
        }

        public WidgetsApplicationState PreparePersistentStateData()
        {
            IEnumerable<WidgetGroup> widgetStates = WidgetGroups?.ToList() ?? new List<WidgetGroup>();
            return new WidgetsApplicationState
            {
                WidgetStates = widgetStates.SelectMany(group => group.Widgets).Select(CreateWidgetState).ToList()
            };
        }

        public void RemoveUserDefinedWidget(IUserDefinedWidget widgetToRemove)
        {
            var fixedProjectWidget = widgetToRemove as FixedBudgetMonitorWidget;
            if (fixedProjectWidget != null)
            {
                // Reassign transactions to Surplus
                var projectBucket =
                    this.bucketRepository.GetByCode(fixedProjectWidget.BucketCode) as FixedBudgetProjectBucket;
                if (projectBucket == null)
                {
                    throw new InvalidOperationException(
                        "The fixed project bucket provided doesn't actually appear to be a Fixed Budget Project Bucket");
                }

                projectBucket.Active = false;
                fixedProjectWidget.Visibility = false;
                // Not sure I really want to do this. It will mean this data is gone forever.
                // fixedProjectWidget.Statement.ReassignFixedProjectTransactions(projectBucket, this.bucketRepository.SurplusBucket);

                // No need to remove it from the Budget, the Budget is actually not aware of these fixed project buckets in any way.
                // this.bucketRepository.RemoveFixedBudgetProject(projectBucket);
                this.budgetRepository.SaveAsync(); // Bucket Repo data is stored in the budget repo however.
                return;
            }

            this.widgetService.Remove(widgetToRemove);

            var baseWidget = (Widget) widgetToRemove;
            var widgetGroup = WidgetGroups.FirstOrDefault(group => group.Heading == baseWidget.Category);

            widgetGroup?.Widgets.Remove(baseWidget);
        }

        public void ShowAllWidgets()
        {
            WidgetGroups?
                .ToList()
                .ForEach(g => g.Widgets.ToList().ForEach(w => w.Visibility = true));
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

        private void OnMonitoringServicesDependencyChanged(object sender, DependencyChangedEventArgs dependencyChangedEventArgs)
        {
            UpdateAllWidgets(dependencyChangedEventArgs.DependencyType);
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
                                "Scheduled Update for \"{0}\" widget. Will run again after {1} minutes.",
                                widget.Name,
                                widget.RecommendedTimeIntervalUpdate.Value.TotalMinutes));
                        UpdateWidget(widget);
                    }
                    // ReSharper disable once FunctionNeverReturns - intentional timer tick infinite loop
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
            foreach (var dependencyType in widget.Dependencies)
            {
                try
                {
                    parameters[index++] = this.monitoringServices.RetrieveDependency(dependencyType);
                }
                catch (NotSupportedException ex)
                {
                    // If you get an exception here first check the MonitorableDependencies.ctor method.
                    throw new NotSupportedException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "The requested dependency {0} for the widget {1} is not supported.",
                            dependencyType.Name,
                            widget.Name), ex);
                }
            }

            widget.Update(parameters);
        }

        private Widget UpdateWidgetCollectionWithNewAddition(Widget baseWidget)
        {
            var widgetGroup = WidgetGroups.FirstOrDefault(group => @group.Heading == baseWidget.Category);
            if (widgetGroup == null)
            {
                widgetGroup = new WidgetGroup
                {
                    Heading = baseWidget.Category,
                    Widgets = new ObservableCollection<Widget>()
                };
                WidgetGroups.Add(widgetGroup);
            }

            widgetGroup.Widgets.Add(baseWidget);
            UpdateAllWidgets();
            return baseWidget;
        }
    }
}