using System;
using System.Globalization;
using System.Windows.Data;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Converters
{
    public class BudgetNameViewModelConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
            {
                return null;
            }

            var budget = values[0] as BudgetModel;
            var budgets = values[1] as BudgetCollection;

            if (budget == null || budgets == null)
            {
                return null;
            }

            return new BudgetCurrencyContext(budgets, budget);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
