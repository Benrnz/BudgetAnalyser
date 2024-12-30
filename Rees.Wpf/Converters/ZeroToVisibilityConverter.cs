using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Rees.Wpf.Converters
{
    /// <summary>
    ///     A Converter that will convert any number type (decimal, double, int etc) of zero into
    ///     <see cref="Visibility.Collapsed" /> and
    ///     non-zero into <see cref="Visibility.Visible" />.
    /// </summary>
    public class ZeroToVisibilityConverter : IValueConverter
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
            if (value is null)
            {
                return Visibility.Hidden;
            }

            if (value is decimal && (decimal)value == 0)
            {
                return Visibility.Hidden;
            }

            if (value is double && (double)value == 0)
            {
                return Visibility.Hidden;
            }

            if (value is int && (int)value == 0)
            {
                return Visibility.Hidden;
            }

            return value is long && (long)value == 0 ? Visibility.Hidden : (object)Visibility.Visible;
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
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
