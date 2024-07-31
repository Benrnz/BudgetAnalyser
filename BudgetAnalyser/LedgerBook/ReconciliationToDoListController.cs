using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Services;
using CommunityToolkit.Mvvm.Input;
using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook
{
    [AutoRegisterWithIoC]
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
            [UsedImplicitly] get { return this.doNotUseAddingNewTask; }
            private set
            {
                this.doNotUseAddingNewTask = value;
                OnPropertyChanged();
            }
        }

        [UsedImplicitly]
        public ICommand AddReminderCommand => new RelayCommand(OnAddReminderCommandExecuted, () => !string.IsNullOrWhiteSpace(NewTaskDescription));

        [UsedImplicitly]
        public ICommand BeginAddingReminderCommand => new RelayCommand(() => AddingNewTask = true);

        public string NewTaskDescription
        {
            get { return this.doNotUseNewTaskDescription; }
            [UsedImplicitly]
            set
            {
                this.doNotUseNewTaskDescription = value;
                OnPropertyChanged();
            }
        }

        [UsedImplicitly]
        public ICommand RemoveReminderCommand => new RelayCommand<ToDoTask>(OnRemoveReminderCommandExecuted, t => t != null);

        [UsedImplicitly]
        public ICommand RemoveTaskCommand => new RelayCommand<ToDoTask>(OnRemoveTaskCommandExecuted, t => t != null);

        [UsedImplicitly]
        public ToDoTask SelectedTask
        {
            get { return this.doNotUseSelectedTask; }
            set
            {
                this.doNotUseSelectedTask = value;
                OnPropertyChanged();
            }
        }

        public ToDoCollection Tasks
        {
            get { return this.doNotUseTasks; }
            private set
            {
                this.doNotUseTasks = value;
                OnPropertyChanged();
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for data binding")]
        [UsedImplicitly]
        public string Title => "Reconciliation Reminders and To Do's";

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