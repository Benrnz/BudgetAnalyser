using BudgetAnalyser.Engine.BankFeed.Akahu;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.BankFeed.Akahu;

public class AkahuDatesTest
{
    [Theory]
    [InlineData("2026-11-11T11:00:00Z", "2026-11-12")] // Midnight 12 Nov NZDT (UTC+13)
    [InlineData("2026-11-11T10:59:59Z", "2026-11-11")] // 11:59:59pm 11 Nov NZDT
    [InlineData("2026-07-09T12:00:00Z", "2026-07-10")] // Midnight 10 Jul NZST (UTC+12)
    [InlineData("2026-07-09T11:59:59Z", "2026-07-09")] // 11:59:59pm 9 Jul NZST
    public void ToNewZealandDate_ShouldConvertFromUtc(string utcTimestamp, string expectedDate)
    {
        var result = AkahuDates.ToNewZealandDate(DateTimeOffset.Parse(utcTimestamp));

        result.ShouldBe(DateOnly.Parse(expectedDate));
    }

    [Fact]
    public void ToQueryRange_ShouldUseExclusiveStartAndInclusiveEnd_GivenSummerDates()
    {
        var (start, end) = AkahuDates.ToQueryRange(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 12));

        start.ShouldBe("2026-10-31T23:59:59.999+13:00");
        end.ShouldBe("2026-11-12T23:59:59.999+13:00");
    }

    [Fact]
    public void ToQueryRange_ShouldUseStandardTimeOffset_GivenWinterDates()
    {
        var (start, end) = AkahuDates.ToQueryRange(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 2));

        start.ShouldBe("2026-06-30T23:59:59.999+12:00");
        end.ShouldBe("2026-07-02T23:59:59.999+12:00");
    }

    [Fact]
    public void ToQueryRange_ShouldUseEachDatesOwnOffset_GivenRangeAcrossDaylightSavingStart()
    {
        // NZ daylight saving starts on Sunday 27 September 2026.
        var (start, end) = AkahuDates.ToQueryRange(new DateOnly(2026, 9, 25), new DateOnly(2026, 9, 28));

        start.ShouldBe("2026-09-24T23:59:59.999+12:00");
        end.ShouldBe("2026-09-28T23:59:59.999+13:00");
    }
}
