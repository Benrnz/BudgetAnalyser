namespace BudgetAnalyser.Engine.BankFeed.Akahu;

/// <summary>
///     The two tokens an Akahu personal app needs for every request. Both are secret.
/// </summary>
/// <param name="AppIdToken">Sent in the X-Akahu-Id header.</param>
/// <param name="UserAccessToken">Sent as the Bearer token in the Authorization header.</param>
internal sealed record AkahuCredentials(string AppIdToken, string UserAccessToken)
{
    /// <summary>
    ///     Overridden so the tokens can never appear in logs, exception messages or the debugger display.
    /// </summary>
    public override string ToString()
    {
        return $"{nameof(AkahuCredentials)} {{ tokens hidden }}";
    }
}
