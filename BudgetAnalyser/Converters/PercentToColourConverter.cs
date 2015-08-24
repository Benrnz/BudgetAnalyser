using System;
using System.Globalization;
using System.Windows.Data;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Converters
{
    public class PercentToColourConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || value == null)
            {
                return ConverterHelper.TransparentBrush;
            }

            try
            {
                var percent = (double)value;
                if (string.IsNullOrWhiteSpace(parameter?.ToString()) || parameter.ToString() == "Expense")
                {
                    return ConvertToExpenseColors(percent);
                }

                if (parameter.ToString() == "Income")
                {
                    return ConvertToIncomeColors(percent);
                }

                if (parameter.ToString() == "Performance")
                {
                    return ConvertToPerformanceColors(percent);
                }

                return ConverterHelper.NeutralNumberBackgroundBrush;
            }
            catch (InvalidCastException)
            {
                return ConverterHelper.TransparentBrush;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private static object ConvertToExpenseColors(double percent)
        {
            if (percent < 0.67)
            {
                return ConverterHelper.CreditBackground1Brush;
            }

            if (percent < 0.85)
            {
                return ConverterHelper.SlightDebitBrush;
            }

            if (percent < 1.1)
            {
                return ConverterHelper.NotSoBadDebitBrush;
            }

            return ConverterHelper.DebitBackground1Brush;
        }

        private static object ConvertToIncomeColors(double percent)
        {
            if (percent < 0.67)
            {
                return ConverterHelper.DebitBackground1Brush;
            }

            if (percent < 0.85)
            {
                return ConverterHelper.NotSoBadDebitBrush;
            }

            if (percent < 1.00)
            {
                return ConverterHelper.SlightDebitBrush;
            }

            return ConverterHelper.CreditBackground1Brush;
        }

        private static object ConvertToPerformanceColors(double percent)
        {
            if (percent < -100)
            {
                return ConverterHelper.DebitBackground1Brush;
            }

            if (percent < 0)
            {
                return ConverterHelper.NotSoBadDebitBrush;
            }

            if (percent < 100.00)
            {
                return ConverterHelper.NeutralNumberBackgroundBrush;
            }

            return ConverterHelper.CreditBackground1Brush;
        }
    }
}