using System.Globalization;
using System.Windows.Data;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Converters;

public class BucketToColourConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not BudgetBucket bucketValue)
        {
            return null;
        }

        if (!bucketValue.Active)
        {
            return ConverterHelper.TileBackgroundAlternateBrush;
        }

        if (value is SpentPerPeriodExpenseBucket)
        {
            return ConverterHelper.SpentPeriodicallyBucketBrush;
        }

        if (value is SavedUpForExpenseBucket)
        {
            return ConverterHelper.AccumulatedBucketBrush;
        }

        return value is IncomeBudgetBucket ? ConverterHelper.IncomeBucketBrush : (object?)ConverterHelper.NeutralNumberBackgroundBrush;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
