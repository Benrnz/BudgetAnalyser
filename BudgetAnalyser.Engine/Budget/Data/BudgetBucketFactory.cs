namespace BudgetAnalyser.Engine.Budget.Data;

/// <summary>
///     Converts DTO's to models, and vice versa.
/// </summary>
[AutoRegisterWithIoC(SingleInstance = true)]
internal class BudgetBucketFactory : IBudgetBucketFactory
{
    public BudgetBucket BuildModel(BudgetBucketDto dto)
    {
        if (dto is null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        switch (dto.Type)
        {
            case BucketDtoType.Income:
                return new IncomeBudgetBucket();
            case BucketDtoType.PayCreditCard:
            case BucketDtoType.Surplus:
                throw new NotSupportedException("You may not create multiple instances of the Pay Credit Card or Surplus buckets.");
            case BucketDtoType.SavedUpForExpense:
            case BucketDtoType.SavingsCommitment:
                // Keeping SavingsCommitment here for converting old files, it is an obsolete bucket type not to be used.
                return new SavedUpForExpenseBucket();
            case BucketDtoType.SpentPeriodicallyExpense:
            case BucketDtoType.SpentMonthlyExpense:
                // Keeping SpentMonthlyExpense here for converting old files, it is an obsolete bucket type not to be used.
                return new SpentPerPeriodExpenseBucket();
            case BucketDtoType.FixedBudgetProject:
                var f = (FixedBudgetBucketDto)dto;
                return new FixedBudgetProjectBucket(f.Code, f.Description, f.FixedBudgetAmount, f.Created);
            default:
                throw new NotSupportedException("Unsupported Bucket type detected: " + dto);
        }
    }

    /// <summary>
    ///     Builds a <see cref="BudgetBucketDto" /> based on the model passed in.
    /// </summary>
    public BudgetBucketDto BuildDto(BudgetBucket bucket)
    {
        BudgetBucketDto dto;
        var fixedProjectBucket = bucket as FixedBudgetProjectBucket;
        dto = fixedProjectBucket is not null
            ? new FixedBudgetBucketDto { Created = fixedProjectBucket.Created, FixedBudgetAmount = fixedProjectBucket.FixedBudgetAmount }
            : new BudgetBucketDto();

        dto.Type = SerialiseType(bucket);
        return dto;
    }

    public BucketDtoType SerialiseType(BudgetBucket bucket)
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
            // Note that BucketDtoType.SpentMonthlyExpense is obsolete, if it was set to this during deserialisation, this will auto convert from here.
            return BucketDtoType.SpentPeriodicallyExpense;
        }

        throw new NotSupportedException("Unsupported bucket type detected: " + bucket.GetType().FullName);
    }
}
