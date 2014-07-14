using System;
using System.Collections.Generic;
using System.Xaml;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Budget
{
    [TestClass]
    public class DtoToBudgetModelMapperTest
    {
        [TestMethod]
        public void OutputBudgetModelTestData1()
        {
            var testData = new BudgetModelDto
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
                        BudgetBucketCode = TestDataConstants.IncomeBucketCode,
                    },
                },
                Expenses = new List<ExpenseDto>
                {
                    new ExpenseDto
                    {
                        Amount = 350.11M,
                        BudgetBucketCode = TestDataConstants.PhoneBucketCode,
                    },
                    new ExpenseDto
                    {
                        Amount = 221.22M,
                        BudgetBucketCode = TestDataConstants.PowerBucketCode,
                    },
                },
            };
            string serialised = XamlServices.Save(testData);
            Console.WriteLine(serialised);
        }
    }
}