using System.IO;
using System.Xml.Linq;
using BudgetAnalyser.LedgerBook;
using Shouldly;

namespace BudgetAnalyser.Wpf.XUnit3.LedgerBook;

/// <summary>
///     <see cref="LedgerBookGridBuilderV2" /> looks up styles, brushes and templates by string key at runtime, so nothing in XAML references them and they look unused.
///     These tests guard against those resources being deleted, which crashes the app with a ResourceReferenceKeyNotFoundException when a ledger book is shown.
///     The builder searches the TopLedgerBookUserControl resources first, then the application resources merged in App.xaml, so these tests search the same XAML source files.
/// </summary>
public class LedgerBookGridBuilderV2ResourceTest
{
    private static readonly XNamespace XamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
    private static readonly XNamespace PresentationNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

    private static readonly Lazy<HashSet<string>> AvailableKeys = new(LoadAvailableKeys);

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

    private static IEnumerable<XElement> KeyedResources(XElement resourcesContainer)
    {
        var dictionary = resourcesContainer.Element(PresentationNamespace + "ResourceDictionary") ?? resourcesContainer;
        return dictionary.Elements().Where(e => e.Attribute(XamlNamespace + "Key") is not null);
    }

    private static void AddKeysFromDictionaryFile(string fileName, HashSet<string> keys)
    {
        var dictionary = XDocument.Load(fileName).Root!;
        keys.UnionWith(KeyedResources(dictionary).Select(e => e.Attribute(XamlNamespace + "Key")!.Value));
        AddKeysFromMergedDictionaries(dictionary, Path.GetDirectoryName(fileName)!, keys);
    }

    private static void AddKeysFromMergedDictionaries(XElement dictionary, string baseFolder, HashSet<string> keys)
    {
        var mergedSources = dictionary
            .Descendants(PresentationNamespace + "ResourceDictionary.MergedDictionaries")
            .Elements(PresentationNamespace + "ResourceDictionary")
            .Select(e => e.Attribute("Source")?.Value)
            .Where(source => source is not null && !source.StartsWith("pack:", StringComparison.OrdinalIgnoreCase)); // Third party dictionaries, eg Material Design.

        foreach (var source in mergedSources)
        {
            AddKeysFromDictionaryFile(Path.Combine(baseFolder, source!), keys);
        }
    }

    private static string FindWpfProjectFolder()
    {
        var folder = new DirectoryInfo(AppContext.BaseDirectory);
        while (folder is not null)
        {
            var candidate = Path.Combine(folder.FullName, "BudgetAnalyser");
            if (File.Exists(Path.Combine(candidate, "App.xaml")))
            {
                return candidate;
            }

            folder = folder.Parent;
        }

        throw new DirectoryNotFoundException("Unable to locate the BudgetAnalyser WPF project folder (containing App.xaml) from " + AppContext.BaseDirectory);
    }

    private static HashSet<string> LoadAvailableKeys()
    {
        var projectFolder = FindWpfProjectFolder();
        var keys = new HashSet<string>();

        // Local view resources passed to BuildGrid.
        var userControl = XDocument.Load(Path.Combine(projectFolder, "LedgerBook", "TopLedgerBookUserControl.xaml")).Root!;
        keys.UnionWith(KeyedResources(userControl.Element(PresentationNamespace + "UserControl.Resources")!).Select(e => e.Attribute(XamlNamespace + "Key")!.Value));

        // Application resources.
        var appResources = XDocument.Load(Path.Combine(projectFolder, "App.xaml")).Root!.Element(PresentationNamespace + "Application.Resources")!;
        AddKeysFromMergedDictionaries(appResources, projectFolder, keys);

        return keys;
    }
}
