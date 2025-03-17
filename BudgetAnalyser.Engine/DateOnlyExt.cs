namespace BudgetAnalyser.Engine;

public static class DateOnlyExt
{
    /// <summary>
    ///     Works out the duration in whole months between the two dates.  The end date must be at least one whole calendar month ahead of the instance date to deemed one month.
    /// </summary>
    /// <returns>An integer number of whole calendar months.</returns>
    public static int DurationInMonths(this DateOnly minDate, DateOnly maxDate)
    {
        var durationInMonths = (int)Math.Round(maxDate.Subtract(minDate) / 30.0, 0);
        if (durationInMonths <= 0)
        {
            durationInMonths = 1;
        }

        return durationInMonths;
    }

    /// <summary>
    ///     Works out the duration in whole months between the two dates.  The end date must be at least one whole calendar month ahead of the instance date to deemed one month.
    /// </summary>
    /// <param name="minDate">The Start Date, inclusive</param>
    /// <param name="maxDate">The End Date, inclusive</param>
    /// <returns>An integer number of whole calendar months.</returns>
    public static int DurationInWeeks(this DateOnly minDate, DateOnly maxDate)
    {
        var duration = maxDate.AddDays(1).Subtract(minDate) / 7;
        if (duration <= 0)
        {
            duration = 1;
        }

        return duration;
    }

    /// <summary>
    ///     Increments the day until it is not a weekend.  If the given date is already a weekday, the same date is returned.
    /// </summary>
    public static DateOnly FindNextWeekday(this DateOnly instance)
    {
        while (instance.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            instance = instance.AddDays(1);
        }

        return instance;
    }

    /// <summary>
    ///     Returns the first day of the current calendar month.
    /// </summary>
    public static DateOnly FirstDateInMonth(this DateOnly instance)
    {
        return new DateOnly(instance.Year, instance.Month, 1);
    }

    /// <summary>
    ///     Returns the last day of the given month.
    /// </summary>
    public static DateOnly LastDateInMonth(this DateOnly instance)
    {
        return instance.AddMonths(1).FirstDateInMonth().AddDays(-1);
    }

    public static int Subtract(this DateOnly date1, DateOnly date2)
    {
        return date1.DayNumber - date2.DayNumber;
    }

    public static DateOnly Today()
    {
        var now = DateTime.Now;
        return new DateOnly(now.Year, now.Month, now.Day);
    }
}
