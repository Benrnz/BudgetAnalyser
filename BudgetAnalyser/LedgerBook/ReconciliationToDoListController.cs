using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using BudgetAnalyser.Engine.Ledger;
using GalaSoft.MvvmLight.CommandWpf;
using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook
{
    public class ReconciliationToDoListController : ControllerBase
    {
        private ToDoCollection doNotUseTasks;

        public ICommand RemoveTaskCommand
        {
            get { return new RelayCommand<ToDoTask>(OnRemoveTaskCommandExecuted, t => t != null); }
        }

        public ToDoCollection Tasks
        {
            get { return this.doNotUseTasks; }
            set
            {
                this.doNotUseTasks = value;
                RaisePropertyChanged(() => Tasks);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for data binding")]
        public string Title
        {
            get { return "Reconciliation Reminders and To Do's"; }
        }

        public void Close()
        {
            Tasks = null;
        }

        private void OnRemoveTaskCommandExecuted(ToDoTask task)
        {
            Tasks.Remove(task);
        }
    }
}