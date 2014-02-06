using System;
using System.Globalization;
using System.Windows.Data;
using BudgetAnalyser.Budget;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Converters
{
    public class BudgetBucketToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bucket = value as BudgetBucket;
            if (bucket == null)
            {
                return null;
            }

            return bucket.Code;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stringValue = value as string;
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return null;
            }

            return BudgetBucketBindingSource.BucketRepository.GetByCode(stringValue);
        }
    }
}