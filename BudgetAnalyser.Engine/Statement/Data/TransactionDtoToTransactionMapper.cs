using System;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Statement.Data
{
    [AutoRegisterWithIoC(RegisterAs = typeof(BasicMapper<TransactionDto, Transaction>))]
    public class TransactionDtoToTransactionMapper : BasicMapper<TransactionDto, Transaction>
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

        public override Transaction Map([NotNull] TransactionDto source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return new Transaction
            {
                AccountType = this.accountTypeRepository.GetOrCreateNew(source.AccountType),
                Amount = source.Amount,
                BudgetBucket = this.bucketRepository.GetByCode(source.BudgetBucketCode),
                Date = source.Date,
                Description = source.Description,
                Id = source.Id,
                Reference1 = source.Reference1,
                Reference2 = source.Reference2,
                Reference3 = source.Reference3,
                TransactionType = this.transactionTypeRepository.GetOrCreateNew(source.TransactionType)
            };
        }
    }
}
