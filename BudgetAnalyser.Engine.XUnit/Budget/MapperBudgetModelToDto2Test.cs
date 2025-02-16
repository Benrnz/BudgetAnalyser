using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.Engine.XUnit.TestData;
using NSubstitute;
using Rees.TangyFruitMapper;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Budget;

public class MapperBudgetModelToDto2Test
{
    private readonly IBudgetBucketRepository bucketRepo;
    private readonly MapperBudgetModelToDto2 mapper;
    private readonly IDtoMapper<ExpenseDto, Expense> mapperExpense;
    private readonly IDtoMapper<IncomeDto, Income> mapperIncome;

    public MapperBudgetModelToDto2Test()
    {
        this.bucketRepo = Substitute.For<IBudgetBucketRepository>();
        this.mapperExpense = Substitute.For<IDtoMapper<ExpenseDto, Expense>>();
        this.mapperIncome = Substitute.For<IDtoMapper<IncomeDto, Income>>();
        this.mapper = new MapperBudgetModelToDto2(this.bucketRepo);
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
        this.mapperExpense.ToDto(Arg.Any<Expense>()).Returns(new ExpenseDto());
        this.mapperIncome.ToDto(Arg.Any<Income>()).Returns(new IncomeDto());

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
    public void ToDto_ShouldThrowArgumentNullException_WhenBudgetModelIsNull()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => this.mapper.ToDto(null));
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
            Expenses = [new ExpenseDto()],
            Incomes = [new IncomeDto()]
        };
        this.mapperExpense.ToModel(Arg.Any<ExpenseDto>()).Returns(
            new Expense { Bucket = BudgetBucketTestData.BudgetModelTestData1Buckets.Single(b => b.Code == TestDataConstants.DoctorBucketCode), Amount = 200M });
        this.mapperIncome.ToModel(Arg.Any<IncomeDto>()).Returns(
            new Income { Bucket = BudgetBucketTestData.BudgetModelTestData1Buckets.Single(b => b.Code == TestDataConstants.IncomeBucketCode), Amount = 2000M });

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

    [Fact]
    public void ToModel_ShouldThrowArgumentNullException_WhenBudgetModelDtoIsNull()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => this.mapper.ToModel(null));
    }
}
