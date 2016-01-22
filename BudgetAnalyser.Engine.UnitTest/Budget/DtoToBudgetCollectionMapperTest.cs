using System;
using System.Collections.Generic;
using System.Xaml;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.Engine.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Budget
{
    [TestClass]
    public class DtoToBudgetCollectionMapperTest
    {
        [TestMethod]
        public void OutputBudgetModelTestData1()
        {
            var testData1 = new BudgetModelDto
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

            var testData2 = new BudgetModelDto
            {
                EffectiveFrom = new DateTime(2012, 2, 29),
                LastModified = new DateTime(2013, 6, 6),
                LastModifiedComment = "Spatchcock.",
                Name = "Old data budget",
                Incomes = new List<IncomeDto>
                {
                    new IncomeDto
                    {
                        Amount = 2100.23M,
                        BudgetBucketCode = TestDataConstants.IncomeBucketCode
                    }
                },
                Expenses = new List<ExpenseDto>
                {
                    new ExpenseDto
                    {
                        Amount = 310.11M,
                        BudgetBucketCode = TestDataConstants.PhoneBucketCode
                    },
                    new ExpenseDto
                    {
                        Amount = 111.22M,
                        BudgetBucketCode = TestDataConstants.PowerBucketCode
                    }
                }
            };

            var collection = new BudgetCollectionDto
            {
                Budgets = new List<BudgetModelDto> { testData1, testData2 },
                StorageKey = "Foo.xml"
            };

            string serialised = XamlServices.Save(collection);
            Console.WriteLine(serialised);
        }
    }
}