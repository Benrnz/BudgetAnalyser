using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BudgetAnalyser.Converters
{
    public class NumberSignToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stringParameter = parameter as string;
            bool light = stringParameter is not null && stringParameter == "Light";
            decimal? number = ConverterHelper.ParseNumber(value);
            if (number is null)
            {
                return ConverterHelper.TransparentBrush;
            }

            return ConvertToBrush(number.Value, light);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private static Brush ConvertToBrush(decimal number, bool light)
        {
            if (light)
            {
                if (number < 0)
                {
                    return ConverterHelper.DebitBackground2Brush;
                }
                return ConverterHelper.CreditBackground2Brush;
            }
            if (number < 0)
            {
                return ConverterHelper.DebitBackground1Brush;
            }
            return ConverterHelper.CreditBackground1Brush;
        }
    }
}