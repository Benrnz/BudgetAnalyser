using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Widget
{
    public interface IWidgetRepository
    {
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Prefered term in repository")]
        IEnumerable<Widget> GetAll();
    }
}