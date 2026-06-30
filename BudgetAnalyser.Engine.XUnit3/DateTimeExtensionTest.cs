using Shouldly;

// ReSharper disable InconsistentNaming
// Method naming matches date rules, ie 1st not 1St as recommended.

namespace BudgetAnalyser.Engine.XUnit;

public class DateTimeExtensionTest
{
    [Fact]
    public void FebruaryToMarchIsOneMonth()
    {
        new DateTime(2014, 2, 1).DurationInMonths(new DateTime(2014, 3, 1)).ShouldBe(1);
    }

    [Fact]
    public void FindNextWeekDayShouldReturn1stJuneGiven30thMay()
    {
        new DateTime(2015, 6, 1).ShouldBe(new DateTime(2015, 5, 30).FindNextWeekday());
    }

    [Fact]
    public void FindNextWeekDayShouldReturn1stJuneGiven31stMay()
    {
        new DateTime(2015, 6, 1).ShouldBe(new DateTime(2015, 5, 31).FindNextWeekday());
    }

    [Fact]
    public void FindNextWeekDayShouldReturn29thMayGiven29thMay()
    {
        new DateTime(2015, 5, 29).ShouldBe(new DateTime(2015, 5, 29).FindNextWeekday());
    }

    [Fact]
    public void Given1DecemberFirstDateInMonthShouldReturn1December()
    {
        new DateTime(2014, 12, 1).ShouldBe(new DateTime(2014, 12, 1).FirstDateInMonth());
    }

    [Fact]
    public void Given28FebruaryLastDateInMonthShouldReturn28February()
    {
        new DateTime(2014, 2, 28).ShouldBe(new DateTime(2014, 2, 28).LastDateInMonth());
    }

    [Fact]
    public void Given28JanuaryFirstDateInMonthShouldReturn1January()
    {
        new DateTime(2014, 1, 1).ShouldBe(new DateTime(2014, 1, 28).FirstDateInMonth());
    }

    [Fact]
    public void Given30JanuaryLastDateInMonthShouldReturn31January()
    {
        new DateTime(2014, 1, 31).ShouldBe(new DateTime(2014, 1, 30).LastDateInMonth());
    }

    [Fact]
    public void Given5February2000LastDateInMonthShouldReturn29February()
    {
        new DateTime(2000, 2, 29).ShouldBe(new DateTime(2000, 2, 5).LastDateInMonth());
    }

    [Fact]
    public void Given5JuneLastDateInMonthShouldReturn30June()
    {
        new DateTime(2014, 6, 30).ShouldBe(new DateTime(2014, 6, 5).LastDateInMonth());
    }

    [Fact]
    public void January2014To17thMarch2014IsRoundedDownToTenWeeks()
    {
        new DateTime(2014, 1, 1).DurationInWeeks(new DateTime(2014, 3, 17)).ShouldBe(10);
    }

    [Fact]
    public void January2014To17thMarch2014IsRoundedDownToTwoMonths()
    {
        new DateTime(2014, 1, 1).DurationInMonths(new DateTime(2014, 3, 17)).ShouldBe(2);
    }

    [Fact]
    public void January2014To23rdMarch2014IsRoundedUpToElevenWeeks()
    {
        new DateTime(2014, 1, 1).DurationInWeeks(new DateTime(2014, 3, 23)).ShouldBe(11);
    }

    [Fact]
    public void January2014To23rdMarch2014IsRoundedUpToThreeMonths()
    {
        new DateTime(2014, 1, 1).DurationInMonths(new DateTime(2014, 3, 23)).ShouldBe(3);
    }

    [Fact]
    public void JanuaryToJanuaryIsOneMonth()
    {
        new DateTime(2014, 1, 1).DurationInMonths(new DateTime(2014, 1, 1)).ShouldBe(1);
    }

    [Fact]
    public void JanuaryToJanuaryIsOneWeek()
    {
        new DateTime(2014, 1, 1).DurationInWeeks(new DateTime(2014, 1, 1)).ShouldBe(1);
    }

    [Fact]
    public void JanuaryToJuneIs21Weeks()
    {
        new DateTime(2014, 1, 1).DurationInWeeks(new DateTime(2014, 6, 1)).ShouldBe(21);
    }

    [Fact]
    public void JanuaryToJuneIsFiveMonths()
    {
        new DateTime(2014, 1, 1).DurationInMonths(new DateTime(2014, 6, 1)).ShouldBe(5);
    }

    [Fact]
    public void October2013ToMarch2014Is21Weeks()
    {
        new DateTime(2013, 10, 1).DurationInWeeks(new DateTime(2014, 3, 1)).ShouldBe(21);
    }

    [Fact]
    public void October2013ToMarch2014IsFiveMonths()
    {
        new DateTime(2013, 10, 1).DurationInMonths(new DateTime(2014, 3, 1)).ShouldBe(5);
    }

    [Fact]
    public void OneStandardPayFortnightIsTwoWeeks1()
    {
        new DateTime(2024, 12, 5).DurationInWeeks(new DateTime(2024, 12, 18)).ShouldBe(2);
    }

    [Fact]
    public void OneStandardPayFortnightIsTwoWeeks2()
    {
        new DateTime(2024, 12, 19).DurationInWeeks(new DateTime(2025, 1, 1)).ShouldBe(2);
    }

    [Fact]
    public void OneStandardPayMonthIsOneMonth1()
    {
        new DateTime(2014, 1, 20).DurationInMonths(new DateTime(2014, 2, 19)).ShouldBe(1);
    }

    [Fact]
    public void OneStandardPayMonthIsOneMonth2()
    {
        new DateTime(2014, 1, 15).DurationInMonths(new DateTime(2014, 2, 14)).ShouldBe(1);
    }
}
