namespace BudgetAnalyser.Engine.BankFeed;

/// <summary>
///     The starting field mapping for a newly mapped account, chosen by the institution name. The user can change it afterwards on the Settings tab.
///     No institution presets have been decided yet, so every institution currently gets <see cref="BankFeedFieldMapping.Default" />.
/// </summary>
public static class BankFeedFieldMappingPresets
{
    private static readonly Dictionary<string, BankFeedFieldMapping> Presets = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Gets the preset for the given institution, or <see cref="BankFeedFieldMapping.Default" /> if there is none.
    /// </summary>
    public static BankFeedFieldMapping ForInstitution(string? institution)
    {
        return institution is not null && Presets.TryGetValue(institution, out var preset) ? preset : BankFeedFieldMapping.Default;
    }
}
