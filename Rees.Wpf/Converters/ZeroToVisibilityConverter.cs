using System.Globalization;
using System.Windows;

namespace Rees.Wpf.Converters;

/// <summary>
///     A Converter that will convert any number type (decimal, double, int etc) of zero into
///     <see cref="Visibility.Collapsed" /> and
///     non-zero into <see cref="Visibility.Visible" />.
/// </summary>
public class ZeroToVisibilityConverter : OneWayValueConverter
{
    /// <inheritdoc />
    public override object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is null or 0 or 0d or 0m or 0L ? Visibility.Hidden : Visibility.Visible;
    }
}
