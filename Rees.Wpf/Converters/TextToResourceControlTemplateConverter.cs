using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Rees.Wpf.Converters;

/// <summary>
///     Use this converter to take a given string and locate a <see cref="ControlTemplate" /> in the Application resources
///     that matches the given text name.
///     This is used to display vector (XAML) images instead of used PNG images.
/// </summary>
public class TextToResourceControlTemplateConverter : OneWayValueConverter
{
    /// <inheritdoc />
    public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var resourceName = value as string;
        return string.IsNullOrWhiteSpace(resourceName) ? null : (object)(ControlTemplate)Application.Current.TryFindResource(resourceName);
    }
}
