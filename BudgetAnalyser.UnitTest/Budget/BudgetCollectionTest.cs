﻿using System;
using System.Linq;
using System.Runtime.Remoting.Messaging;
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
        [ExpectedException(typeof(BudgetException))]
        public void ForDates_WithDatesOutsideBudgetCollection_ShouldThrow()
        {
            var subject = Arrange();

            var results = subject.ForDates(DateTime.MinValue, DateTime.MaxValue);

            Assert.Fail();
        }

        [TestMethod]
        public void ForDate_WithEarlierDateThanFirstBudget_ShouldReturnNull()
        {
            var subject = Arrange();

            var result = subject.ForDate(DateTime.MinValue);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void ForDate_1_1_2014_ShouldReturnBudget1()
        {
            var subject = Arrange();

            var result = subject.ForDate(new DateTime(2014, 1, 1));

            Assert.AreSame(subject.First(b => b.Name == TestDataConstants.Budget1Name), result);
        }

        [TestMethod]
        public void ForDate_1_1_2013_ShouldReturnBudget1()
        {
            var subject = Arrange();

            var result = subject.ForDate(new DateTime(2013, 1, 1));

            Assert.AreSame(subject.First(b => b.Name == TestDataConstants.Budget1Name), result);
        }

        [TestMethod]
        public void ForDate_25_1_2014_ShouldReturnBudget2()
        {
            var subject = Arrange();

            var result = subject.ForDate(new DateTime(2014, 1, 25));

            Assert.AreSame(subject.First(b => b.Name == TestDataConstants.Budget2Name), result);
        }

        [TestMethod]
        public void ForDates_1_1_2013_to_20_1_2014_ShouldReturnBudget1And2()
        {
            var subject = Arrange();
            var result = subject.ForDates(new DateTime(2013, 1, 1), new DateTime(2014, 1, 20));
            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public void ValidateShouldReturnTrueGivenGoodBudgets()
        {
            var subject = Arrange();
            var result = subject.Validate(new StringBuilder());
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ValidateShouldReturnFalseGivenOneBadBudget()
        {
            var subject = Arrange();
            subject.Add(new BudgetModelFake
            {
                EffectiveFrom = DateTime.Now,
                Name = "Foo123",
                InitialiseOverride = () => { },
                ValidateOverride = msg => false,
            });

            var result = subject.Validate(new StringBuilder());

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ValidateShouldReturnTrueGivenBudgetsWithDuplicateEffectiveDates()
        {
            var subject = Arrange();
            subject.Add(new BudgetModelFake
            {
                EffectiveFrom = subject.First().EffectiveFrom,
                Name = Guid.NewGuid().ToString(),
            });

            Assert.IsTrue(subject.Validate(new StringBuilder()));
        }

        [TestMethod]
        public void ValidateShouldFixGivenBudgetsWithDuplicateEffectiveDates()
        {
            var subject = Arrange();
            subject.Add(new BudgetModelFake
            {
                EffectiveFrom = subject.First().EffectiveFrom,
                Name = Guid.NewGuid().ToString(),
            });
            subject.Validate(new StringBuilder());

            Assert.IsTrue(subject.GroupBy(b => b.EffectiveFrom).Sum(group => group.Count()) == 3);
        }

        private BudgetCollection Arrange()
        {
            return new BudgetCollection(new[]
            {
                BudgetModelTestData.CreateTestData2(),
                BudgetModelTestData.CreateTestData1(),
            });
        }
    }
}
