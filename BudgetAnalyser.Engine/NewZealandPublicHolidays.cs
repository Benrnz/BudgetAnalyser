using System.Globalization;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine;

/// <summary>
///     A calculator that will list New Zealand holidays.
/// </summary>
[AutoRegisterWithIoC]
public static class NewZealandPublicHolidays
{
    private static readonly List<Holiday> HolidayTemplates =
    [
        new FixedDateHoliday { Name = "New Years Day", Day = 1, Month = 1, MondayiseIfOnWeekend = true },
        new FixedDateHoliday { Name = "New Years Holiday", Day = 2, Month = 1, MondayiseIfOnWeekend = true },
        new FixedDateHoliday { Name = "Waitangi Day", Day = 6, Month = 2, MondayiseIfOnWeekend = true },
        new EasterHoliday { Name = "Good Friday", Day = DayOfWeek.Friday },
        new EasterHoliday { Name = "Easter Monday", Day = DayOfWeek.Monday },
        new FixedDateHoliday { Name = "ANZAC Day", Day = 25, Month = 4, MondayiseIfOnWeekend = true },
        new FixedDateHoliday { Name = "Christmas Day", Day = 25, Month = 12, MondayiseIfOnWeekend = true },
        new FixedDateHoliday { Name = "Boxing Day", Day = 26, Month = 12, MondayiseIfOnWeekend = true },
        new IndexDayHoliday { Name = "Queen's Birthday", Day = DayOfWeek.Monday, Month = 6, Index = 0 },
        new IndexDayHoliday { Name = "Labor Day", Day = DayOfWeek.Monday, Month = 10, Index = 3 },
        new DayClosestMondayToHoliday { Name = "Auckland Anniversary", Month = 1, CloseToDate = 29 }
    ];

    [PropertyInjection]
    public static ILogger? Logger
    {
        get;
        [UsedImplicitly]
        set;
    }

    /// <summary>
    ///     Calculate and list New Zealand holidays between two dates.
    /// </summary>
    public static IEnumerable<DateOnly> CalculateHolidays(DateOnly start, DateOnly end)
    {
        return CalculateHolidaysVerbose(start, end).Select(t => t.Item2);
    }

    /// <summary>
    ///     Calculate and list New Zealand holidays between two dates. The return collection contains labeled holidays and their dates.
    /// </summary>
    public static IEnumerable<Tuple<string, DateOnly>> CalculateHolidaysVerbose(DateOnly start, DateOnly end)
    {
        var holidays = new Dictionary<DateOnly, string>();
        foreach (var holidayTemplate in HolidayTemplates)
        {
            try
            {
                var proposedDate = holidayTemplate.CalculateDate(start, end);
                if (holidays.ContainsKey(proposedDate))
                {
                    holidays.Add(proposedDate.AddDays(1), holidayTemplate.Name);
                }
                else
                {
                    holidays.Add(proposedDate, holidayTemplate.Name);
                }
            }
            catch (InvalidOperationException ex)
            {
                Logger?.LogError(ex, _ => "Can occur if a holiday template implementation fails to report back its exact holiday date. (Could be fixed)");
            }
        }

        return holidays.Select(h => new Tuple<string, DateOnly>(h.Value, h.Key)).OrderBy(d => d.Item2);
    }

    /// <summary>
    ///     A holiday that is celebrated closest to an anniversary date.
    ///     For example: Auckland anniversary is celebrated on the closest Monday to the 29th of January.
    /// </summary>
    private class DayClosestMondayToHoliday : Holiday
    {
        public int CloseToDate { get; init; }
        public int Month { get; init; }

