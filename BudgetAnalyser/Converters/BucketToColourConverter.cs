using System.Globalization;
using BudgetAnalyser.Engine.Budget;
using Rees.Wpf.Converters;

namespace BudgetAnalyser.Converters;

public class BucketToColourConverter : OneWayValueConverter
{
    /// <inheritdoc />
    public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
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
}
