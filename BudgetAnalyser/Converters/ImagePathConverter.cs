using System;
using System.Globalization;
using System.Windows.Data;

namespace BudgetAnalyser.Converters
{
    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
            {
                return null;
            }

            if (parameter is null)
            {
                return value;
            }

            string path = value.ToString();
            if (path.StartsWith("../", StringComparison.OrdinalIgnoreCase))
            {
                return string.Format(CultureInfo.CurrentCulture, "../{0}/{1}", parameter, path.Substring(3));
            }

            return string.Format(CultureInfo.CurrentCulture, "../{0}/{1}", parameter, path);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}