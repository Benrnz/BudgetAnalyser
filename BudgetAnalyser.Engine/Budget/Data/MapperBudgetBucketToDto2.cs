using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.Budget.Data;

[AutoRegisterWithIoC]
public class MapperBudgetBucketToDto2 : IDtoMapper<BudgetBucketDto, BudgetBucket>
{
    public BudgetBucketDto ToDto(BudgetBucket model)
    {
        var dto = model is FixedBudgetProjectBucket fixedProjectBucket
            ? new FixedBudgetBucketDto
            {
                Type = SerialiseType(model),
                Active = model.Active,
                Code = model.Code,
                Description = model.Description,
                Created = fixedProjectBucket.Created.ToUniversalTime(),
                FixedBudgetAmount = fixedProjectBucket.FixedBudgetAmount
            }
            : new BudgetBucketDto { Type = SerialiseType(model), Active = model.Active, Code = model.Code, Description = model.Description };

        return dto;
    }

    public BudgetBucket ToModel(BudgetBucketDto dto)
    {
        if (dto is null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        switch (dto.Type)
        {
            case BucketDtoType.Income:
                return new IncomeBudgetBucket { Active = dto.Active, Code = dto.Code, Description = dto.Description };
            case BucketDtoType.PayCreditCard:
                // Ignore these, these are created by default whether they are in the file or not.
                return new PayCreditCardBucket { Active = true, Code = PayCreditCardBucket.PayCreditCardCode, Description = dto.Description };
            case BucketDtoType.Surplus:
                return new SurplusBucket { Active = true, Code = SurplusBucket.SurplusCode, Description = dto.Description };
            case BucketDtoType.SavedUpForExpense:
                return new SavedUpForExpenseBucket { Active = dto.Active, Code = dto.Code, Description = dto.Description };
            case BucketDtoType.SpentPeriodicallyExpense:
                return new SpentPerPeriodExpenseBucket { Active = dto.Active, Code = dto.Code, Description = dto.Description };
            case BucketDtoType.FixedBudgetProject:
                var f = (FixedBudgetBucketDto)dto;
                return new FixedBudgetProjectBucket(f.Code, f.Description, f.FixedBudgetAmount, f.Created.ToLocalTime()) { Active = dto.Active };
            default:
                throw new DataFormatException("Unsupported Bucket type detected: " + dto);
        }
    }

    private BucketDtoType SerialiseType(BudgetBucket bucket)
    {
        if (bucket is null)
        {
            throw new ArgumentNullException(nameof(bucket));
        }

        if (bucket is IncomeBudgetBucket)
        {
            return BucketDtoType.Income;
        }

        if (bucket is FixedBudgetProjectBucket)
        {
            return BucketDtoType.FixedBudgetProject;
        }

        if (bucket is SurplusBucket)
        {
            return BucketDtoType.Surplus;
        }

        if (bucket is PayCreditCardBucket)
        {
            return BucketDtoType.PayCreditCard;
        }

        if (bucket is SavedUpForExpenseBucket)
        {
            return BucketDtoType.SavedUpForExpense;
        }

        if (bucket is SpentPerPeriodExpenseBucket)
        {
            return BucketDtoType.SpentPeriodicallyExpense;
        }

        throw new DataFormatException($"Unsupported bucket type detected: {bucket.GetType().FullName}");
    }
}
