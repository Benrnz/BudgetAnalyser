using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Widgets
{
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "Preferred spelling")]
    public class MultiInstanceWidgetState : WidgetPersistentState
    {
        public string Id { get; set; }
    }
}