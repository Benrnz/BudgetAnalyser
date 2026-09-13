namespace BudgetAnalyser.Engine.Persistence;

public record GlobalFilterDto(DateOnly? BeginDate = null, DateOnly? EndDate = null);
