using System.Diagnostics.CodeAnalysis;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Ledger;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook;

[AutoRegisterWithIoC]
public class ReconciliationToDoListController : ControllerBase
{
    private readonly IApplicationDatabaseFacade applicationDatabaseService;

    public ReconciliationToDoListController(IMessenger messenger, IApplicationDatabaseFacade applicationDatabaseService)
        : base(messenger)
    {
        this.applicationDatabaseService = applicationDatabaseService ?? throw new ArgumentNullException(nameof(applicationDatabaseService));
        AddReminderCommand = new RelayCommand(OnAddReminderCommandExecuted, () => !string.IsNullOrWhiteSpace(NewTaskDescription));
        BeginAddingReminderCommand = new RelayCommand(() => AddingNewTask = true);
        RemoveReminderCommand = new RelayCommand<ToDoTask?>(OnRemoveReminderCommandExecuted, t => t is not null);
        RemoveTaskCommand = new RelayCommand<ToDoTask?>(OnRemoveTaskCommandExecuted, t => t is not null);
    }

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

    public IRelayCommand AddReminderCommand { get; }

    public IRelayCommand BeginAddingReminderCommand { get; }

    public string NewTaskDescription
    {
        get;
        [UsedImplicitly]
        set
        {
            field = value;
            OnPropertyChanged();
            AddReminderCommand.NotifyCanExecuteChanged();
        }
    } = string.Empty;

    public IRelayCommand<ToDoTask?> RemoveReminderCommand { get; }

    public IRelayCommand<ToDoTask?> RemoveTaskCommand { get; }

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
