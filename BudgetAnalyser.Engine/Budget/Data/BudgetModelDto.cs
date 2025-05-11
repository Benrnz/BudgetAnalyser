namespace BudgetAnalyser.Engine.Budget.Data;

/// <summary>
///     A Dto object to persist a Budget Model.
/// </summary>
public record BudgetModelDto(BudgetCycle BudgetCycle, DateOnly EffectiveFrom, ExpenseDto[] Expenses, IncomeDto[] Incomes, DateTime? LastModified, string Name)
{
    public string LastModifiedComment { get; init; } = string.Empty;
}
