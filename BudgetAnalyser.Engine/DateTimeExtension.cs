using System;

namespace BudgetAnalyser.Engine
{
    /// <summary>
    ///     An extension for DateTime.
    /// </summary>
    public static class DateTimeExtension
    {
        /// <summary>
        ///     Works out the duration in whole months between the two dates.  The end date must be at least one whole calendar
        ///     month
        ///     ahead of the instance date to deemed one month.
        /// </summary>
        /// <returns>An integer number of whole calendar months.</returns>
        public static int DurationInMonths(this DateTime minDate, DateTime maxDate)
        {
            var durationInMonths = (int) Math.Round(maxDate.Subtract(minDate).TotalDays/30, 0);
            if (durationInMonths <= 0)
            {
                durationInMonths = 1;
            }

            return durationInMonths;
        }

        /// <summary>
        ///     Increments the day until it is not a weekend.  If the given date is already a weekday, the same date is returned.
        /// </summary>
        public static DateTime FindNextWeekday(this DateTime instance)
        {
            while (instance.DayOfWeek == DayOfWeek.Saturday || instance.DayOfWeek == DayOfWeek.Sunday)
            {
                instance = instance.AddDays(1);
            }

            return instance;
        }

        /// <summary>
        ///     Returns the first day of the current calendar month.
        /// </summary>
        public static DateTime FirstDateInMonth(this DateTime instance)
        {
            return new DateTime(instance.Year, instance.Month, 1);
        }

        /// <summary>
        ///     Returns the last day of the given month.
        /// </summary>
        public static DateTime LastDateInMonth(this DateTime instance)
        {
            return instance.AddMonths(1).FirstDateInMonth().AddDays(-1);
        }
    }
}