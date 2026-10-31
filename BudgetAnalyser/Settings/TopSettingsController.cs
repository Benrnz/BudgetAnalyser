using BudgetAnalyser.ApplicationState;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Transactions;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.Settings;

/// <summary>
///     Presents the user preference settings stored in the application state file (BudgetAnalyserAppState.json). Settings are grouped in the view to match their grouping in
///     the file. Window size and position are deliberately not shown, they are captured automatically when the application exits.
/// </summary>
[AutoRegisterWithIoC(SingleInstance = true)]
public partial class TopSettingsController : ControllerBase, IShowableController
{
    private readonly TopTransactionsListController transactionsController;

    public TopSettingsController(IMessenger messenger, TopTransactionsListController transactionsController) : base(messenger)
    {
        this.transactionsController = transactionsController ?? throw new ArgumentNullException(nameof(transactionsController));

        Messenger.Register<TopSettingsController, ApplicationStateLoadedMessage>(this, static (r, m) => r.OnApplicationStateLoaded(m));
    }

    /// <summary>
    ///     The Budget Analyser file that was automatically opened at start-up. This is read-only, it is updated to the currently open file when the application exits.
    /// </summary>
    [ObservableProperty]
    public partial string LastBaxFile { get; private set; } = string.Empty;

    /// <summary>
    ///     The number of rows shown per page in paginated lists, such as the Transactions list.  Persisted by the <see cref="ShellController" /> when the application exits.
    /// </summary>
    public int ListPageSize
    {
        get => this.transactionsController.PageSize;
        set
        {
            this.transactionsController.PageSize = value;
            OnPropertyChanged();
        }
    }

    [ObservableProperty]
    public partial bool Shown { get; set; }

    private void OnApplicationStateLoaded(ApplicationStateLoadedMessage message)
    {
        var engineState = message.ElementOfType<ApplicationEngineState>();
        LastBaxFile = engineState?.BudgetAnalyserDataStorageKey ?? string.Empty;
        OnPropertyChanged(nameof(ListPageSize));
    }

    partial void OnShownChanged(bool value)
    {
        if (value)
        {
            // Values owned by other controllers may have changed since the view last bound to them.
            OnPropertyChanged(nameof(ListPageSize));
        }
    }
}
