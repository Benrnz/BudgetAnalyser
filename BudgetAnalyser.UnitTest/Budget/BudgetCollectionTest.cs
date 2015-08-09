using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.UnitTest.TestData;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Budget
{
    [TestClass]
    public class BudgetCollectionTest
    {
        [TestMethod]
        public void ForDate_1_1_2013_ShouldReturnBudget1()
        {
            BudgetCollection subject = Arrange();

            BudgetModel result = subject.ForDate(new DateTime(2013, 1, 1));

            Assert.AreSame(subject.First(b => b.Name == TestDataConstants.Budget1Name), result);
        }

        [TestMethod]
        public void ForDate_1_1_2014_ShouldReturnBudget1()
        {
            BudgetCollection subject = Arrange();

            BudgetModel result = subject.ForDate(new DateTime(2014, 1, 1));

            Assert.AreSame(subject.First(b => b.Name == TestDataConstants.Budget1Name), result);
        }

        [TestMethod]
        public void ForDate_25_1_2014_ShouldReturnBudget2()
        {
            BudgetCollection subject = Arrange();

            BudgetModel result = subject.ForDate(new DateTime(2014, 1, 25));

            Assert.AreSame(subject.First(b => b.Name == TestDataConstants.Budget2Name), result);
        }

        [TestMethod]
        public void ForDate_WithEarlierDateThanFirstBudget_ShouldReturnNull()
        {
            BudgetCollection subject = Arrange();

            BudgetModel result = subject.ForDate(DateTime.MinValue);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void ForDates_1_1_2013_to_20_1_2014_ShouldReturnBudget1And2()
        {
            BudgetCollection subject = Arrange();
            IEnumerable<BudgetModel> result = subject.ForDates(new DateTime(2013, 1, 1), new DateTime(2014, 1, 20));
            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(BudgetException))]
        public void ForDates_WithDatesOutsideBudgetCollection_ShouldThrow()
        {
            BudgetCollection subject = Arrange();

            subject.ForDates(DateTime.MinValue, DateTime.MaxValue);

            Assert.Fail();
        }

        [TestMethod]
        public void OutputBudgetCollection()
        {
            BudgetCollection subject = Arrange();
            foreach (BudgetModel budget in subject)
            {
                Console.WriteLine("Budget: '{0}' EffectiveFrom: {1:d}", budget.Name, budget.EffectiveFrom);
            }
        }

        [TestMethod]
        public void ShouldHaveAKnownNumberOfProperties()
        {
            // If this test breaks consider putting the new property into the Mappers and DTO's before updating the count.
            IEnumerable<PropertyInfo> properties = typeof(BudgetCollection).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite);
            Assert.AreEqual(1, properties.Count());
        }

        [TestMethod]
        public void ValidateShouldFixGivenBudgetsWithDuplicateEffectiveDates()
        {
            BudgetCollection subject = Arrange();
            subject.Add(
                new BudgetModelFake
                {
                    EffectiveFrom = subject.First().EffectiveFrom,
                    Name = Guid.NewGuid().ToString()
                });
            subject.Validate(new StringBuilder());

            Assert.IsTrue(subject.GroupBy(b => b.EffectiveFrom).Sum(group => group.Count()) == 3);
        }

        [TestMethod]
        public void ValidateShouldReturnFalseGivenOneBadBudget()
        {
            BudgetCollection subject = Arrange();
            subject.Add(
                new BudgetModelFake
                {
                    EffectiveFrom = DateTime.Now,
                    Name = "Foo123",
                    InitialiseOverride = () => { },
                    ValidateOverride = msg => false
                });

            bool result = subject.Validate(new StringBuilder());

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ValidateShouldReturnTrueGivenBudgetsWithDuplicateEffectiveDates()
        {
            BudgetCollection subject = Arrange();
            subject.Add(
                new BudgetModelFake
                {
                    EffectiveFrom = subject.First().EffectiveFrom,
                    Name = Guid.NewGuid().ToString()
                });

            Assert.IsTrue(subject.Validate(new StringBuilder()));
        }

        [TestMethod]
        public void ValidateShouldReturnTrueGivenGoodBudgets()
        {
            BudgetCollection subject = Arrange();
            bool result = subject.Validate(new StringBuilder());
            Assert.IsTrue(result);
        }

        private BudgetCollection Arrange()
        {
            return new BudgetCollection(
                new[]
                {
                    BudgetModelTestData.CreateTestData2(),
                    BudgetModelTestData.CreateTestData1()
                });
        }
    }
}