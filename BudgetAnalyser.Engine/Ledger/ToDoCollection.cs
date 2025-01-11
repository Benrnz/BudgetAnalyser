using System.Collections.ObjectModel;

namespace BudgetAnalyser.Engine.Ledger;

/// <summary>
///     A list of tasks that should be carried out during the ledger book reconciliation.
/// </summary>
public class ToDoCollection : ObservableCollection<ToDoTask>
{
    /// <summary>
    ///     Removes the specified task, only if it is allowed to be deleted by the user. Use this method primarily and in the
    ///     UI.
    /// </summary>
    /// <exception cref="System.ArgumentNullException"></exception>
    public new bool Remove(ToDoTask task)
    {
        if (task is null)
        {
            throw new ArgumentNullException(nameof(task));
        }

        return task.CanDelete && base.Remove(task);
    }

    /// <summary>
    ///     Forced removal of a task.
    /// </summary>
    /// <exception cref="System.ArgumentNullException"></exception>
    public bool RemoveReminderTask(ToDoTask task)
    {
        if (task is null)
        {
            throw new ArgumentNullException(nameof(task));
        }

        return Remove(task);
    }

    /// <summary>
    ///     Removes all items from the collection.
    /// </summary>
    protected override void ClearItems()
    {
        foreach (var task in this.ToArray())
        {
            Remove(task);
        }
    }

    /// <summary>
    ///     Removes the item at the specified index of the collection.
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove.</param>
    protected override void RemoveItem(int index)
    {
        var task = this[index];
        if (task.CanDelete)
        {
            base.RemoveItem(index);
        }
    }
}
