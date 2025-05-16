using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using NSubstitute;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Budget;

public class MapperBudgetCollectionToDto2Test
{
    private readonly MapperBudgetCollectionToDto2 mapper;
    private readonly IDtoMapper<BudgetModelDto, BudgetModel> mapperBudgetModel;

    public MapperBudgetCollectionToDto2Test()
    {
        var bucketRepo = Substitute.For<IBudgetBucketRepository>();
        this.mapperBudgetModel = Substitute.For<IDtoMapper<BudgetModelDto, BudgetModel>>();
        this.mapper = new MapperBudgetCollectionToDto2(bucketRepo, new MapperBudgetModelToDto2(new MapperExpenseToDto2(bucketRepo), new MapperIncomeToDto2(bucketRepo)));
    }

    [Fact]
    public void ShouldMapBudgetsCorrectlyMathematically()
    {
        var budgetCollection = BudgetModelTestData.CreateCollectionWith1And2();
        var bucketRepo = new BudgetBucketRepoAlwaysFind();
        var subject = new MapperBudgetCollectionToDto2(bucketRepo, new MapperBudgetModelToDto2(new MapperExpenseToDto2(bucketRepo), new MapperIncomeToDto2(bucketRepo)));

        // Act
        var dto = subject.ToDto(budgetCollection);
        var mappedModel = subject.ToModel(dto);

        mappedModel.SelectMany(budgetModel => budgetModel.Expenses).Sum(e => e.Amount)
            .ShouldBe(budgetCollection.SelectMany(b => b.Expenses).Sum(e => e.Amount));
        mappedModel.SelectMany(budgetModel => budgetModel.Incomes).Sum(e => e.Amount)
            .ShouldBe(budgetCollection.SelectMany(b => b.Incomes).Sum(e => e.Amount));
    }

    [Fact]
    public void ToDto_ShouldMapBudgetCollectionToBudgetCollectionDto()
    {
        // Arrange
        var budgetCollection = BudgetModelTestData.CreateCollectionWith1And2();
        this.mapperBudgetModel.ToDto(Arg.Any<BudgetModel>()).Returns(
            new BudgetModelDto(
                BudgetCycle.Fortnightly,
                new DateOnly(2018, 01, 01),
                [],
                [],
                DateTime.Now,
                DateTime.Now.ToString()));

        // Act
        var dto = this.mapper.ToDto(budgetCollection);

        // Assert
        dto.ShouldBeOfType<BudgetCollectionDto>();
        dto.StorageKey.ShouldBe(budgetCollection.StorageKey);
        dto.Budgets.Count().ShouldBe(budgetCollection.Count);
    }

    [Fact]
    public void ToModel_ShouldMapBudgetCollectionDtoToBudgetCollection()
    {
        // Arrange
        var budgetCollectionDto = new BudgetCollectionDto(StorageKey: "TestKey", Budgets:
            [
                new BudgetModelDto(BudgetCycle.Monthly, new DateOnly(2018, 01, 01), [], [], DateTime.Now, DateTime.Now.ToString())
            ],
            Buckets: []);
        this.mapperBudgetModel.ToModel(Arg.Any<BudgetModelDto>()).Returns(new BudgetModel());

        // Act
        var collection = this.mapper.ToModel(budgetCollectionDto);

        // Assert
        collection.ShouldBeOfType<BudgetCollection>();
        collection.StorageKey.ShouldBe(budgetCollectionDto.StorageKey);
        collection.Count.ShouldBe(budgetCollectionDto.Budgets.Count());
    }
}
