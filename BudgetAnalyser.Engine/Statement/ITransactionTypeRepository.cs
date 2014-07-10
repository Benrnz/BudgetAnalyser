namespace BudgetAnalyser.Engine.Statement
{
    public interface ITransactionTypeRepository
    {
        TransactionType GetOrCreateNew(string name);
    }
}