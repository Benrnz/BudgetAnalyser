using System.Globalization;
using System.Windows.Data;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Converters;

public class BudgetNameViewModelConverter : IMultiValueConverter
{
    public object? Convert(object?[]? values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is null || values.Length < 2)
        {
            return null;
        }

        return values[0] is not BudgetModel budget || values[1] is not BudgetCollection budgets ? null : (object)new BudgetCurrencyContext(budgets, budget);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
