using System.Globalization;
using Rees.Wpf.Converters;

namespace BudgetAnalyser.Converters;

/// <summary>
///     Only used in conjunction with Matching Rules and converting a true/false boolean value into And for true and Or for false.
/// </summary>
public class BoolToAndConverter : OneWayValueConverter
{
    /// <inheritdoc />
    public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return b ? "And" : "Or";
        }

        return null;
    }
}
