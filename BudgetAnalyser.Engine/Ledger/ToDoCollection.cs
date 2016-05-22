using System;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Ledger
{
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
        public new bool Remove([NotNull] ToDoTask task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            if (task.CanDelete)
            {
                return base.Remove(task);
            }

            return false;
        }

        /// <summary>
        ///     Forced removal of a task.
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
        public bool RemoveReminderTask([NotNull] ToDoTask task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            task.CanDelete = true;
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
}