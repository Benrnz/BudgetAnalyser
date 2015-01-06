using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest
{
    [TestClass]
    public class PublicHolidays2015Test
    {
        private List<DateTime> expectedHolidays = new List<DateTime>
        {
            new DateTime(2015, 1, 1),
            new DateTime(2015, 1, 2),
            new DateTime(2015, 1, 26),
            new DateTime(2015, 2, 6),
            new DateTime(2015, 4, 3),
            new DateTime(2015, 4, 6),
            new DateTime(2015, 4, 27),
            new DateTime(2015, 6, 1),
            new DateTime(2015, 10, 26),
            new DateTime(2015, 12, 25),
            new DateTime(2015, 12, 28),
        };

        private NewZealandPublicHolidaysTestHarness subject;

        [TestInitialize]
        public void Initialise()
        {
            this.subject = new NewZealandPublicHolidaysTestHarness(2015);
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
