using System.Security;
using Rees.Wpf;

namespace BudgetAnalyser.Dashboard;

public class PasswordSetMessage : MessageBase
{
    public required string DatabaseStorageKey { get; init; }

    /// <summary>
    ///     The password entered by the user. It is set as the credential only after any currently loaded file is closed, because closing clears the credential.
    /// </summary>
    public required SecureString Password { get; init; }
}
