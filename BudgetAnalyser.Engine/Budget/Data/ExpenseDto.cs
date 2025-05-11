namespace BudgetAnalyser.Engine.Budget.Data;

/// <summary>
///     A Dto to persist a Budget Expense object.
/// </summary>
public record ExpenseDto(decimal Amount, string BudgetBucketCode);
