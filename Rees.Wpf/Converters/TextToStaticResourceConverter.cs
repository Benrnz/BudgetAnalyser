using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Rees.Wpf.Converters
{
    /// <summary>
    ///     This convereter is used to find and return a resouce from a resource dictionary attached to the Application.
    ///     Because it returns a direct reference to the resource it doesn't work very well when the same resource needs to be
    ///     used more than once on a view.
    ///     It works fine when the resources are <see cref="DrawingImage" />s, because these are used as a Source for the
    ///     image. For anything else errors will
    ///     most likely result. For these reasons it is not recommended.  Use
    ///     <see cref="TextToResourceControlTemplateConverter" /> instead.
    /// </summary>
    public class TextToStaticResourceConverter : IValueConverter
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
            var resourceName = value as string;
            if (string.IsNullOrWhiteSpace(resourceName))
            {
                return null;
            }

            var returnValue = Application.Current.TryFindResource(resourceName);
            return returnValue;
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