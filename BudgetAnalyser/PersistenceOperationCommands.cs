using BudgetAnalyser.Engine;
using CommunityToolkit.Mvvm.Input;

namespace BudgetAnalyser;

[AutoRegisterWithIoC]
public static class PersistenceOperationCommands
{
    [PropertyInjection]
    public static PersistenceOperations? PersistenceOperations
    {
        get;
        [UsedImplicitly]
        set;
    }

    // These properties use fields rather than new'ing up instance for each call because this is a shared static class consumed by many forms.
    public static AsyncRelayCommand SaveDatabaseCommand { get; } = new(async () => await PersistenceOperations!.OnSaveDatabaseCommandExecute(), () => PersistenceOperations!.HasUnsavedChanges);
}
