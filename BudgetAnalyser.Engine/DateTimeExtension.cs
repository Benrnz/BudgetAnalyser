using System;

namespace BudgetAnalyser.Engine
{
    public static class DateTimeExtension
    {
        public static DateTime ToEndOfMonth(this DateTime instance)
        {
            int lastDay = 1;
            DateTime next = instance;
            int month;
            do
            {
                lastDay = next.Day;
                month = instance.Month;
                next = next.AddDays(1);
            } while (month == next.Month);

            return new DateTime(instance.Year, instance.Month, lastDay);
        }

        public static DateTime ToBeginingOfMonth(this DateTime instance)
        {
            return instance.AddDays(-instance.Day + 1);
        }
    }
}
