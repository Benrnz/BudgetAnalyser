using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Rees.Wpf.Converters;

/// <summary>
///     Use when you'd prefer hide something leaving a blank space for it rather than collapsing the space when using the
///     standard <see cref="BooleanToVisibilityConverter" />.
/// </summary>
public class BoolToVisibility2Converter : OneWayValueConverter
{
    /// <inheritdoc />
    public override object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return Visibility.Hidden;
        }

        return (bool)value ? Visibility.Visible : Visibility.Hidden;
    }
}
