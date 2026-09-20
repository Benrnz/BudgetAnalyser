using System.Diagnostics.CodeAnalysis;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Ledger;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook;

[AutoRegisterWithIoC]
public partial class ReconciliationToDoListController : ControllerBase
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

    [ObservableProperty]
    public partial bool AddingNewTask
    {
        [UsedImplicitly]
        get;
        private set;
    }

    public IRelayCommand AddReminderCommand { get; }

    public IRelayCommand BeginAddingReminderCommand { get; }

    [ObservableProperty]
    public partial string NewTaskDescription
    {
        get;
        [UsedImplicitly]
        set;
    } = string.Empty;

    public IRelayCommand<ToDoTask?> RemoveReminderCommand { get; }

    public IRelayCommand<ToDoTask?> RemoveTaskCommand { get; }

    [ObservableProperty]
    public partial ToDoTask? SelectedTask { get; set; }

    [ObservableProperty]
    public partial ToDoCollection? Tasks { get; private set; }

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

    partial void OnNewTaskDescriptionChanged(string value)
    {
        AddReminderCommand.NotifyCanExecuteChanged();
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
