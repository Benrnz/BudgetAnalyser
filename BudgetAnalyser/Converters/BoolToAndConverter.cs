using System;
using System.Globalization;
using System.Windows.Data;

namespace BudgetAnalyser.Converters
{
    /// <summary>
    /// Only used in conjunction with Matching Rules and converting a true/false boolean value into And for true and Or for false.
    /// </summary>
    public class BoolToAndConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return (bool)value ? "And" : "Or";
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}