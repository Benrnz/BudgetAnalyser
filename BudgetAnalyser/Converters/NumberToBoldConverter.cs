using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BudgetAnalyser.Converters;

public class NumberToBoldConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var number = ConverterHelper.ParseNumber(value);
        return number is null ? FontWeights.Normal : (object)(number < 0 ? FontWeights.Bold : FontWeights.Normal);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
