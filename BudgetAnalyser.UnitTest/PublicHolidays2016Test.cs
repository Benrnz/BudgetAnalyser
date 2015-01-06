using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest
{
    [TestClass]
    public class PublicHolidays2016Test
    {
        private List<DateTime> expectedHolidays = new List<DateTime>
        {
            new DateTime(2016, 1, 1),
            new DateTime(2016, 1, 4),
            new DateTime(2016, 2, 1),
            new DateTime(2016, 2, 8),
            new DateTime(2016, 3, 25),
            new DateTime(2016, 3, 28),
            new DateTime(2016, 4, 25),
            new DateTime(2016, 6, 6),
            new DateTime(2016, 10, 24),
            new DateTime(2016, 12, 26),
            new DateTime(2016, 12, 27),
        };

        private NewZealandPublicHolidaysTestHarness subject;

        [TestInitialize]
        public void Initialise()
        {
            this.subject = new NewZealandPublicHolidaysTestHarness(2016);
        }

        [TestMethod]
        public void VerifyHolidays()
        {
            this.subject.VerifyHolidays(expectedHolidays);
        }

        [TestMethod]
        public void CorrectNumberOfHolidays()
        {
            Assert.AreEqual(expectedHolidays.Count(), this.subject.Results.Count());
        }
    }
}
