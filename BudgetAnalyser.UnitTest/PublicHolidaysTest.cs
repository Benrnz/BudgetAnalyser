using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest
{
    [TestClass]
    public class PublicHolidaysTest
    {
        private List<DateTime> dateResults;
        private DateTime start = new DateTime(2015, 01, 01);
        private DateTime end = new DateTime(2015, 12, 31);
        private List<Tuple<string, DateTime>> results;

        [TestMethod]
        public void OutputDates()
        {
            foreach (var dateTime in this.dateResults)
            {
                Console.WriteLine(dateTime.ToString("d-MMM-yy"));
            }
        }

        [TestMethod]
        public void Output()
        {
            foreach (var holiday in this.results)
            {
                Console.WriteLine("{0} {1}", holiday.Item1.PadRight(20), holiday.Item2.ToString("d-MMM-yy dddd"));
            }
        }

        [TestInitialize]
        public void Initialise()
        {
            this.dateResults = NewZealandPublicHolidays.CalculateHolidays(this.start, this.end).ToList();
            this.results = NewZealandPublicHolidays.CalcuateHolidaysVerbose(this.start, this.end).ToList();
        }
    }
}
