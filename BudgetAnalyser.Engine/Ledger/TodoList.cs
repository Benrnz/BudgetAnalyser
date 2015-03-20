using System.Collections.ObjectModel;

namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    ///     A list of tasks that should be carried out during the ledger book reconciliation.
    /// </summary>
    public class TodoList : ObservableCollection<TodoTask>
    {
    }
}