using System;
using System.Collections.Generic;
using System.Linq;
using System.Xaml;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Budget
{
    [TestClass]
    public class DtoToBudgetModelMapperTest
    {
        private BudgetModel Result { get; set; }

        private BudgetModelDto TestData
        {
            get
            {
                return
                    new BudgetModelDto
                    {
                        EffectiveFrom = new DateTime(2014, 4, 28),
                        LastModified = new DateTime(2014, 5, 2),
                        LastModifiedComment = "The quick brown fox jumped over the lazy dog.",
                        Name = "Foo data budget",
                        Incomes = new List<IncomeDto>
                        {
                            new IncomeDto
                            {
                                Amount = 2300.23M,
                                BudgetBucketCode = TestDataConstants.IncomeBucketCode
                            }
                        },
                        Expenses = new List<ExpenseDto>
                        {
                            new ExpenseDto
                            {
                                Amount = 350.11M,
                                BudgetBucketCode = TestDataConstants.PhoneBucketCode
                            },
                            new ExpenseDto
                            {
                                Amount = 221.22M,
                                BudgetBucketCode = TestDataConstants.PowerBucketCode
                            }
                        }
                    };
            }
        }

        [TestMethod]
        public void OutputBudgetModelTestData1()
        {
            string serialised = XamlServices.Save(TestData);
            Console.WriteLine(serialised);
        }

        [TestMethod]
        public void ShouldMapAllExpenses()
        {
            Assert.AreEqual(TestData.Expenses.Count(), Result.Expenses.Count());
        }

        [TestMethod]
        public void ShouldMapAllIncomes()
        {
            Assert.AreEqual(TestData.Incomes.Count(), Result.Incomes.Count());
        }

        [TestMethod]
        public void ShouldMapEffectiveFrom()
        {
            Assert.AreEqual(new DateTime(2014, 4, 28), Result.EffectiveFrom);
        }

        [TestMethod]
        public void ShouldMapExpenses()
        {
            foreach (Expense expense in Result.Expenses)
            {
                Assert.AreEqual(TestData.Expenses.First(e => e.BudgetBucketCode == expense.Bucket.Code && expense.Amount == e.Amount).Amount, expense.Amount);
            }
        }

        [TestMethod]
        public void ShouldMapIncomes()
        {
            foreach (Income incomes in Result.Incomes)
            {
                Assert.AreEqual(TestData.Incomes.First(e => e.BudgetBucketCode == incomes.Bucket.Code && incomes.Amount == e.Amount).Amount, incomes.Amount);
            }
        }

        [TestMethod]
        public void ShouldMapLastModified()
        {
            Assert.AreEqual(new DateTime(2014, 5, 2), Result.LastModified);
        }

        [TestMethod]
        public void ShouldMapLastModifiedComment()
        {
            Assert.AreEqual("The quick brown fox jumped over the lazy dog.", Result.LastModifiedComment);
        }

        [TestMethod]
        public void ShouldMapLastName()
        {
            Assert.AreEqual("Foo data budget", Result.Name);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            var subject = new Mapper_BudgetModelDto_BudgetModel(new BucketBucketRepoAlwaysFind());
            Result = subject.ToModel(TestData);
        }
    }
}