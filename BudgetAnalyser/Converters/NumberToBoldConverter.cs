using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BudgetAnalyser.Converters
{
    public class NumberToBoldConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal? number = ConverterHelper.ParseNumber(value);
            if (number == null)
            {
                return FontWeights.Normal;
            }

            return number < 0 ? FontWeights.Bold : FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
