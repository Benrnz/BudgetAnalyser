using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Rees.Wpf.Converters
{
    /// <summary>
    ///     Use when you'd prefer hide something leaving a blank space for it rather than collapsing the space when using the
    ///     standard <see cref="BooleanToVisibilityConverter" />.
    /// </summary>
    public class NotBoolToVisibilityConverter : IValueConverter
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
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        ///     Not supported.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        ///     A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}