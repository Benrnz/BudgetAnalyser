using System.Collections.Generic;
using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser.Engine.Widgets
{
    public class WidgetsApplicationStateV1 : IPersistent
    {
        public List<WidgetPersistentState> WidgetStates { get; set; }

        public int LoadSequence
        {
            get { return 100; }
        }
    }
}