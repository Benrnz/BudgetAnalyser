using System;
using System.ComponentModel;

namespace BudgetAnalyser.Engine
{
    public static class DateTimeExtension
    {
        public static int DurationInMonths(this DateTime minDate, DateTime maxDate)
        {
            var durationInMonths = (int)Math.Round(maxDate.Subtract(minDate).TotalDays / 30, 0);
            if (durationInMonths <= 0)
            {
                durationInMonths = 1;
            }

            return durationInMonths;
        }

        public static DateTime LastDateInMonth(this DateTime instance)
        {
            return instance.AddMonths(1).FirstDateInMonth().AddDays(-1);
        }

        public static DateTime FirstDateInMonth(this DateTime instance)
        {
            return instance.AddDays(-instance.Day + 1);
        }
    }
}