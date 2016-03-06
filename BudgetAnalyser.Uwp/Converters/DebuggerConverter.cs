using System;
using System.Diagnostics;
using Windows.UI.Xaml.Data;

namespace BudgetAnalyser.Uwp.Converters
{
    public class DebuggerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Debugger.Break();
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            Debugger.Break();
            return value;
        }
    }
}
