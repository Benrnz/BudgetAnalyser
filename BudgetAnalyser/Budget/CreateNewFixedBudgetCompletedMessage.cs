using Rees.Wpf;

namespace BudgetAnalyser.Budget;

public class CreateNewFixedBudgetCompletedMessage(Guid correlationId, bool canceled) : MessageBase
{
    public decimal Amount { get; init; }

    public bool Canceled { get; } = canceled;

    public string Code { get; init; } = string.Empty;
    public Guid CorrelationId { get; } = correlationId;

    public string Description { get; init; } = string.Empty;
}
