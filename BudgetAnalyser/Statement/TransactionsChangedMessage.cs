using Rees.Wpf;

namespace BudgetAnalyser.Statement
{
    /// <summary>
    ///     A message primarily used to notify of a change to the <see cref="StatementViewModel.Transactions" /> collection.
    ///     This does not mean that underlying data has been edited, but a filter has been applied or new transactions added.
    /// </summary>
    public class TransactionsChangedMessage : MessageBase
    {
    }
}
