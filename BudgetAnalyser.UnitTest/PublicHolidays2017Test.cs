using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest
{
    [TestClass]
    public class PublicHolidays2017Test
    {
        private readonly List<DateTime> expectedHolidays = new List<DateTime>
        {
            new DateTime(2017, 1, 2),
            new DateTime(2017, 1, 3),
            new DateTime(2017, 1, 30),
            new DateTime(2017, 2, 6),
            new DateTime(2017, 4, 14),
            new DateTime(2017, 4, 17),
            new DateTime(2017, 4, 25),
            new DateTime(2017, 6, 5),
            new DateTime(2017, 10, 23),
            new DateTime(2017, 12, 25),
            new DateTime(2017, 12, 26)
        };

        private NewZealandPublicHolidaysTestHarness subject;

        [TestMethod]
        public void CorrectNumberOfHolidays()
        {
            Assert.AreEqual(this.expectedHolidays.Count(), this.subject.Results.Count());
        }

        [TestInitialize]
        public void Initialise()
        {
            this.subject = new NewZealandPublicHolidaysTestHarness(2017);
        }

        [TestMethod]
        public void VerifyHolidays()
        {
            this.subject.VerifyHolidays(this.expectedHolidays);
        }
    }
}