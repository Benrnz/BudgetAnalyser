using Rees.Wpf;

namespace BudgetAnalyser.Dashboard;

public class PasswordSetMessage : MessageBase
{
    public required string DatabaseStorageKey { get; init; }
}
