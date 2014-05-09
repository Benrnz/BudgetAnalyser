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
            if (value is SpentMonthlyExpenseBucket)
            {
                return ConverterHelper.DebitBackground1Brush;
            }

            if (value is SavedUpForExpenseBucket)
            {
                return ConverterHelper.DebitBackground2Brush;
            }

            if (value is IncomeBudgetBucket)
            {
                return ConverterHelper.CreditBackground1Brush;
            }

            return ConverterHelper.NeutralNumberBackgroundBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}