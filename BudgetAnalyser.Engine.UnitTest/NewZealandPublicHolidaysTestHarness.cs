using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest
{
    public class NewZealandPublicHolidaysTestHarness
    {
        public NewZealandPublicHolidaysTestHarness(int year)
        {
            Year = year;
            CalculateHolidays();
        }

        public IEnumerable<DateTime> Results { get; set; }
        public int Year { get; set; }

        public void VerifyHolidays(IEnumerable<DateTime> expectedResults)
        {
            Console.WriteLine();
            Console.WriteLine("Expected Holidays:");
            foreach (DateTime holiday in expectedResults)
            {
                Console.WriteLine("{0}", holiday.ToString("d-MMM-yy dddd"));
                Assert.IsTrue(Results.Contains(holiday));
            }
        }

        private void CalculateHolidays()
        {
            Results = NewZealandPublicHolidays.CalculateHolidays(new DateTime(Year, 1, 1), new DateTime(Year, 12, 31));
            Console.WriteLine("Calculated Holidays:");
            foreach (Tuple<string, DateTime> holiday in NewZealandPublicHolidays.CalculateHolidaysVerbose(new DateTime(Year, 1, 1), new DateTime(Year, 12, 31)))
            {
                Console.WriteLine("{0} {1}", holiday.Item1.PadRight(20), holiday.Item2.ToString("d-MMM-yy dddd"));
            }
        }
    }
}