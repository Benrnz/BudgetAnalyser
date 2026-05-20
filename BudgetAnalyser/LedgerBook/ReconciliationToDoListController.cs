using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Ledger;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook;

[AutoRegisterWithIoC]
public class ReconciliationToDoListController(IMessenger messenger, IApplicationDatabaseFacade applicationDatabaseService) : ControllerBase(messenger)
{
    private readonly IApplicationDatabaseFacade applicationDatabaseService = applicationDatabaseService ?? throw new ArgumentNullException(nameof(applicationDatabaseService));

    public bool AddingNewTask
    {
        [UsedImplicitly]
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public ICommand AddReminderCommand => new RelayCommand(OnAddReminderCommandExecuted, () => !string.IsNullOrWhiteSpace(NewTaskDescription));

    public ICommand BeginAddingReminderCommand => new RelayCommand(() => AddingNewTask = true);

    public string NewTaskDescription
    {
        get;
        [UsedImplicitly]
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public ICommand RemoveReminderCommand => new RelayCommand<ToDoTask>(OnRemoveReminderCommandExecuted, t => t is not null);

    public ICommand RemoveTaskCommand => new RelayCommand<ToDoTask>(OnRemoveTaskCommandExecuted, t => t is not null);

    public ToDoTask? SelectedTask
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public ToDoCollection? Tasks
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for data binding")]
    //public string Title => "Reconciliation Reminders and To Do's";
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
        Tasks!.Add(new ToDoTask { Description = NewTaskDescription, CanDelete = false, SystemGenerated = false });
        this.applicationDatabaseService.NotifyOfChange(ApplicationDataType.Tasks);
    }

    private void OnRemoveReminderCommandExecuted(ToDoTask? task)
    {
        if (task is null)
        {
            return;
        }

        Tasks!.RemoveReminderTask(task);
    }

    private void OnRemoveTaskCommandExecuted(ToDoTask? task)
    {
        if (task is null)
        {
            return;
        }

        Tasks!.Remove(task);
    }
}
