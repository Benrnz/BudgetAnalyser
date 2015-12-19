using System.Collections.Concurrent;

namespace BudgetAnalyser.Engine.Statement
{
    /// <summary>
    ///     An in memory repository to store <see cref="TransactionType"/>s.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Statement.ITransactionTypeRepository" />
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class InMemoryTransactionTypeRepository : ITransactionTypeRepository
    {
        private readonly ConcurrentDictionary<string, TransactionType> transactionTypes = new ConcurrentDictionary<string, TransactionType>();

        /// <summary>
        /// Gets or creates a <see cref="TransactionType" /> based on the <paramref name="name" /> provided.
        /// </summary>
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