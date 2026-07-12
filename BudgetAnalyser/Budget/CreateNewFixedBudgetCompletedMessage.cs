using Rees.Wpf;

namespace BudgetAnalyser.Budget;

public class CreateNewFixedBudgetCompletedMessage(Guid correlationId, bool canceled) : MessageBase
{
    public Guid CorrelationId { get; } = correlationId;

    public bool Canceled { get; } = canceled;
}
