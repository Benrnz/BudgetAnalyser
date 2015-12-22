using System;
using BudgetAnalyser.Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest
{
    [TestClass]
    public class DateTimeExtensionTest
    {
        [TestMethod]
        public void FebruaryToMarchIsOneMonth()
        {
            Assert.AreEqual(1, new DateTime(2014, 2, 1).DurationInMonths(new DateTime(2014, 3, 1)));
        }

        [TestMethod]
        public void FindNextWeekDayShouldReturn1stJuneGiven30thMay()
        {
            Assert.AreEqual(new DateTime(2015, 6, 1), new DateTime(2015, 5, 30).FindNextWeekday());
        }

        [TestMethod]
        public void FindNextWeekDayShouldReturn1stJuneGiven31stMay()
        {
            Assert.AreEqual(new DateTime(2015, 6, 1), new DateTime(2015, 5, 31).FindNextWeekday());
        }

        [TestMethod]
        public void FindNextWeekDayShouldReturn29thMayGiven29thMay()
        {
            Assert.AreEqual(new DateTime(2015, 5, 29), new DateTime(2015, 5, 29).FindNextWeekday());
        }

        [TestMethod]
        public void Given1DecemberFirstDateInMonthShouldReturn1December()
        {
            Assert.AreEqual(new DateTime(2014, 12, 1), new DateTime(2014, 12, 1).FirstDateInMonth());
        }

        [TestMethod]
        public void Given28FebruaryLastDateInMonthShouldReturn28February()
        {
            Assert.AreEqual(new DateTime(2014, 2, 28), new DateTime(2014, 2, 28).LastDateInMonth());
        }

        [TestMethod]
        public void Given28JanuaryFirstDateInMonthShouldReturn1January()
        {
            Assert.AreEqual(new DateTime(2014, 1, 1), new DateTime(2014, 1, 28).FirstDateInMonth());
        }

        [TestMethod]
        public void Given30JanuaryLastDateInMonthShouldReturn31January()
        {
            Assert.AreEqual(new DateTime(2014, 1, 31), new DateTime(2014, 1, 30).LastDateInMonth());
        }

        [TestMethod]
        public void Given5February2000LastDateInMonthShouldReturn29February()
        {
            Assert.AreEqual(new DateTime(2000, 2, 29), new DateTime(2000, 2, 5).LastDateInMonth());
        }

        [TestMethod]
        public void Given5JuneLastDateInMonthShouldReturn30June()
        {
            Assert.AreEqual(new DateTime(2014, 6, 30), new DateTime(2014, 6, 5).LastDateInMonth());
        }

        [TestMethod]
        public void January2014To17thMarch2014IsRoundedDownToTwoMonths()
        {
            Assert.AreEqual(2, new DateTime(2014, 1, 1).DurationInMonths(new DateTime(2014, 3, 17)));
        }

        [TestMethod]
        public void January2014To23rdMarch2014IsRoundedUpToThreeMonths()
        {
            Assert.AreEqual(3, new DateTime(2014, 1, 1).DurationInMonths(new DateTime(2014, 3, 23)));
        }

        [TestMethod]
        public void JanuaryToJanuaryIsOneMonth()
        {
            Assert.AreEqual(1, new DateTime(2014, 1, 1).DurationInMonths(new DateTime(2014, 1, 1)));
        }

        [TestMethod]
        public void JanuaryToJuneIsFiveMonths()
        {
            Assert.AreEqual(5, new DateTime(2014, 1, 1).DurationInMonths(new DateTime(2014, 6, 1)));
        }

        [TestMethod]
        public void October2013ToMarch2014IsFiveMonths()
        {
            Assert.AreEqual(5, new DateTime(2013, 10, 1).DurationInMonths(new DateTime(2014, 3, 1)));
        }

        [TestMethod]
        public void OneStandardPayMonthIsOneMonth1()
        {
            Assert.AreEqual(1, new DateTime(2014, 1, 20).DurationInMonths(new DateTime(2014, 2, 19)));
        }

        [TestMethod]
        public void OneStandardPayMonthIsOneMonth2()
        {
            Assert.AreEqual(1, new DateTime(2014, 1, 15).DurationInMonths(new DateTime(2014, 2, 14)));
        }
    }
}