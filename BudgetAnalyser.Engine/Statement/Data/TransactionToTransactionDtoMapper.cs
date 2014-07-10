namespace BudgetAnalyser.Engine.Statement.Data
{
    public class TransactionToTransactionDtoMapper
    {
        public TransactionDto Map(Transaction transaction)
        {
            return new TransactionDto()
            {
                AccountType = transaction.AccountType == null ? null : transaction.AccountType.Name,
                Amount = transaction.Amount,
                BudgetBucketCode = transaction.BudgetBucket == null ? null : transaction.BudgetBucket.Code,
                Date = transaction.Date,
                Description = transaction.Description,
                Id = transaction.Id,
                Reference1 = transaction.Reference1,
                Reference2 = transaction.Reference2,
                Reference3 = transaction.Reference3,
                TransactionType = transaction.TransactionType == null ? null : transaction.TransactionType.Name,
            };
        }
    }
}