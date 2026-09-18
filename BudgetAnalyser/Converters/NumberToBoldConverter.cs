using System.Globalization;
using System.Windows;
using Rees.Wpf.Converters;

namespace BudgetAnalyser.Converters;

public class NumberToBoldConverter : OneWayValueConverter
{
    /// <inheritdoc />
    public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var number = ConverterHelper.ParseNumber(value);
        return number is null ? FontWeights.Normal : (object)(number < 0 ? FontWeights.Bold : FontWeights.Normal);
    }
}
