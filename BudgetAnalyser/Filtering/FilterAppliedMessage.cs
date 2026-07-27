using BudgetAnalyser.Engine;
using Rees.Wpf;

namespace BudgetAnalyser.Filtering;

public class FilterAppliedMessage : MessageBase
{
    public FilterAppliedMessage(object? sender, GlobalFilterCriteria criteria)
    {
        Criteria = criteria;
        Sender = sender;
    }

    public GlobalFilterCriteria Criteria { get; private set; }
}
