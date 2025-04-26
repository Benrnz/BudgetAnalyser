using System.Globalization;
using System.Windows;
using System.Windows.Data;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Converters;

public class BucketToVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not BudgetBucket bucket)
        {
            return Visibility.Visible;
        }

        if (parameter is not null)
        {
            if (parameter.ToString() == "Expense")
            {
                return bucket is SurplusBucket ? Visibility.Visible : (object)(bucket is ExpenseBucket ? Visibility.Visible : Visibility.Collapsed);
            }

            if (parameter.ToString() == "Income")
            {
                return bucket is IncomeBudgetBucket ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        return Visibility.Visible;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
