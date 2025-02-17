using Rees.TangyFruitMapper;

namespace BudgetAnalyser.Engine.Budget.Data;

[AutoRegisterWithIoC]
public class MapperBudgetModelToDto2(IDtoMapper<ExpenseDto, Expense> mapperExpense, IDtoMapper<IncomeDto, Income> mapperIncome)
    : IDtoMapper<BudgetModelDto, BudgetModel>
{
    private readonly IDtoMapper<ExpenseDto, Expense> mapperExpense = mapperExpense ?? throw new ArgumentNullException(nameof(mapperExpense));
    private readonly IDtoMapper<IncomeDto, Income> mapperIncome = mapperIncome ?? throw new ArgumentNullException(nameof(mapperIncome));

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
