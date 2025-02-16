using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using NSubstitute;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Budget;

public class MapperIncomeToDto2Test
{
    private readonly IBudgetBucketRepository bucketRepo;
    private readonly MapperIncomeToDto2 mapper;

    public MapperIncomeToDto2Test()
    {
        this.bucketRepo = Substitute.For<IBudgetBucketRepository>();
        this.mapper = new MapperIncomeToDto2(this.bucketRepo);
    }

    [Fact]
    public void ToDto_ShouldMapIncomeToIncomeDto()
    {
        // Arrange
        var bucket = new IncomeBudgetBucket { Code = "INCOME" };
        var income = new Income { Amount = 1000, Bucket = bucket };

        // Act
        var dto = this.mapper.ToDto(income);

        // Assert
        dto.ShouldBeOfType<IncomeDto>();
        dto.Amount.ShouldBe(income.Amount);
        dto.BudgetBucketCode.ShouldBe(income.Bucket.Code);
    }

    [Fact]
    public void ToDto_ShouldThrowArgumentNullException_WhenIncomeIsNull()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => this.mapper.ToDto(null));
    }

    [Fact]
    public void ToModel_ShouldMapIncomeDtoToIncome()
    {
        // Arrange
        var dto = new IncomeDto { Amount = 1000, BudgetBucketCode = "INCOME" };
        var bucket = new IncomeBudgetBucket { Code = "INCOME" };
        this.bucketRepo.GetByCode(dto.BudgetBucketCode).Returns(bucket);

        // Act
        var income = this.mapper.ToModel(dto);

        // Assert
        income.ShouldBeOfType<Income>();
        income.Amount.ShouldBe(dto.Amount);
        income.Bucket.ShouldBe(bucket);
    }

    [Fact]
    public void ToModel_ShouldThrowArgumentNullException_WhenIncomeDtoIsNull()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => this.mapper.ToModel(null));
    }

    [Fact]
    public void ToModel_ShouldThrowDataFormatException_WhenBucketCodeIsInvalid()
    {
        // Arrange
        var dto = new IncomeDto { Amount = 1000, BudgetBucketCode = "INVALID" };
        this.bucketRepo.GetByCode(dto.BudgetBucketCode).Returns((BudgetBucket)null);

        // Act & Assert
        Should.Throw<DataFormatException>(() => this.mapper.ToModel(dto));
    }
}
