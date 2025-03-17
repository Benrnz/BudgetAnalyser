namespace BudgetAnalyser.Engine.UnitTest;

public class NewZealandPublicHolidaysTestHarness
{
    public NewZealandPublicHolidaysTestHarness(int year)
    {
        Year = year;
        CalculateHolidays();
    }

    public IEnumerable<DateOnly> Results { get; set; }
    public int Year { get; set; }

    public void VerifyHolidays(IEnumerable<DateOnly> expectedResults)
    {
        Console.WriteLine();
        Console.WriteLine("Expected Holidays:");
        foreach (var holiday in expectedResults)
        {
            Console.WriteLine("{0}", holiday.ToString("d-MMM-yy dddd"));
            Assert.IsTrue(Results.Contains(holiday));
        }
    }

    private void CalculateHolidays()
    {
        Results = NewZealandPublicHolidays.CalculateHolidays(new DateOnly(Year, 1, 1), new DateOnly(Year, 12, 31));
        Console.WriteLine("Calculated Holidays:");
        foreach (var holiday in NewZealandPublicHolidays.CalculateHolidaysVerbose(new DateOnly(Year, 1, 1), new DateOnly(Year, 12, 31)))
        {
            Console.WriteLine("{0} {1}", holiday.Item1.PadRight(20), holiday.Item2.ToString("d-MMM-yy dddd"));
        }
    }
}
