using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Converters
{
    public class BucketToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bucket = value as BudgetBucket;
            if (bucket is null)
            {
                return Visibility.Visible;
            }

            if (parameter is not null)
            {
                if (parameter.ToString() == "Expense")
                {
                    if (bucket is SurplusBucket)
                    {
                        return Visibility.Visible;
                    }

                    return bucket is ExpenseBucket ? Visibility.Visible : Visibility.Collapsed;
                }

                if (parameter.ToString() == "Income")
                {
                    return bucket is IncomeBudgetBucket ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}