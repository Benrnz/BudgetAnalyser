using BudgetAnalyser.Engine.Transactions;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Transactions;

public class TransactionsCalculationsSuggestedImportDateRangeTest
{
    [Theory]
    // A week with no public holidays: the end is yesterday, except weekends and Mondays roll back to Friday.
    [InlineData("2026-11-02", "2026-10-30")] // Monday -> previous Friday
    [InlineData("2026-11-03", "2026-11-02")] // Tuesday -> Monday
    [InlineData("2026-11-04", "2026-11-03")] // Wednesday -> Tuesday
    [InlineData("2026-11-05", "2026-11-04")] // Thursday -> Wednesday
    [InlineData("2026-11-06", "2026-11-05")] // Friday -> Thursday
    [InlineData("2026-11-07", "2026-11-06")] // Saturday -> Friday
    [InlineData("2026-11-08", "2026-11-06")] // Sunday -> Friday
    public void End_ShouldBePreviousBusinessDay_GivenWeekWithoutHolidays(string today, string expectedEnd)
    {
        var result = TransactionsCalculations.CalculateSuggestedImportDateRange([], DateOnly.Parse(today));

        result.End.ShouldBe(DateOnly.Parse(expectedEnd));
    }

    [Theory]
    [InlineData("2027-03-29", "2027-03-25")] // Easter Monday; Good Friday 26 Mar is also a holiday
    [InlineData("2027-03-30", "2027-03-25")] // Tuesday after Easter
    [InlineData("2026-12-29", "2026-12-24")] // Christmas Fri 25 Dec, Boxing Day Mondayised to Mon 28 Dec
    [InlineData("2027-01-05", "2026-12-31")] // New Year Fri 1 Jan, 2 Jan Mondayised to Mon 4 Jan
    [InlineData("2026-07-13", "2026-07-09")] // Matariki Fri 10 Jul 2026
    [InlineData("2026-01-27", "2026-01-23")] // Auckland Anniversary Mon 26 Jan 2026
    public void End_ShouldSkipPublicHolidays(string today, string expectedEnd)
    {
        var result = TransactionsCalculations.CalculateSuggestedImportDateRange([], DateOnly.Parse(today));

        result.End.ShouldBe(DateOnly.Parse(expectedEnd));
    }

    [Fact]
    public void Start_ShouldBeDayAfterLatestTransaction_GivenUnorderedTransactions()
    {
        Transaction[] transactions =
        [
            new() { Date = new DateOnly(2026, 10, 20) },
            new() { Date = new DateOnly(2026, 11, 1) },
            new() { Date = new DateOnly(2026, 10, 28) }
        ];

        var result = TransactionsCalculations.CalculateSuggestedImportDateRange(transactions, new DateOnly(2026, 11, 4));

        result.Start.ShouldBe(new DateOnly(2026, 11, 2));
        result.End.ShouldBe(new DateOnly(2026, 11, 3));
        result.IsEmpty.ShouldBeFalse();
    }

    [Fact]
    public void Start_ShouldBeNull_GivenNoTransactions()
    {
        var result = TransactionsCalculations.CalculateSuggestedImportDateRange([], new DateOnly(2026, 11, 4));

        result.Start.ShouldBeNull();
        result.IsEmpty.ShouldBeFalse();
    }

    [Fact]
    public void IsEmpty_ShouldBeTrue_GivenTransactionsAreAlreadyUpToDate()
    {
        Transaction[] transactions = [new() { Date = new DateOnly(2026, 11, 3) }];

        var result = TransactionsCalculations.CalculateSuggestedImportDateRange(transactions, new DateOnly(2026, 11, 4));

        result.Start.ShouldBe(new DateOnly(2026, 11, 4));
        result.End.ShouldBe(new DateOnly(2026, 11, 3));
        result.IsEmpty.ShouldBeTrue();
    }

    [Fact]
    public void ShouldThrow_GivenNullTransactions()
    {
        Should.Throw<ArgumentNullException>(() => TransactionsCalculations.CalculateSuggestedImportDateRange(null!, new DateOnly(2026, 11, 4)));
    }
}
