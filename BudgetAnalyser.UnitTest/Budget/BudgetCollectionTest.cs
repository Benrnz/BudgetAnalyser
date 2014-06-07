using System;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Budget
{
    [TestClass]
    public class BudgetCollectionTest
    {
        [TestMethod]
        [ExpectedException(typeof(BudgetException))]
        public void ForDates_WithDatesOutsideBudgetCollection_ShouldThrow()
        {
            var subject = CreateSubject();

            var results = subject.ForDates(DateTime.MinValue, DateTime.MaxValue);

            Assert.Fail();
        }

        [TestMethod]
        public void ForDate_WithEarlierDateThanFirstBudget_ShouldReturnNull()
        {
            var subject = CreateSubject();

            var result = subject.ForDate(DateTime.MinValue);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void ForDate_1_1_2014_ShouldReturnBudget1()
        {
            var subject = CreateSubject();

            var result = subject.ForDate(new DateTime(2014, 1, 1));

            Assert.AreSame(subject.First(b => b.Name == TestDataConstants.Budget1Name), result);
        }

        [TestMethod]
        public void ForDate_1_1_2013_ShouldReturnBudget1()
        {
            var subject = CreateSubject();

            var result = subject.ForDate(new DateTime(2013, 1, 1));

            Assert.AreSame(subject.First(b => b.Name == TestDataConstants.Budget1Name), result);
        }

        [TestMethod]
        public void ForDate_25_1_2014_ShouldReturnBudget2()
        {
            var subject = CreateSubject();

            var result = subject.ForDate(new DateTime(2014, 1, 25));

            Assert.AreSame(subject.First(b => b.Name == TestDataConstants.Budget2Name), result);
        }

        private BudgetCollection CreateSubject()
        {
            return new BudgetCollection(new[]
            {
                BudgetModelTestData.CreateTestData2(),
                BudgetModelTestData.CreateTestData1(),
            });
        }
    }
}
