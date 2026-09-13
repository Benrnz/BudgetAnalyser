using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Budget;

public class MapperBudgetBucketToDto2Test
{
    private readonly MapperBudgetBucketToDto2 mapper;

    public MapperBudgetBucketToDto2Test()
    {
        this.mapper = new MapperBudgetBucketToDto2();
    }

    [Fact]
    public void ToDto_ShouldMapFixedBudgetProjectBucketToFixedBudgetBucketDto()
    {
        // Arrange
        var bucket = new FixedBudgetProjectBucket("CODE", "Description", 1000, DateTime.Now.ToUniversalTime()) { Active = true };

        // Act
        var dto = this.mapper.ToDto(bucket);

        // Assert
        dto.ShouldBeOfType<FixedBudgetBucketDto>();
        dto.Code.ShouldBe(bucket.Code);
        dto.Description.ShouldBe(bucket.Description);
        dto.Active.ShouldBe(bucket.Active);
        dto.Type.ShouldBe(BucketDtoType.FixedBudgetProject);
        ((FixedBudgetBucketDto)dto).FixedBudgetAmount.ShouldBe(bucket.FixedBudgetAmount);
        ((FixedBudgetBucketDto)dto).Created.ShouldBe(bucket.Created);
    }

    [Fact]
    public void ToDto_ShouldThrowArgumentNullException_WhenBucketIsNull()
    {
        // Act & Assert
        Should.Throw<NullReferenceException>(() => this.mapper.ToDto(null!));
    }

    [Fact]
    public void ToModel_ShouldMapFixedBudgetBucketDtoToFixedBudgetProjectBucket()
    {
        // Arrange
        var dto = new FixedBudgetBucketDto
        (
            Code: "CODE",
            Description: "Description",
            Active: true,
            FixedBudgetAmount: 1000,
            Created: DateTime.Now);

        // Act
        var bucket = this.mapper.ToModel(dto);

        // Assert
        bucket.ShouldBeOfType<FixedBudgetProjectBucket>();
        bucket.Code.ShouldBe(dto.Code);
        bucket.Description.ShouldBe(dto.Description);
        bucket.Active.ShouldBe(dto.Active);
        ((FixedBudgetProjectBucket)bucket).FixedBudgetAmount.ShouldBe(dto.FixedBudgetAmount);
        ((FixedBudgetProjectBucket)bucket).Created.ShouldBe(dto.Created);
    }

    [Fact]
    public void ToModel_ShouldThrowArgumentNullException_WhenDtoIsNull()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => this.mapper.ToModel(null!));
    }

    [Fact]
    public void ToModel_ShouldThrowDataFormatException_WhenDtoTypeIsUnsupported()
    {
        // Arrange
        var dto = new BudgetBucketDto(Type: (BucketDtoType)999, Code: "XXX", Description: "XXX", Active: false);

        // Act & Assert
        Should.Throw<DataFormatException>(() => this.mapper.ToModel(dto));
    }
}
