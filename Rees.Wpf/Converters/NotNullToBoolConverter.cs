using System.Globalization;
using System.Windows.Data;

namespace Rees.Wpf.Converters
{
    /// <summary>
    ///     If a value is Null will return false, if it is something it will return true.
    ///     Useful to bind IsEnabled properties to the presence of a value a command might be dependent on.
    /// </summary>
    public class NotNullToBoolConverter : IValueConverter
    {
        /// <summary>
        ///     Converts a <see cref="Boolean"/>value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">Not Used.</param>
        /// <param name="parameter">Not Used.</param>
        /// <param name="culture">Not Used.</param>
        /// <returns>
        ///     Will always return true, or false. True if the value is something, false if it is null.
        /// </returns>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is not null;
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
}