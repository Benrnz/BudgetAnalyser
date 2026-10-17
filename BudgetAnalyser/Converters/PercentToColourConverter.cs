using System.Globalization;
using Rees.Wpf.Converters;

namespace BudgetAnalyser.Converters;

public class PercentToColourConverter : OneWayValueConverter
{
    /// <inheritdoc />
    public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is null || value is null)
        {
            return ConverterHelper.TransparentBrush;
        }

        try
        {
            var percent = (double)value;
            if (string.IsNullOrWhiteSpace(parameter.ToString()) || parameter.ToString() == "Expense")
            {
                return ConvertToExpenseColors(percent);
            }

            if (parameter.ToString() == "Income")
            {
                return ConvertToIncomeColors(percent);
            }

            return parameter.ToString() == "Performance" ? ConvertToPerformanceColors(percent) : ConverterHelper.NeutralNumberBackgroundBrush;
        }
        catch (InvalidCastException)
        {
            return ConverterHelper.TransparentBrush;
        }
    }

    private static object? ConvertToExpenseColors(double percent)
    {
        if (percent < 0.67)
        {
            return ConverterHelper.CreditBackground1Brush;
        }

        if (percent < 0.85)
        {
            return ConverterHelper.NeutralNumberBackgroundBrush;
        }

        return percent < 1.15 ? ConverterHelper.NotSoBadDebitBrush : (object?)ConverterHelper.DebitBackground1Brush;
    }

    private static object? ConvertToIncomeColors(double percent)
    {
        if (percent < 0.67)
        {
            return ConverterHelper.DebitBackground1Brush;
        }

        if (percent < 0.85)
        {
            return ConverterHelper.NotSoBadDebitBrush;
        }

        return percent < 1.00 ? ConverterHelper.NeutralNumberBackgroundBrush : (object?)ConverterHelper.CreditBackground1Brush;
    }

    private static object? ConvertToPerformanceColors(double percent)
    {
        if (percent < -100)
        {
            return ConverterHelper.DebitBackground1Brush;
        }

        if (percent < 0)
        {
            return ConverterHelper.NotSoBadDebitBrush;
        }

        return percent < 100.00 ? ConverterHelper.NeutralNumberBackgroundBrush : (object?)ConverterHelper.CreditBackground1Brush;
    }
}
