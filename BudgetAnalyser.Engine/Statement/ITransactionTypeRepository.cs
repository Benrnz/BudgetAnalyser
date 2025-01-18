namespace BudgetAnalyser.Engine.Statement;

/// <summary>
///     A simple repository to serve <see cref="TransactionType" /> instances.
/// </summary>
public interface ITransactionTypeRepository
{
    /// <summary>
    ///     Gets or creates a <see cref="TransactionType" /> based on the <paramref name="name" /> provided.
    /// </summary>
    TransactionType GetOrCreateNew(string name);
}
