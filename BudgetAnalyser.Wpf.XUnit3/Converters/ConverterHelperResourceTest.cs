using BudgetAnalyser.Converters;
using BudgetAnalyser.Wpf.XUnit3.TestData;
using Shouldly;

namespace BudgetAnalyser.Wpf.XUnit3.Converters;

/// <summary>
///     <see cref="ConverterHelper" /> looks up brushes from Application.Current.Resources by string key, so nothing in XAML references them and they look unused.
///     A missing key does not crash, the brush is silently null and the converters using it render nothing. These tests guard against those resources being deleted.
/// </summary>
public class ConverterHelperResourceTest
{
    // When adding a resource key constant to ConverterHelper, add it here too.
    private static readonly string[] HelperResourceKeys =
    [
        ConverterHelper.AccumulatedBucket,
        ConverterHelper.CreditBackground1,
        ConverterHelper.DebitBackground1,
        ConverterHelper.IncomeBucket,
        ConverterHelper.NegativeText,
        ConverterHelper.NeutralNumberBackground,
        ConverterHelper.NotSoBadDebit,
        ConverterHelper.PositiveText,
        ConverterHelper.SpentMonthlyBucket,
        ConverterHelper.TileBackground,
        ConverterHelper.TileBackgroundAlternate
    ];

    public static TheoryData<string> ResourceKeys()
    {
        return new TheoryData<string>(HelperResourceKeys.Distinct());
    }

    [Theory]
    [MemberData(nameof(ResourceKeys))]
    public void ResourceUsedByHelperShouldExist(string resourceKey)
    {
        XamlResourceKeys.ApplicationResourceKeys.ShouldContain(
            resourceKey,
            $"ConverterHelper loads resource '{resourceKey}' by key, but it is not defined in any dictionary merged in App.xaml.");
    }
}
