using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Rees.Wpf.Converters;

/// <summary>
///     This convereter is used to find and return a resouce from a resource dictionary attached to the Application.
///     Because it returns a direct reference to the resource it doesn't work very well when the same resource needs to be
///     used more than once on a view.
///     It works fine when the resources are <see cref="DrawingImage" />s, because these are used as a Source for the
///     image. For anything else errors will
///     most likely result. For these reasons it is not recommended.  Use
///     <see cref="TextToResourceControlTemplateConverter" /> instead.
/// </summary>
public class TextToStaticResourceConverter : OneWayValueConverter
{
    /// <inheritdoc />
    public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var resourceName = value as string;
        if (string.IsNullOrWhiteSpace(resourceName))
        {
            return null;
        }

        var returnValue = Application.Current.TryFindResource(resourceName);
        return returnValue;
    }
}
