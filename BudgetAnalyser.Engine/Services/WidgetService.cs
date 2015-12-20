using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BudgetAnalyser.Engine.Widgets;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Services
{
    /// <summary>
    ///     A service class to retrieve and prepare the Widgets and arrange them in a grouped fashion for display in the UI.
    /// </summary>
    [AutoRegisterWithIoC]
    internal class WidgetService : IWidgetService
    {
        private static readonly Dictionary<string, int> GroupSequence = new Dictionary<string, int>
        {
            {WidgetGroup.OverviewSectionName, 1},
            {WidgetGroup.GlobalFilterSectionName, 2},
            {WidgetGroup.MonthlyTrackingSectionName, 3},
            {WidgetGroup.ProjectsSectionName, 4}
        };

        private readonly ILogger logger;
        private readonly IWidgetRepository widgetRepo;

        /// <summary>
        ///     Initializes a new instance of the <see cref="WidgetService" /> class.
        /// </summary>
        /// <param name="widgetRepo">The widget repository.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public WidgetService([NotNull] IWidgetRepository widgetRepo, [NotNull] ILogger logger)
        {
            if (widgetRepo == null)
            {
                throw new ArgumentNullException(nameof(widgetRepo));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.widgetRepo = widgetRepo;
            this.logger = logger;
        }

        /// <summary>
        ///     Prepares the widgets.
        /// </summary>
        /// <param name="storedStates">The stored application state for widgets.</param>
        public IEnumerable<WidgetGroup> PrepareWidgets(IEnumerable<WidgetPersistentState> storedStates)
        {
            if (storedStates != null)
            {
                var widgets = this.widgetRepo.GetAll().ToList();
                foreach (var widgetState in storedStates)
                {
                    var stateClone = widgetState;
                    var multiInstanceState = widgetState as MultiInstanceWidgetState;
                    if (multiInstanceState != null)
                    {
                        CreateMultiInstanceWidget(multiInstanceState);
                    }
                    else
                    {
                        // Ordinary widgets will already exist in the repository as they are single instance per class.
                        var typedWidget = widgets.FirstOrDefault(w => w.GetType().FullName == stateClone.WidgetType);
                        if (typedWidget != null)
                        {
                            typedWidget.Visibility = widgetState.Visible;
                        }
                    }
                }
            }

            return this.widgetRepo.GetAll()
                .GroupBy(w => w.Category)
                .Select(
                    group => new WidgetGroup
                    {
                        Heading = group.Key,
                        Widgets = new ObservableCollection<Widget>(group.OrderBy(w => w.Sequence)),
                        Sequence = GroupSequence[group.Key]
                    })
                .OrderBy(g => g.Sequence).ThenBy(g => g.Heading);
        }

        private void CreateMultiInstanceWidget(MultiInstanceWidgetState multiInstanceState)
        {
            // MultiInstance widgets need to be created at this point.  The App State data is required to create them.
            var newIdWidget = this.widgetRepo.Create(multiInstanceState.WidgetType, multiInstanceState.Id);
            newIdWidget.Visibility = multiInstanceState.Visible;
            newIdWidget.Initialise(multiInstanceState, this.logger);
        }
    }
}