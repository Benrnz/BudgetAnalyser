using System;
using System.Collections.Generic;
using System.Xaml;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest
{
    [TestClass]
    public class DataBudgetModelToBudgetModelMapperTest
    {
        [TestMethod]
        public void OutputBudgetModelTestData1()
        {
            var testData = new DataBudgetModel
            {
                EffectiveFrom = new DateTime(2014, 4, 28),
                LastModified = new DateTime(2014, 5, 2),
                LastModifiedComment = "The quick brown fox jumped over the lazy dog.",
                Name = "Foo data budget",
                Incomes = new List<Income>
                {
                    new Income
                    {
                        Amount = 2300.23M,
                        Bucket = StatementModelTestData.IncomeBucket,
                    },
                },
                Expenses = new List<Expense>
                {
                    new Expense
                    {
                        Amount = 350.11M,
                        Bucket = StatementModelTestData.PhoneBucket,
                    },
                    new Expense
                    {
                        Amount = 221.22M,
                        Bucket = StatementModelTestData.PowerBucket,
                    },
                },
            };
            string serialised = XamlServices.Save(testData);
            Console.WriteLine(serialised);
        }
    }
}
