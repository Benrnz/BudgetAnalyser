using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Matching;

namespace BudgetAnalyser.Converters
{
    public class MatchingRuleDescriptorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var rule = value as MatchingRule;
            if (rule == null)
            {
                return string.Empty;
            }

            int truncateTo;
            if (parameter is bool)
            {
                if ((bool) parameter)
                {
                    truncateTo = 15;
                }
                else
                {
                    truncateTo = int.MaxValue;
                }
            }
            else if (parameter is string)
            {
                truncateTo = parameter.ToString() == "true" ? 15 : int.MaxValue;
            }
            else
            {
                truncateTo = int.MaxValue;
            }

            var builder = new StringBuilder();
            bool isEmpty = true;
            builder.Append(rule.Reference1.Truncate(truncateTo, ref isEmpty, true, string.Empty));
            builder.Append(rule.Reference2.Truncate(truncateTo, ref isEmpty, true, " / "));
            builder.Append(rule.Reference3.Truncate(truncateTo, ref isEmpty, true, " / "));
            builder.Append(rule.TransactionType.Truncate(truncateTo, ref isEmpty, true, " / "));

            return builder.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}