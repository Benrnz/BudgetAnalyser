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
            if (!(value is BudgetBucket)) return null;

            if (!((BudgetBucket)value).Active)
            {
                return ConverterHelper.TileBackgroundAlternateBrush;
            }

            if (value is SpentMonthlyExpenseBucket)
            {
                return ConverterHelper.SpentMonthlyBucketBrush;
            }

            if (value is SavedUpForExpenseBucket)
            {
                return ConverterHelper.AccumulatedBucketBrush;
            }

            if (value is SavingsCommitmentBucket)
            {
                return ConverterHelper.NotSoBadDebitBrush;
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