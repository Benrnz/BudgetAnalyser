namespace BudgetAnalyser.Engine.Persistence;

public record GlobalFilterDto
{
    public DateOnly? BeginDate { get; init; }
    public DateOnly? EndDate { get; init; }
}
