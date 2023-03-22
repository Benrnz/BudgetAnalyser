using System;
using System.Globalization;
using System.Windows.Data;

namespace Rees.Wpf.Converters
{
    /// <summary>
    /// A Converter that will convert any type (decimal, double, int etc) of zero into Null and leave non-zero intact.
    /// Useful when you want numbers to be blank when they are zero.
    /// </summary>
    public class ZeroToBlankConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            if (value is decimal && (decimal) value == 0)
            {
                return null;
            }

            if (value is double && (double) value == 0)
            {
                return null;
            }

            if (value is int && (int) value == 0)
            {
                return null;
            }

            if (value is long && (long) value == 0)
            {
                return null;
            }

            return value;
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}