

// Generated code do not directly modify
// 6/12/2015 12:42:10 PM
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace BudgetAnalyser.Engine.UnitTest
{

    [TestClass]
    public class PublicHolidays2014Test
    {
        private const int Year = 2014;
        private NewZealandPublicHolidaysTestHarness subject;
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

        [TestMethod]
        public void CorrectNumberOfHolidays()
        {
            Assert.AreEqual(this.expectedHolidays.Count(), this.subject.Results.Count());
        }

        [TestInitialize]
        public void Initialise()
        {
            this.subject = new NewZealandPublicHolidaysTestHarness(2014);
        }

        [TestMethod]
        public void VerifyHolidays()
        {
            this.subject.VerifyHolidays(this.expectedHolidays);
        }

    } // End Test class for 2014
    [TestClass]
    public class PublicHolidays2015Test
    {
        private const int Year = 2015;
        private NewZealandPublicHolidaysTestHarness subject;
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

        [TestMethod]
        public void CorrectNumberOfHolidays()
        {
            Assert.AreEqual(this.expectedHolidays.Count(), this.subject.Results.Count());
        }

        [TestInitialize]
        public void Initialise()
        {
            this.subject = new NewZealandPublicHolidaysTestHarness(2015);
        }

        [TestMethod]
        public void VerifyHolidays()
        {
            this.subject.VerifyHolidays(this.expectedHolidays);
        }

    } // End Test class for 2015
    [TestClass]
    public class PublicHolidays2016Test
    {
        private const int Year = 2016;
        private NewZealandPublicHolidaysTestHarness subject;
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

        [TestMethod]
        public void CorrectNumberOfHolidays()
        {
            Assert.AreEqual(this.expectedHolidays.Count(), this.subject.Results.Count());
        }

        [TestInitialize]
        public void Initialise()
        {
            this.subject = new NewZealandPublicHolidaysTestHarness(2016);
        }

        [TestMethod]
        public void VerifyHolidays()
        {
            this.subject.VerifyHolidays(this.expectedHolidays);
        }

    } // End Test class for 2016
    [TestClass]
    public class PublicHolidays2017Test
    {
        private const int Year = 2017;
        private NewZealandPublicHolidaysTestHarness subject;
        private List<DateTime> expectedHolidays = new List<DateTime>
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

                new DateTime(2017, 12, 26),
            };

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

    } // End Test class for 2017
    [TestClass]
    public class PublicHolidays2018Test
    {
        private const int Year = 2018;
        private NewZealandPublicHolidaysTestHarness subject;
        private List<DateTime> expectedHolidays = new List<DateTime>
            {

                new DateTime(2018, 1, 1),

                new DateTime(2018, 1, 2),

                new DateTime(2018, 1, 29),

                new DateTime(2018, 2, 6),

                new DateTime(2018, 3, 30),

                new DateTime(2018, 4, 2),

                new DateTime(2018, 4, 25),

                new DateTime(2018, 6, 4),

                new DateTime(2018, 10, 22),

                new DateTime(2018, 12, 25),

                new DateTime(2018, 12, 26),
            };

        [TestMethod]
        public void CorrectNumberOfHolidays()
        {
            Assert.AreEqual(this.expectedHolidays.Count(), this.subject.Results.Count());
        }

        [TestInitialize]
        public void Initialise()
        {
            this.subject = new NewZealandPublicHolidaysTestHarness(2018);
        }

        [TestMethod]
        public void VerifyHolidays()
        {
            this.subject.VerifyHolidays(this.expectedHolidays);
        }

    } // End Test class for 2018
} // End namespace
