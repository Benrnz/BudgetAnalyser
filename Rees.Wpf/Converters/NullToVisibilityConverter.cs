using System.Globalization;
using System.Windows;

namespace Rees.Wpf.Converters;

/// <summary>
///     Returns <see cref="Visibility.Hidden" /> (or <see cref="Visibility.Collapsed" /> if the converter parameter is
///     "Collapsed") when the value is null. Otherwise, <see cref="Visibility.Visible" />.
///     Set <see cref="Invert" /> to true to get the opposite: <see cref="Visibility.Visible" /> when the value is null,
///     the hidden value otherwise.
///     If the converter parameter is "" or "Empty" and the value is a string, an empty or whitespace-only string is
///     also treated as null.
/// </summary>
public class NullToVisibilityConverter : OneWayValueConverter
{
    /// <summary>
    ///     When true, inverts the result: visible when the value is null, hidden otherwise.
    /// </summary>
    public bool Invert { get; set; }

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
    public override object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var stringParameter = parameter as string;
        var hiddenValue = Visibility.Hidden;
        var isNullIsh = string.IsNullOrWhiteSpace(value?.ToString());

        if (stringParameter is not null)
        {
            if (value is string && (stringParameter == string.Empty || stringParameter == "Empty"))
            {
                isNullIsh = string.IsNullOrWhiteSpace(value.ToString());
            }

            if (stringParameter == "Collapsed")
            {
                hiddenValue = Visibility.Collapsed;
            }
        }

        if (Invert)
        {
            return isNullIsh ? Visibility.Visible : hiddenValue;
        }

        return isNullIsh ? hiddenValue : Visibility.Visible;
    }
}
