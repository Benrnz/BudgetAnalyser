using BudgetAnalyser.Engine.Statement;
using Rees.Wpf;

namespace BudgetAnalyser.Statement;

/// <summary>
///     A message to notify interested parties that the transactions-model is ready for use.  Can be used to indicate a new transactions-model has been loaded.
/// </summary>
public class TransactionSetModelReadyMessage : MessageBase
{
    public TransactionSetModelReadyMessage(TransactionSetModel? transactionsSet)
    {
        TransactionsSetModel = transactionsSet;
    }

    /// <summary>
    ///     The new transactions-model.  This may be null if the transactions-model is closed.
    /// </summary>
    public TransactionSetModel? TransactionsSetModel { get; private set; }
}
