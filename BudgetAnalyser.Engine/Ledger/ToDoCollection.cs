using System;
using System.Collections.ObjectModel;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    ///     A list of tasks that should be carried out during the ledger book reconciliation.
    /// </summary>
    public class ToDoCollection : ObservableCollection<ToDoTask>
    {
        public new bool Remove([NotNull] ToDoTask task)
        {
            if (task == null)
            {
                throw new ArgumentNullException("task");
            }

            if (task.CanDelete)
            {
                return base.Remove(task);
            }

            return false;
        }

        public bool RemoveReminderTask([NotNull] ToDoTask task)
        {
            if (task == null)
            {
                throw new ArgumentNullException("task");
            }

            task.CanDelete = true;
            return Remove(task);
        }

        /// <summary>
        ///     Removes all items from the collection.
        /// </summary>
        protected override void ClearItems()
        {
            foreach (ToDoTask task in this.ToArray())
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
            ToDoTask task = this[index];
            if (task.CanDelete)
            {
                base.RemoveItem(index);
            }
        }
    }
}