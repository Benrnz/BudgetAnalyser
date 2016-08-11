using System;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Budget.Data
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    internal class BudgetBucketFactory : IBudgetBucketFactory
    {
        public BudgetBucket BuildModel([NotNull] BudgetBucketDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            switch (dto.Type)
            {
                case BucketDtoType.Income:
                    return new IncomeBudgetBucket();
                case BucketDtoType.PayCreditCard:
                case BucketDtoType.Surplus:
                    throw new NotSupportedException("You may not create multiple instances of the Pay Credit Card or Surplus buckets.");
                case BucketDtoType.SavedUpForExpense:
                    return new SavedUpForExpenseBucket();
                case BucketDtoType.SavingsCommitment:
                    return new SavingsCommitmentBucket();
                case BucketDtoType.SpentMonthlyExpense:
                    return new SpentMonthlyExpenseBucket();
                case BucketDtoType.FixedBudgetProject:
                    var f = (FixedBudgetBucketDto) dto;
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
            if (fixedProjectBucket != null)
            {
                dto = new FixedBudgetBucketDto
                {
                    Created = fixedProjectBucket.Created,
                    FixedBudgetAmount = fixedProjectBucket.FixedBudgetAmount
                };
            }
            else
            {
                dto = new BudgetBucketDto();
            }

            dto.Type = SerialiseType(bucket);
            return dto;
        }

        public BucketDtoType SerialiseType([NotNull] BudgetBucket bucket)
        {
            if (bucket == null)
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

            if (bucket is SpentMonthlyExpenseBucket)
            {
                return BucketDtoType.SpentMonthlyExpense;
            }

            if (bucket is SavingsCommitmentBucket)
            {
                return BucketDtoType.SavingsCommitment;
            }

            throw new NotSupportedException("Unsupported bucket type detected: " + bucket.GetType().FullName);
        }
    }
}