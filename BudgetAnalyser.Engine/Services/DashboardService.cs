using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.Widgets;

namespace BudgetAnalyser.Engine.Services
{
    public interface IDashboardService
    {
        event EventHandler WidgetRefreshRequested;

        Widget CreateNewBucketMonitorWidget(IList<WidgetGroup> widgetGroups, string bucketCode);

        IDictionary<Type, object> InitialiseSupportedDependenciesArray();
        
        IEnumerable<WidgetPersistentState> PreparePersistentData(IEnumerable<WidgetGroup> widgetGroups);

        ObservableCollection<WidgetGroup> ViewWidgetGroups(IEnumerable<WidgetPersistentState> storedState);
        
        void RemoveBucketMonitorWidget(IList<WidgetGroup> widgetGroups, Widget widgetToRemove);
    }

    public class DashboardService : IDashboardService
    {
        private readonly IBudgetBucketRepository bucketRepository;
        private readonly LedgerCalculation ledgerCalculator;
        private readonly IWidgetRepository widgetRepository;
        private readonly IWidgetService widgetService;

        public DashboardService(
            [NotNull] IWidgetService widgetService,
            [NotNull] IWidgetRepository widgetRepository,
            [NotNull] IBudgetBucketRepository bucketRepository,
            [NotNull] LedgerCalculation ledgerCalculator)
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

            if (ledgerCalculator == null)
            {
                throw new ArgumentNullException("ledgerCalculator");
            }

            this.widgetService = widgetService;
            this.widgetRepository = widgetRepository;
            this.bucketRepository = bucketRepository;
            this.ledgerCalculator = ledgerCalculator;
            RegularlyRefreshWidgetStatus();
        }

        public event EventHandler WidgetRefreshRequested;

        public Widget CreateNewBucketMonitorWidget(IList<WidgetGroup> widgetGroups, string bucketCode)
        {
            if (widgetGroups.SelectMany(group => group.Widgets).OfType<BudgetBucketMonitorWidget>().Any(w => w.BucketCode == bucketCode))
            {
                // Bucket code already exists - so already has a bucket monitor widget.
                return null;
            }

            IMultiInstanceWidget widget = this.widgetRepository.Create(typeof(BudgetBucketMonitorWidget).FullName, bucketCode);
            var baseWidget = (Widget)widget;
            WidgetGroup widgetGroup = widgetGroups.FirstOrDefault(group => group.Heading == baseWidget.Category);
            if (widgetGroup == null)
            {
                widgetGroup = new WidgetGroup { Heading = baseWidget.Category, Widgets = new ObservableCollection<Widget>() };
                widgetGroups.Add(widgetGroup);
            }

            widgetGroup.Widgets.Add(baseWidget);
        }

        public IDictionary<Type, object> InitialiseSupportedDependenciesArray()
        {
            var availableDependencies = new Dictionary<Type, object>();
            availableDependencies[typeof(StatementModel)] = null;
            availableDependencies[typeof(BudgetCollection)] = null;
            availableDependencies[typeof(IBudgetCurrencyContext)] = null;
            availableDependencies[typeof(LedgerBook)] = null;
            availableDependencies[typeof(IBudgetBucketRepository)] = this.bucketRepository;
            availableDependencies[typeof(GlobalFilterCriteria)] = null;
            availableDependencies[typeof(LedgerCalculation)] = this.ledgerCalculator;

            return availableDependencies;
        }

        public IEnumerable<WidgetPersistentState> PreparePersistentData(IEnumerable<WidgetGroup> widgetGroups)
        {
            return widgetGroups.SelectMany(group => group.Widgets).Select(CreateWidgetState);
        }

        public ObservableCollection<WidgetGroup> ViewWidgetGroups(IEnumerable<WidgetPersistentState> storedState)
        {
            return new ObservableCollection<WidgetGroup>(this.widgetService.PrepareWidgets(storedState));
        }

        public void RemoveBucketMonitorWidget(IList<WidgetGroup> widgetGroups, Widget widgetToRemove)
        {
            WidgetGroup widgetGroup = widgetGroups.FirstOrDefault(group => group.Heading == widgetToRemove.Category);
            if (widgetGroup == null)
            {
                return;
            }

            widgetGroup.Widgets.Remove(widgetToRemove);
        }

        private static WidgetPersistentState CreateWidgetState(Widget widget)
        {
            var multiInstanceWidget = widget as IMultiInstanceWidget;
            if (multiInstanceWidget != null)
            {
                return new MultiInstanceWidgetState
                {
                    Id = multiInstanceWidget.Id,
                    Visible = multiInstanceWidget.Visibility,
                    WidgetType = multiInstanceWidget.WidgetType.FullName,
                };
            }

            return new WidgetPersistentState
            {
                Visible = widget.Visibility,
                WidgetType = widget.GetType().FullName,
            };
        }

        private async void RegularlyRefreshWidgetStatus()
        {
            while (true)
            {
                // TODO Consider changing to use a task for every widget with a time dependency.
                await Task.Delay(TimeSpan.FromSeconds(60));
                EventHandler handler = WidgetRefreshRequested;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }
        }
    }
}