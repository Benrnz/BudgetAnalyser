using System;
using System.Globalization;
using System.Windows.Data;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Converters
{
    public class BucketToColourConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bucketValue = value as BudgetBucket;
            if (bucketValue is null)
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

            if (value is IncomeBudgetBucket)
            {
                return ConverterHelper.IncomeBucketBrush;
            }

            return ConverterHelper.NeutralNumberBackgroundBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}