using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Services;
using GalaSoft.MvvmLight.CommandWpf;
using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook
{
    public class ReconciliationToDoListController : ControllerBase
    {
        private readonly IApplicationDatabaseService applicationDatabaseService;
        private bool doNotUseAddingNewTask;
        private string doNotUseNewTaskDescription;
        private ToDoTask doNotUseSelectedTask;
        private ToDoCollection doNotUseTasks;

        public ReconciliationToDoListController([NotNull] IApplicationDatabaseService applicationDatabaseService)
        {
            if (applicationDatabaseService == null)
            {
                throw new ArgumentNullException(nameof(applicationDatabaseService));
            }

            this.applicationDatabaseService = applicationDatabaseService;
        }

        public bool AddingNewTask
        {
            get { return this.doNotUseAddingNewTask; }
            private set
            {
                this.doNotUseAddingNewTask = value;
                RaisePropertyChanged();
            }
        }

        public  ICommand AddReminderCommand => new RelayCommand(OnAddReminderCommandExecuted, () => !string.IsNullOrWhiteSpace(NewTaskDescription));

        public  ICommand BeginAddingReminderCommand => new RelayCommand(() => AddingNewTask = true);

        public string NewTaskDescription
        {
            get { return this.doNotUseNewTaskDescription; }
            set
            {
                this.doNotUseNewTaskDescription = value;
                RaisePropertyChanged();
            }
        }

        public  ICommand RemoveReminderCommand => new RelayCommand<ToDoTask>(OnRemoveReminderCommandExecuted, t => t != null);

        public  ICommand RemoveTaskCommand => new RelayCommand<ToDoTask>(OnRemoveTaskCommandExecuted, t => t != null);

        public ToDoTask SelectedTask
        {
            get { return this.doNotUseSelectedTask; }
            set
            {
                this.doNotUseSelectedTask = value;
                RaisePropertyChanged();
            }
        }

        public ToDoCollection Tasks
        {
            get { return this.doNotUseTasks; }
            private set
            {
                this.doNotUseTasks = value;
                RaisePropertyChanged();
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for data binding")]
        public  string Title => "Reconciliation Reminders and To Do's";

        public void Close()
        {
            Tasks = null;
        }

        public void Load(ToDoCollection tasks)
        {
            Tasks = tasks;
        }

        private void OnAddReminderCommandExecuted()
        {
            AddingNewTask = false;
            Tasks.Add(new ToDoTask(NewTaskDescription, false, false));
            this.applicationDatabaseService.NotifyOfChange(ApplicationDataType.Tasks);
        }

        private void OnRemoveReminderCommandExecuted(ToDoTask task)
        {
            Tasks.RemoveReminderTask(task);
        }

        private void OnRemoveTaskCommandExecuted(ToDoTask task)
        {
            Tasks.Remove(task);
        }
    }
}