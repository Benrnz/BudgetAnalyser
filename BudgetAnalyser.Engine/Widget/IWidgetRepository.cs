using System.Collections.Generic;

namespace BudgetAnalyser.Engine.Widget
{
    public interface IWidgetRepository
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification="Prefered term in repository")]
        IEnumerable<Widget> GetAll();
    }
}