using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Widgets;

namespace BudgetAnalyser.Engine.Services
{
    /// <summary>
    ///     A service class to retrieve and prepare the Widgets and arrange them in a grouped fashion for display in the UI.
    /// </summary>
    [AutoRegisterWithIoC]
    public class WidgetService : IWidgetService
    {
        private readonly IWidgetRepository widgetRepo;

        public WidgetService([NotNull] IWidgetRepository widgetRepo)
        {
            if (widgetRepo == null)
            {
                throw new ArgumentNullException("widgetRepo");
            }

            this.widgetRepo = widgetRepo;
        }

        public IEnumerable<WidgetGroup> PrepareWidgets(IEnumerable<WidgetPersistentState> storedStates)
        {
            if (storedStates != null)
            {
                List<Widget> widgets = this.widgetRepo.GetAll().ToList();
                foreach (WidgetPersistentState widgetState in storedStates)
                {
                    WidgetPersistentState stateClone = widgetState;
                    var multiInstanceState = widgetState as MultiInstanceWidgetState;
                    if (multiInstanceState != null)
                    {
                        // MultiInstance widgets need to be created at this point.  The App State data is required to create them.
                        IMultiInstanceWidget newIdWidget = this.widgetRepo.Create(multiInstanceState.WidgetType, multiInstanceState.Id);
                        newIdWidget.Visibility = multiInstanceState.Visible;
                    }
                    else
                    {
                        // Ordinary widgets will already exist in the repository as they are single instance per class.
                        Widget typedWidget = widgets.FirstOrDefault(w => w.GetType().FullName == stateClone.WidgetType);
                        if (typedWidget != null)
                        {
                            typedWidget.Visibility = widgetState.Visible;
                        }
                    }
                }
            }

            return this.widgetRepo.GetAll()
                .GroupBy(w => w.Category)
                .Select(group => new WidgetGroup { Heading = group.Key, Widgets = new ObservableCollection<Widget>(group) });
        }
    }
}