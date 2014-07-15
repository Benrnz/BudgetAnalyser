using System;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Statement.Data
{
    [AutoRegisterWithIoC(RegisterAs = typeof(BasicMapper<Transaction, TransactionDto>))]
    public class TransactionToTransactionDtoMapper : BasicMapper<Transaction, TransactionDto>
    {
        public override TransactionDto Map([NotNull] Transaction source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return new TransactionDto()
            {
                AccountType = source.AccountType == null ? null : source.AccountType.Name,
                Amount = source.Amount,
                BudgetBucketCode = source.BudgetBucket == null ? null : source.BudgetBucket.Code,
                Date = source.Date,
                Description = source.Description,
                Id = source.Id,
                Reference1 = source.Reference1,
                Reference2 = source.Reference2,
                Reference3 = source.Reference3,
                TransactionType = source.TransactionType == null ? null : source.TransactionType.Name,
            };
        }
    }
}