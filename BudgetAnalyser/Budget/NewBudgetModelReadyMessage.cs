using BudgetAnalyser.Engine.Budget;
using Rees.Wpf;

namespace BudgetAnalyser.Budget;

/// <summary>
///     Broadcast when the new budget model dialog completes successfully.
/// </summary>
public class NewBudgetModelReadyMessage(Guid correlationId, DateOnly effectiveFrom, BudgetCycle budgetCycle) : MessageBase
{
    public BudgetCycle BudgetCycle { get; } = budgetCycle;

    public Guid CorrelationId { get; } = correlationId;
    public DateOnly EffectiveFrom { get; } = effectiveFrom;
}
