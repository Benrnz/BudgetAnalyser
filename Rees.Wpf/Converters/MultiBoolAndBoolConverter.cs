using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Rees.Wpf.Converters;

/// <summary>
///     A multi-converter that takes any number of Boolean values, and will return <see cref="Visibility.Visible" /> if all
///     are true. Otherwise, <see cref="Visibility.Collapsed" />.
/// </summary>
public class MultiBoolAndBoolConverter : IMultiValueConverter
{
    /// <summary>
    ///     Expected to receive an array of <see cref="bool" /> values. If any are false, will return false, it all are true, will return true.
    /// </summary>
    /// <param name="values">
    ///     The array of bools that the source bindings in the
    ///     <see cref="T:System.Windows.Data.MultiBinding" /> produces. The value
    ///     <see cref="F:System.Windows.DependencyProperty.UnsetValue" /> indicates that the source binding has no value to
    ///     provide for conversion.
    /// </param>
    /// <param name="targetType">Not Used.</param>
    /// <param name="parameter">Not Used.</param>
    /// <param name="culture">Not Used.</param>
    /// <returns>true if all bool values are true, otherwise false. If anything goes wrong, will return false.</returns>
    public object Convert(object[]? values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null)
        {
            return false;
        }

        try
        {
            return values.Cast<bool>().All(v => v);
        }
        catch (InvalidCastException)
        {
            return false;
        }
    }

    /// <summary>
    ///     Not Supported.
    /// </summary>
    /// <param name="value">The value that the binding target produces.</param>
    /// <param name="targetTypes">
    ///     The array of types to convert to. The array length indicates the number and types of values
    ///     that are suggested for the method to return.
    /// </param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>
    ///     An array of values that have been converted from the target value back to the source values.
    /// </returns>
    /// <exception cref="System.NotSupportedException"></exception>
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}