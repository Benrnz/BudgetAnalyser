using BudgetAnalyser.Engine.Transactions;
using Rees.Wpf;

namespace BudgetAnalyser.Transactions;

/// <summary>
///     A message to notify interested parties that the transactionsListModel model is ready for use.  Can be used to indicate a new transactions model has been loaded.
/// </summary>
public class TransactionsListModelReadyMessage(TransactionsListModel? transactionsListModel) : MessageBase
{
    /// <summary>
    ///     The new transactions model.  This may be null if the transaction model is closed.
    /// </summary>
    public TransactionsListModel? Model { get; private set; } = transactionsListModel;
}
