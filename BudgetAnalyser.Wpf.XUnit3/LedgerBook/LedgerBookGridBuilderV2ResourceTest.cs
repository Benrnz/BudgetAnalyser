using BudgetAnalyser.LedgerBook;
using BudgetAnalyser.Wpf.XUnit3.TestData;
using Shouldly;

namespace BudgetAnalyser.Wpf.XUnit3.LedgerBook;

/// <summary>
///     <see cref="LedgerBookGridBuilderV2" /> looks up styles, brushes and templates by string key at runtime, so nothing in XAML references them and they look unused.
///     These tests guard against those resources being deleted, which crashes the app with a ResourceReferenceKeyNotFoundException when a ledger book is shown.
///     The builder searches the TopLedgerBookUserControl resources first, then the application resources merged in App.xaml, so these tests search the same XAML source files.
/// </summary>
public class LedgerBookGridBuilderV2ResourceTest
{
    // The view resources passed to BuildGrid, then the application resources.
    private static readonly Lazy<HashSet<string>> AvailableKeys = new(() =>
        [.. XamlResourceKeys.UserControlResourceKeys(@"LedgerBook\TopLedgerBookUserControl.xaml"), .. XamlResourceKeys.ApplicationResourceKeys]);

    // When adding a resource key constant to LedgerBookGridBuilderV2, add it here too.
    private static readonly string[] BuilderResourceKeys =
    [
        LedgerBookGridBuilderV2.BankBalanceBackground,
        LedgerBookGridBuilderV2.BankBalanceTextBrush,
        LedgerBookGridBuilderV2.CreditTextBrush,
        LedgerBookGridBuilderV2.DateColumnStyle,
        LedgerBookGridBuilderV2.DebitTextBrush,
        LedgerBookGridBuilderV2.HeadingStyle,
        LedgerBookGridBuilderV2.ImportantNumberStyle,
        LedgerBookGridBuilderV2.LessButtonStyle,
        LedgerBookGridBuilderV2.LightBorderBrush,
        LedgerBookGridBuilderV2.MainBackground,
        LedgerBookGridBuilderV2.MoreButtonStyle,
        LedgerBookGridBuilderV2.NormalHighlightBackground,
        LedgerBookGridBuilderV2.NormalStyle,
        LedgerBookGridBuilderV2.NumberStyle,
        LedgerBookGridBuilderV2.RemarksStyle,
        LedgerBookGridBuilderV2.SurplusBackground,
        LedgerBookGridBuilderV2.SurplusTextBrush,
        LedgerBookGridBuilderV2.WarningImageTemplate,
        LedgerBookGridBuilderV2.ZeroTextBrush
    ];

    public static TheoryData<string> ResourceKeys()
    {
        // Several constants share the same key, only test each key once.
        return new TheoryData<string>(BuilderResourceKeys.Distinct());
    }

    [Theory]
    [MemberData(nameof(ResourceKeys))]
    public void ResourceUsedByBuilderShouldExist(string resourceKey)
    {
        AvailableKeys.Value.ShouldContain(
            resourceKey,
            $"LedgerBookGridBuilderV2 loads resource '{resourceKey}' by key, but it is not defined in TopLedgerBookUserControl.xaml or any dictionary merged in App.xaml.");
    }
}
