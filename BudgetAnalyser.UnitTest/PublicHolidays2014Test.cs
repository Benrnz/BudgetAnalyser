using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest
{
    [TestClass]
    public class PublicHolidays2014Test
    {
        private List<DateTime> expectedHolidays = new List<DateTime>
        {
            new DateTime(2014, 1, 1),
            new DateTime(2014, 1, 2),
            new DateTime(2014, 1, 27),
            new DateTime(2014, 2, 6),
            new DateTime(2014, 4, 18),
            new DateTime(2014, 4, 21),
            new DateTime(2014, 4, 25),
            new DateTime(2014, 6, 2),
            new DateTime(2014, 10, 27),
            new DateTime(2014, 12, 25),
            new DateTime(2014, 12, 26),
        };

        private NewZealandPublicHolidaysTestHarness subject;

        [TestInitialize]
        public void Initialise()
        {
            this.subject = new NewZealandPublicHolidaysTestHarness(2014);
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
