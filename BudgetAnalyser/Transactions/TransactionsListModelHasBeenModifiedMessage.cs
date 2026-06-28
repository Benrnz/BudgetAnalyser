using Rees.Wpf;

namespace BudgetAnalyser.Transactions;

/// <summary>
///     A message to notify interested parties that the transaction data has been modified.
///     This includes data edits, deletions, and adding new transactions to the collection.
/// </summary>
public class TransactionsListModelHasBeenModifiedMessage : MessageBase
{
}
