using System;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest
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

        private BudgetCollection CreateSubject()
        {
            return new BudgetCollection(new[]
            {
                TestData.BudgetModelTestData.CreateTestData1(),
                TestData.BudgetModelTestData.CreateTestData2(),
            });
        }
    }
}