        public override DateOnly CalculateDate(DateOnly start, DateOnly end)
        {
            for (var year = start.Year; year <= end.Year; year++)
            {
                var proposed = new DateOnly(year, Month, CloseToDate);
                switch (proposed.DayOfWeek)
                {
                    case DayOfWeek.Sunday:
                        proposed = proposed.AddDays(1);
                        break;
                    case DayOfWeek.Monday:
                        break;
                    case DayOfWeek.Tuesday:
                        proposed = proposed.AddDays(-1);
                        break;
                    case DayOfWeek.Wednesday:
                        proposed = proposed.AddDays(-2);
                        break;
                    case DayOfWeek.Thursday:
                        proposed = proposed.AddDays(-3);
                        break;
                    case DayOfWeek.Friday:
                        proposed = proposed.AddDays(3);
                        break;
                    case DayOfWeek.Saturday:
                        proposed = proposed.AddDays(2);
                        break;
                }

                if (proposed >= start && proposed <= end)
                {
                    return proposed;
                }
            }

            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                "Cannot find a suitable date between {0} and {1}", start, end));
        }
    }

    /// <summary>
    ///     A custom calculator specifically to find the dates for Easter for any year.
    /// </summary>
    private class EasterHoliday : Holiday
    {
        public DayOfWeek Day { get; init; }

        public override DateOnly CalculateDate(DateOnly start, DateOnly end)
        {
            for (var year = start.Year; year <= end.Year; year++)
            {
                // first calculate Easter Sunday

                var goldenNumber = year % 19;
                var century = year / 100;
                var h = (century - (century / 4) - (((8 * century) + 13) / 25) + (19 * goldenNumber) + 15) % 30;
                var i = h - (h / 28 * (1 - (h / 28 * (29 / (h + 1)) * ((21 - goldenNumber) / 11))));

                var day = i - ((year + (year / 4) + i + 2 - century + (century / 4)) % 7) + 28;
                var month = 3;

                if (day > 31)
                {
                    month++;
                    day -= 31;
                }

                var proposed = new DateOnly(year, month, day);

                switch (Day)
                {
                    case DayOfWeek.Friday:
                        proposed = proposed.AddDays(-2);
                        break;
                    case DayOfWeek.Sunday:
                        break;
                    case DayOfWeek.Monday:
                        proposed = proposed.AddDays(1);
                        break;
                    case DayOfWeek.Tuesday:
                        proposed = proposed.AddDays(2);
                        break;
                    default:
                        throw new NotSupportedException("Only Easter Friday, Monday, and Tuesday are supported.");
                }

                if (proposed >= start && proposed <= end)
                {
                    return proposed;
                }
            }

            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                "Cannot find a suitable date between {0} and {1}", start, end));
        }
    }

    /// <summary>
    ///     A holiday that occurs every year on a specific date.  For example Christmas day, the 25th of December.
    ///     These holidays can still be optionally "Monday-ised" using the <see cref="MondayiseIfOnWeekend" /> property.
    /// </summary>
    private class FixedDateHoliday : Holiday
    {
        public int Day { get; init; }
        public bool MondayiseIfOnWeekend { get; init; }
        public int Month { get; init; }

        public override DateOnly CalculateDate(DateOnly start, DateOnly end)
        {
            var proposed = DateOnly.MinValue;
            for (var year = start.Year; year <= end.Year; year++)
            {
                proposed = new DateOnly(year, Month, Day);
                if (proposed >= start && proposed <= end)
                {
                    break;
                }
            }

            if (MondayiseIfOnWeekend &&
                (proposed.DayOfWeek == DayOfWeek.Saturday || proposed.DayOfWeek == DayOfWeek.Sunday))
            {
                do
                {
                    proposed = proposed.AddDays(1);
                } while (proposed.DayOfWeek != DayOfWeek.Monday);
            }

            return proposed < DateOnly.MinValue.AddMonths(1)
                ? throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                    "Cannot find a suitable date between {0} and {1}", start, end))
                : proposed;
        }
    }

    private abstract class Holiday
    {
        public required string Name { get; init; }
        public abstract DateOnly CalculateDate(DateOnly start, DateOnly end);
    }

    /// <summary>
    ///     A holiday that occurs on a certain day of the week and at a certain indexed week in the month. For example: Kings's birthday is celebrated on the first Monday of June.
    /// </summary>
    private class IndexDayHoliday : Holiday
    {
        public DayOfWeek Day { get; init; }
        public int Index { get; init; }
        public int Month { get; init; }

        public override DateOnly CalculateDate(DateOnly start, DateOnly end)
        {
            for (var year = start.Year; year <= end.Year; year++)
            {
                var proposed = ProposeDate(year);
                if (proposed >= start && proposed <= end)
                {
                    return proposed;
                }
            }

            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                "Cannot find a suitable date between {0} and {1}", start, end));
        }

        private DateOnly ProposeDate(int year)
        {
            var proposed = new DateOnly(year, Month, 1);
            while (proposed.DayOfWeek != Day)
            {
                proposed = proposed.AddDays(1);
            }

            var count = 0;
            while (count != Index)
            {
                proposed = proposed.AddDays(7);
                count++;
            }

            return proposed;
        }
    }
}
