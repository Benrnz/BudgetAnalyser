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
            var light = stringParameter is not null && stringParameter == "Light";
            var number = ConverterHelper.ParseNumber(value);
            return number is null ? ConverterHelper.TransparentBrush : (object)ConvertToBrush(number.Value, light);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private static Brush ConvertToBrush(decimal number, bool light)
        {
            if (light)
            {
                return number < 0 ? ConverterHelper.DebitBackground2Brush : ConverterHelper.CreditBackground2Brush;
            }
            return number < 0 ? ConverterHelper.DebitBackground1Brush : ConverterHelper.CreditBackground1Brush;
        }
    }
}
