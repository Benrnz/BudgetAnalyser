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

            return bucket?.Code;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stringValue = value as string;
            return string.IsNullOrWhiteSpace(stringValue) ? null : (object)BudgetBucketBindingSource.BucketRepository.GetByCode(stringValue);
        }
    }
}
