namespace BudgetAnalyser.Engine.Budget.Data;

/// <summary>
///     A Dto persistence class to store an Income object.
/// </summary>
public record IncomeDto(decimal Amount, string BudgetBucketCode);
