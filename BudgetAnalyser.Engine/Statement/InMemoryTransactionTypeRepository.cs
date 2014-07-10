using System.Collections.Concurrent;

namespace BudgetAnalyser.Engine.Statement
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class InMemoryTransactionTypeRepository : ITransactionTypeRepository
    {
        private readonly ConcurrentDictionary<string, TransactionType> transactionTypes = new ConcurrentDictionary<string, TransactionType>();

        public TransactionType GetOrCreateNew(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            return this.transactionTypes.GetOrAdd(name, n => new NamedTransaction(n));
        }
    }
}