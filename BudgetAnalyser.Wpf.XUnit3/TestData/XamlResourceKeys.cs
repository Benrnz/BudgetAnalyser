using System.IO;
using System.Xml.Linq;

namespace BudgetAnalyser.Wpf.XUnit3.TestData;

/// <summary>
///     Reads resource keys (x:Key) from the WPF project's XAML source files. Used to verify resources that code looks up by string key at runtime still exist,
///     because nothing in XAML references them and they are easily mistaken as unused and deleted.
/// </summary>
public static class XamlResourceKeys
{
    private static readonly XNamespace XamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
    private static readonly XNamespace PresentationNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

    private static readonly Lazy<string> WpfProjectFolder = new(FindWpfProjectFolder);
    private static readonly Lazy<IReadOnlySet<string>> AppKeys = new(LoadApplicationResourceKeys);

    /// <summary>
    ///     The keys available from Application.Current.Resources: those in App.xaml and every dictionary it merges, excluding third party (pack:) dictionaries such as Material Design.
    /// </summary>
    public static IReadOnlySet<string> ApplicationResourceKeys => AppKeys.Value;

    /// <summary>
    ///     The keys defined in a UserControl's own UserControl.Resources.
    /// </summary>
    /// <param name="relativePath">Path of the XAML file relative to the WPF project folder, eg LedgerBook\TopLedgerBookUserControl.xaml.</param>
    public static IReadOnlySet<string> UserControlResourceKeys(string relativePath)
    {
        var userControl = XDocument.Load(Path.Combine(WpfProjectFolder.Value, relativePath)).Root!;
        var resources = userControl.Element(PresentationNamespace + "UserControl.Resources");
        var keys = new HashSet<string>();
        if (resources is not null)
        {
            AddKeys(resources, WpfProjectFolder.Value, keys);
        }

        return keys;
    }

    private static void AddKeys(XElement resourcesContainer, string baseFolder, HashSet<string> keys)
    {
        var dictionary = resourcesContainer.Element(PresentationNamespace + "ResourceDictionary") ?? resourcesContainer;
        keys.UnionWith(dictionary.Elements().Select(e => e.Attribute(XamlNamespace + "Key")?.Value).OfType<string>());

        var mergedSources = dictionary
            .Elements(PresentationNamespace + "ResourceDictionary.MergedDictionaries")
            .Elements(PresentationNamespace + "ResourceDictionary")
            .Select(e => e.Attribute("Source")?.Value)
            .OfType<string>()
            .Where(source => !source.StartsWith("pack:", StringComparison.OrdinalIgnoreCase));

        foreach (var source in mergedSources)
        {
            var fileName = Path.Combine(baseFolder, source);
            AddKeys(XDocument.Load(fileName).Root!, Path.GetDirectoryName(fileName)!, keys);
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

    private static IReadOnlySet<string> LoadApplicationResourceKeys()
    {
        var appResources = XDocument.Load(Path.Combine(WpfProjectFolder.Value, "App.xaml")).Root!.Element(PresentationNamespace + "Application.Resources")!;
        var keys = new HashSet<string>();
        AddKeys(appResources, WpfProjectFolder.Value, keys);
        return keys;
    }
}
