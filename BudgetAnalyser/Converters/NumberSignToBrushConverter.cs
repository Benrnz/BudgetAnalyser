using System.Globalization;
using System.Windows.Media;
using Rees.Wpf.Converters;

namespace BudgetAnalyser.Converters;

public class NumberSignToBrushConverter : OneWayValueConverter
{
    /// <inheritdoc />
    public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var stringParameter = parameter as string;
        var light = stringParameter is not null && stringParameter == "Light";
        var number = ConverterHelper.ParseNumber(value);
        return number is null ? ConverterHelper.TransparentBrush : (object?)ConvertToBrush(number.Value, light);
    }

    private static Brush? ConvertToBrush(decimal number, bool light)
    {
        if (light)
        {
            return number < 0 ? ConverterHelper.NegativeTextBrush : ConverterHelper.PositiveTextBrush;
        }

        return number < 0 ? ConverterHelper.DebitBackground1Brush : ConverterHelper.CreditBackground1Brush;
    }
}
