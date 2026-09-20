using System.Globalization;
using System.Windows.Data;

namespace Rees.Wpf.Converters;

/// <summary>
///     Base class for <see cref="IValueConverter" /> implementations that only convert one way. Binding a
///     two-way property (e.g. a <see cref="System.Windows.Controls.TextBox.Text" />) through one of these
///     converters will throw <see cref="NotSupportedException" /> from <see cref="ConvertBack" />.
/// </summary>
public abstract class OneWayValueConverter : IValueConverter
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
    public abstract object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture);

    /// <summary>
    ///     Not supported — this converter is one-way.
    /// </summary>
    /// <exception cref="System.NotSupportedException"></exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
