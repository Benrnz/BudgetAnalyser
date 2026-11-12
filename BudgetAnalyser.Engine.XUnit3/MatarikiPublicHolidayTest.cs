using Shouldly;

namespace BudgetAnalyser.Engine.XUnit;

public class MatarikiPublicHolidayTest
{
    [Theory]
    [InlineData(2022, 6, 24)]
    [InlineData(2026, 7, 10)]
    [InlineData(2041, 7, 19)]
    [InlineData(2052, 6, 21)]
    public void CalculateHolidays_ShouldIncludeMatariki_GivenScheduledYear(int year, int month, int day)
    {
        var holidays = NewZealandPublicHolidays.CalculateHolidays(new DateOnly(year, 1, 1), new DateOnly(year, 12, 31));

        holidays.ShouldContain(new DateOnly(year, month, day));
    }

    [Fact]
    public void Matariki_ShouldBeAFridayInJuneOrJuly_ForEveryScheduledYear()
    {
        for (var year = 2022; year <= 2052; year++)
        {
            var matariki = MatarikiFor(year);

            matariki.ShouldNotBeNull($"Matariki missing for {year}");
            matariki.Value.DayOfWeek.ShouldBe(DayOfWeek.Friday, $"Matariki {year}");
            new[] { 6, 7 }.ShouldContain(matariki.Value.Month, $"Matariki {year}");
        }
    }

    [Theory]
    [InlineData(2021)]
    [InlineData(2053)]
    public void Matariki_ShouldNotExist_OutsideScheduledYears(int year)
    {
        MatarikiFor(year).ShouldBeNull();
    }

    [Fact]
    public void KingsBirthday_ShouldBeNamedKingsBirthday()
    {
        var names = NewZealandPublicHolidays.CalculateHolidaysVerbose(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)).Select(h => h.Item1).ToList();

        names.ShouldContain("King's Birthday");
        names.ShouldNotContain("Queen's Birthday");
    }

    private static DateOnly? MatarikiFor(int year)
    {
        var match = NewZealandPublicHolidays.CalculateHolidaysVerbose(new DateOnly(year, 1, 1), new DateOnly(year, 12, 31))
            .FirstOrDefault(h => h.Item1 == "Matariki");
        return match?.Item2;
    }
}
