using System;

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
    }
}