using System;
using System.Globalization;
using System.Windows.Data;

namespace BudgetAnalyser.Converters
{
    public class NumberSignToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal number;
            try
            {
                number = (decimal)value;
            }
            catch (InvalidCastException)
            {
                try
                {
                    var doublenumber = (double)value;
                    number = System.Convert.ToDecimal(doublenumber);
                }
                catch (InvalidCastException)
                {
                    return ConverterHelper.TransparentBrush;
                }
            }

            return ConvertToBrush(number);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private static object ConvertToBrush(decimal number)
        {
            if (number < 0)
            {
                return ConverterHelper.DebitBackground1Brush;
            }
            return ConverterHelper.CreditBackground1Brush;
        }
    }
}