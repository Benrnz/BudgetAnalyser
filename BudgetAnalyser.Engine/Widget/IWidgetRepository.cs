using System.Collections.Generic;

namespace BudgetAnalyser.Engine.Widget
{
    public interface IWidgetRepository
    {
        IEnumerable<Widget> GetAll();
    }
}
