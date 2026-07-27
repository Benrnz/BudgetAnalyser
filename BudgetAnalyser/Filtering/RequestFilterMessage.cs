using BudgetAnalyser.Engine;
using Rees.Wpf;

namespace BudgetAnalyser.Filtering;

public class RequestFilterMessage : MessageBase
{
    public RequestFilterMessage(object? sender)
    {
        Sender = sender;
    }

    public GlobalFilterCriteria? Criteria { get; set; }
}
