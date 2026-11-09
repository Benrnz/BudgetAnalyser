using System.Globalization;

namespace BudgetAnalyser.Engine.BankFeed.Akahu;

/// <summary>
///     Converts between Budget Analyser's New Zealand local dates and the UTC timestamps used by the Akahu API.
/// </summary>
internal static class AkahuDates
{
    private static readonly TimeZoneInfo NewZealandTimeZone = FindNewZealandTimeZone();

    /// <summary>
    ///     Converts an Akahu UTC timestamp to the New Zealand local date. Akahu dates are often only accurate to the day, and midnight in New Zealand is the previous day in UTC,
    ///     so the date part must never be taken from the UTC value directly.
    /// </summary>
    public static DateOnly ToNewZealandDate(DateTimeOffset timestamp)
    {
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(timestamp, NewZealandTimeZone).DateTime);
    }

    /// <summary>
    ///     Builds the start and end query values for an inclusive range of New Zealand local dates. Akahu treats start as exclusive and end as inclusive, with millisecond
    ///     resolution, so start is the last millisecond of the day before <paramref name="firstDate" /> and end is the last millisecond of <paramref name="lastDate" />.
    /// </summary>
    public static (string Start, string End) ToQueryRange(DateOnly firstDate, DateOnly lastDate)
    {
        return (EndOfDay(firstDate.AddDays(-1)), EndOfDay(lastDate));
    }

    private static string EndOfDay(DateOnly date)
    {
        var local = date.ToDateTime(new TimeOnly(23, 59, 59, 999));
        var timestamp = new DateTimeOffset(local, NewZealandTimeZone.GetUtcOffset(local));
        return timestamp.ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz", CultureInfo.InvariantCulture);
    }

    private static TimeZoneInfo FindNewZealandTimeZone()
    {
        // The Windows and IANA identifiers are both tried, so this works on any platform.
        foreach (var id in new[] { "New Zealand Standard Time", "Pacific/Auckland" })
        {
            if (TimeZoneInfo.TryFindSystemTimeZoneById(id, out var zone))
            {
                return zone;
            }
        }

        throw new TimeZoneNotFoundException("The New Zealand time zone could not be found on this computer.");
    }
}
