using System;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Statement.Data
{
    public class TransactionDtoToTransactionMapper
    {
        private readonly IAccountTypeRepository accountTypeRepository;
        private readonly IBudgetBucketRepository bucketRepository;
        private readonly ITransactionTypeRepository transactionTypeRepository;

        public TransactionDtoToTransactionMapper(
            [NotNull] IAccountTypeRepository accountTypeRepository, 
            [NotNull] IBudgetBucketRepository bucketRepository, 
            [NotNull] ITransactionTypeRepository transactionTypeRepository)
        {
            if (accountTypeRepository == null)
            {
                throw new ArgumentNullException("accountTypeRepository");
            }

            if (bucketRepository == null)
            {
                throw new ArgumentNullException("bucketRepository");
            }
            if (transactionTypeRepository == null)
            {
                throw new ArgumentNullException("transactionTypeRepository");
            }

            this.accountTypeRepository = accountTypeRepository;
            this.bucketRepository = bucketRepository;
            this.transactionTypeRepository = transactionTypeRepository;
        }

        public Transaction Map(TransactionDto dto)
        {
            return new Transaction
            {
                AccountType = this.accountTypeRepository.GetOrCreateNew(dto.AccountType),
                Amount = dto.Amount,
                BudgetBucket = this.bucketRepository.GetByCode(dto.BudgetBucketCode),
                Date = dto.Date,
                Description = dto.Description,
                Id = dto.Id,
                Reference1 = dto.Reference1,
                Reference2 = dto.Reference2,
                Reference3 = dto.Reference3,
                TransactionType = this.transactionTypeRepository.GetOrCreateNew(dto.TransactionType)
            };
        }
    }
}
