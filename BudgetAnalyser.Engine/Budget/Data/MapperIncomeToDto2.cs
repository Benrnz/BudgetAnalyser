using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.Budget.Data;

[AutoRegisterWithIoC]
public class MapperIncomeToDto2(IBudgetBucketRepository bucketRepo) : IDtoMapper<IncomeDto, Income>
{
    private readonly IBudgetBucketRepository bucketRepo = bucketRepo ?? throw new ArgumentNullException(nameof(bucketRepo));

    public IncomeDto ToDto(Income model)
    {
        return new IncomeDto { Amount = model.Amount, BudgetBucketCode = model.Bucket.Code };
    }

    public Income ToModel(IncomeDto dto)
    {
        return new Income
        {
            Amount = dto.Amount,
            Bucket = this.bucketRepo.GetByCode(dto.BudgetBucketCode) ?? throw new DataFormatException($"Invalid Income Budget Bucket '{dto.BudgetBucketCode}' found in BudgetModel file.")
        };
    }
}
