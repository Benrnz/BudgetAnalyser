using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser.Engine.Widgets
{
    public class WidgetsApplicationStateV1 : IPersistent
    {
        public int LoadSequence => 100;

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Required for serialisation")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Required for serialisation")]
        public List<WidgetPersistentState> WidgetStates { get; set; }
    }
}