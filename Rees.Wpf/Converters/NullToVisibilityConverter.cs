using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Rees.Wpf.Converters;

/// <summary>
///     Returns <see cref="Visibility.Hidden" /> when the value is null or an empty string. Otherwise,
///     <see cref="Visibility.Collapsed" />.
///     This is the opporsite of <see cref="NotNullToVisibilityConverter" />.
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    /// <summary>
    ///     Converts a value.
    /// </summary>
    /// <param name="value">The value produced by the binding source.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>
    ///     A converted value. If the method returns null, the valid null value is used.
    /// </returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var stringParameter = parameter as string;
        var hiddenValue = Visibility.Hidden;
        var test = () => value is null;

        if (stringParameter is not null)
        {
            if (value is string && (stringParameter == string.Empty || stringParameter == "Empty"))
            {
                test = () => string.IsNullOrWhiteSpace(value.ToString());
            }

            if (stringParameter == "Collapsed")
            {
                hiddenValue = Visibility.Collapsed;
            }
        }

        return test() ? hiddenValue : Visibility.Visible;
    }

    /// <summary>
    ///     Not Supported.
    /// </summary>
    /// <param name="value">The value that is produced by the binding target.</param>
    /// <param name="targetType">The type to convert to.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>
    ///     A converted value. If the method returns null, the valid null value is used.
    /// </returns>
    /// <exception cref="System.NotSupportedException"></exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
