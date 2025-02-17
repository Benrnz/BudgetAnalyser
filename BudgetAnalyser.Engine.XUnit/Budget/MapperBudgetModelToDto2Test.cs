using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using NSubstitute;
using Rees.TangyFruitMapper;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Budget;

public class MapperBudgetModelToDto2Test
{
    private MapperBudgetModelToDto2 mapper;
    private readonly IDtoMapper<ExpenseDto, Expense> mapperExpense;
    private readonly IDtoMapper<IncomeDto, Income> mapperIncome;

    public MapperBudgetModelToDto2Test()
    {
        var bucketRepo = new BucketBucketRepoAlwaysFind();
        this.mapperExpense = new MapperExpenseToDto2(bucketRepo);
        this.mapperIncome = new MapperIncomeToDto2(bucketRepo);
        this.mapper = new MapperBudgetModelToDto2(this.mapperExpense, this.mapperIncome);
    }

    [Fact]
    public void ExpensesSumShouldBeMapped()
    {
        var budgetModel = BudgetModelTestData.CreateTestData1();
        var dto = this.mapper.ToDto(budgetModel);

        dto.Expenses.Sum(i => i.Amount).ShouldBe(budgetModel.Expenses.Sum(i => i.Amount));
    }

    [Fact]
    public void ExpensesSumShouldBeMappedThereAndBack()
    {
        var bucketRepo = new BucketBucketRepoAlwaysFind();
        this.mapper = new MapperBudgetModelToDto2(new MapperExpenseToDto2(bucketRepo), new MapperIncomeToDto2(bucketRepo));
        var budgetModel = BudgetModelTestData.CreateTestData1();
        var dto = this.mapper.ToDto(budgetModel);
        var mappedModel = this.mapper.ToModel(dto);

        mappedModel.Expenses.Sum(i => i.Amount).ShouldBe(budgetModel.Expenses.Sum(i => i.Amount));
    }


    [Fact]
    public void ToDto_ShouldMapBudgetModelToBudgetModelDto()
    {
        // Arrange
        var budgetModel = BudgetModelTestData.CreateTestData1();

        // Act
        var dto = this.mapper.ToDto(budgetModel);

        // Assert
        dto.ShouldBeOfType<BudgetModelDto>();
        dto.Name.ShouldBe(budgetModel.Name);
        dto.BudgetCycle.ShouldBe(budgetModel.BudgetCycle);
        dto.EffectiveFrom.ShouldBe(budgetModel.EffectiveFrom);
        dto.LastModified.ShouldBe(budgetModel.LastModified);
        dto.LastModifiedComment.ShouldBe(budgetModel.LastModifiedComment);
        dto.Expenses.Count.ShouldBe(budgetModel.Expenses.Count());
        dto.Incomes.Count.ShouldBe(budgetModel.Incomes.Count());
    }

    [Fact]
    public void ToModel_ShouldMapBudgetModelDtoToBudgetModel()
    {
        // Arrange
        var budgetModelDto = new BudgetModelDto
        {
            Name = "Test Budget",
            BudgetCycle = BudgetCycle.Monthly,
            EffectiveFrom = DateTime.Now,
            LastModified = DateTime.Now,
            LastModifiedComment = "Test Comment",
            Expenses = [new ExpenseDto { Amount = 200M, BudgetBucketCode = TestDataConstants.PowerBucketCode }],
            Incomes = [new IncomeDto { Amount = 2000M, BudgetBucketCode = TestDataConstants.IncomeBucketCode }]
        };

        // Act
        var model = this.mapper.ToModel(budgetModelDto);

        // Assert
        model.ShouldBeOfType<BudgetModel>();
        model.Name.ShouldBe(budgetModelDto.Name);
        model.BudgetCycle.ShouldBe(budgetModelDto.BudgetCycle);
        model.EffectiveFrom.ShouldBe(budgetModelDto.EffectiveFrom);
        model.LastModified.ShouldBe(budgetModelDto.LastModified.Value);
        model.LastModifiedComment.ShouldBe(budgetModelDto.LastModifiedComment);
        model.Expenses.Count().ShouldBe(budgetModelDto.Expenses.Count);
        model.Incomes.Count().ShouldBe(budgetModelDto.Incomes.Count);
    }
}
