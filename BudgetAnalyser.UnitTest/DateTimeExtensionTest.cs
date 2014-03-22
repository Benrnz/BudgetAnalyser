using System;
using BudgetAnalyser.Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest
{
    [TestClass]
    public class DateTimeExtensionTest
    {
        [TestMethod]
        public void JanuaryToJuneIsFiveMonths()
        {
            Assert.AreEqual(5, new DateTime(2014, 1, 1).DurationInMonths(new DateTime(2014, 6, 1)));
        }

        [TestMethod]
        public void JanuaryToJanuaryIsOneMonth()
        {
            Assert.AreEqual(1, new DateTime(2014, 1, 1).DurationInMonths(new DateTime(2014, 1, 1)));
        }

        [TestMethod]
        public void FebruaryToMarchIsOneMonth()
        {
            Assert.AreEqual(1, new DateTime(2014, 2, 1).DurationInMonths(new DateTime(2014, 3, 1)));
        }

        [TestMethod]
        public void October2013ToMarch2014IsFiveMonths()
        {
            Assert.AreEqual(5, new DateTime(2013, 10, 1).DurationInMonths(new DateTime(2014, 3, 1)));
        }

        [TestMethod]
        public void January2014To23rdMarch2014IsRoundedUpToThreeMonths()
        {
            Assert.AreEqual(3, new DateTime(2014, 1, 1).DurationInMonths(new DateTime(2014, 3, 23)));
        }

        [TestMethod]
        public void January2014To17thMarch2014IsRoundedDownToTwoMonths()
        {
            Assert.AreEqual(2, new DateTime(2014, 1, 1).DurationInMonths(new DateTime(2014, 3, 17)));
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
