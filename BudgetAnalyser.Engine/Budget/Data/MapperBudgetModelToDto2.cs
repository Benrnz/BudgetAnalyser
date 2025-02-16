using Rees.TangyFruitMapper;

namespace BudgetAnalyser.Engine.Budget.Data;

public class MapperBudgetModelToDto2(IBudgetBucketRepository bucketRepo) : IDtoMapper<BudgetModelDto, BudgetModel>
{
    private readonly IDtoMapper<ExpenseDto, Expense> mapperExpense = new MapperExpenseToDto2(bucketRepo);
    private readonly IDtoMapper<IncomeDto, Income> mapperIncome = new MapperIncomeToDto2(bucketRepo);

    public BudgetModelDto ToDto(BudgetModel model)
    {
        return new BudgetModelDto
        {
            Expenses = model.Expenses.Select(this.mapperExpense.ToDto).ToList(),
            Incomes = model.Incomes.Select(this.mapperIncome.ToDto).ToList(),
            Name = model.Name,
            BudgetCycle = model.BudgetCycle,
            EffectiveFrom = model.EffectiveFrom,
            LastModified = model.LastModified,
            LastModifiedComment = model.LastModifiedComment
        };
    }

    public BudgetModel ToModel(BudgetModelDto dto)
    {
        return new BudgetModel
        {
            BudgetCycle = dto.BudgetCycle,
            EffectiveFrom = dto.EffectiveFrom,
            LastModified = dto.LastModified ?? DateTime.Now,
            LastModifiedComment = dto.LastModifiedComment,
            Name = dto.Name,
            Incomes = dto.Incomes.Select(this.mapperIncome.ToModel),
            Expenses = dto.Expenses.Select(this.mapperExpense.ToModel)
        };
    }
}
