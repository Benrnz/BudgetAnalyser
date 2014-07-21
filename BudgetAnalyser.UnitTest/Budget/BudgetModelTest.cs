using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.UnitTest.TestData;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Budget
{
    [TestClass]
    public class BudgetModelTest
    {
        public StringBuilder Logs { get; private set; }

        [TestMethod]
        public void AfterConstructionEffectiveDateIsValidDate()
        {
            var subject = new BudgetModel();

            Assert.AreNotEqual(DateTime.MinValue, subject.EffectiveFrom);
        }

        [TestMethod]
        public void AfterConstructionLastModifiedDateIsValidDate()
        {
            var subject = new BudgetModel();

            Assert.AreNotEqual(DateTime.MinValue, subject.LastModified);
        }

        [TestMethod]
        public void AfterInitialiseExpensesShouldBeInDescendingOrder()
        {
            BudgetModel subject = BudgetModelTestData.CreateTestData1();

            EnsureDescendingOrder(subject.Expenses);
        }

        [TestMethod]
        public void AfterInitialiseIncomesShouldBeInDescendingOrder()
        {
            BudgetModel subject = BudgetModelTestData.CreateTestData1();

            EnsureDescendingOrder(subject.Incomes);
        }

        [TestMethod]
        public void AfterUpdateExpensesAreReplaced()
        {
            BudgetModel subject = BudgetModelTestData.CreateTestData1();

            var expenses = new List<Expense>
            {
                new Expense { Amount = 4444, Bucket = new SpentMonthlyExpenseBucket("Horse", "Shit") },
                new Expense { Amount = 9999, Bucket = new SavedUpForExpenseBucket("Foo", "Bar") },
            };

            subject.Update(subject.Incomes, expenses);

            Assert.AreEqual(4444M + 9999M, subject.Expenses.Sum(e => e.Amount));
        }

        [TestMethod]
        public void AfterUpdateExpensesAreStillInDescendingOrder()
        {
            BudgetModel subject = BudgetModelTestData.CreateTestData1();

            var expenses = new List<Expense>
            {
                new Expense { Amount = 4444, Bucket = new SpentMonthlyExpenseBucket("Horse", "Shit") },
                new Expense { Amount = 9999, Bucket = new SavedUpForExpenseBucket("Foo", "Bar") },
            };

            subject.Update(subject.Incomes, expenses);

            EnsureDescendingOrder(subject.Expenses);
        }

        [TestMethod]
        public void AfterUpdateIncomesAreReplaced()
        {
            BudgetModel subject = BudgetModelTestData.CreateTestData1();

            var incomes = new List<Income>
            {
                new Income { Amount = 9999, Bucket = new IncomeBudgetBucket("Foo", "Bar") },
                new Income { Amount = 4444, Bucket = new IncomeBudgetBucket("Horse", "Shit") },
            };

            subject.Update(incomes, subject.Expenses);

            Assert.AreEqual(9999M + 4444M, subject.Incomes.Sum(i => i.Amount));
        }

        [TestMethod]
        public void AfterUpdateIncomesAreStillInDescendingOrder()
        {
            BudgetModel subject = BudgetModelTestData.CreateTestData1();

            var incomes = new List<Income>
            {
                new Income { Amount = 4444, Bucket = new IncomeBudgetBucket("Horse", "Shit") },
                new Income { Amount = 9999, Bucket = new IncomeBudgetBucket("Foo", "Bar") },
            };

            subject.Update(incomes, subject.Expenses);

            EnsureDescendingOrder(subject.Incomes);
        }

        [TestMethod]
        public void AfterUpdateLastModifiedIsUpdated()
        {
            BudgetModel subject = BudgetModelTestData.CreateTestData1();

            DateTime lastUpdated = subject.LastModified;

            Thread.Sleep(10);
            subject.Update(subject.Incomes, subject.Expenses);

            Assert.AreNotEqual(lastUpdated, subject.LastModified);
        }

        [TestMethod]
        public void CalculatedSurplusShouldBeExpectedValue()
        {
            BudgetModel subject = BudgetModelTestData.CreateTestData1();

            Assert.AreEqual(1175M, subject.Surplus);
        }

        [TestMethod]
        public void ListsAreInitialised()
        {
            var subject = new BudgetModel();

            Assert.IsNotNull(subject.Incomes);
            Assert.IsNotNull(subject.Expenses);
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationWarningException))]
        public void SurplusCannotBeUsedInTheExpenseList()
        {
            BudgetModel subject = BudgetModelTestData.CreateTestData1();

            List<Expense> myExpenses = subject.Expenses.ToList();
            myExpenses.Add(new Expense { Amount = 445M, Bucket = new SurplusBucket() });
            List<Income> myIncomes = subject.Incomes.ToList();

            subject.Update(myIncomes, myExpenses);

            Assert.Fail();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            this.Logs = new StringBuilder();
        }

        private static void EnsureDescendingOrder(IEnumerable<BudgetItem> items)
        {
            decimal previousAmount = decimal.MaxValue;
            foreach (BudgetItem item in items)
            {
                decimal current = item.Amount;
                if (current > previousAmount)
                {
                    Assert.Fail("Expenses are not in descending order.");
                }

                previousAmount = current;
            }
        }
    }
}