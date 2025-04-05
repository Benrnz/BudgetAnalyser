using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.Engine.XUnit.TestData;
using NSubstitute;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Budget;

public class MapperExpenseToDto2Test
{
    private readonly IBudgetBucketRepository bucketRepo;
    private readonly MapperExpenseToDto2 mapper;

    public MapperExpenseToDto2Test()
    {
        this.bucketRepo = Substitute.For<IBudgetBucketRepository>();
        this.mapper = new MapperExpenseToDto2(this.bucketRepo);
    }

    [Fact]
    public void ToDto_ShouldMapExpenseToExpenseDto()
    {
        // Arrange
        var carMtcBucket = BudgetBucketTestData.BudgetModelTestData1Buckets.Single(b => b.Code == TestDataConstants.CarMtcBucketCode);
        var expense = new Expense { Amount = 500, Bucket = carMtcBucket };

        // Act
        var dto = this.mapper.ToDto(expense);

        // Assert
        dto.ShouldBeOfType<ExpenseDto>();
        dto.Amount.ShouldBe(expense.Amount);
        dto.BudgetBucketCode.ShouldBe(expense.Bucket.Code);
    }

    [Fact]
    public void ToModel_ShouldMapExpenseDtoToExpense()
    {
        // Arrange
        var dto = new ExpenseDto { Amount = 500, BudgetBucketCode = TestDataConstants.CarMtcBucketCode };
        var carMtcBucket = BudgetBucketTestData.BudgetModelTestData1Buckets.Single(b => b.Code == TestDataConstants.CarMtcBucketCode);
        this.bucketRepo.GetByCode(dto.BudgetBucketCode).Returns(carMtcBucket);

        // Act
        var expense = this.mapper.ToModel(dto);

        // Assert
        expense.ShouldBeOfType<Expense>();
        expense.Amount.ShouldBe(dto.Amount);
        expense.Bucket.ShouldBe(carMtcBucket);
    }

    [Fact]
    public void ToModel_ShouldThrowDataFormatException_WhenBucketCodeIsInvalid()
    {
        // Arrange
        var dto = new ExpenseDto { Amount = 500, BudgetBucketCode = "INVALID" };
        this.bucketRepo.GetByCode(dto.BudgetBucketCode).Returns((BudgetBucket)null);

        // Act & Assert
        Should.Throw<DataFormatException>(() => this.mapper.ToModel(dto));
    }
}
