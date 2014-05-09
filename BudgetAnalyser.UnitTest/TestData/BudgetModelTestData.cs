using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.UnitTest.TestData
{
    internal static class BudgetModelTestData
    {
        public static BudgetCollection CreateCollectionWith1And2()
        {
            var collection = new BudgetCollection( new[]
            {
                CreateTestData1(),
                CreateTestData2(),
            });
            collection.FileName = @"C:\Temp\Foo.xaml";
            return collection;
        }

        /// <summary>
        /// A budget model that is effective from 1/1/2013
        /// </summary>
        public static BudgetModel CreateTestData1()
        {
            var budget = new BudgetModel
            {
                EffectiveFrom = new DateTime(2013, 01, 01),
                Name = TestDataConstants.Budget1Name,
            };

            var expenses = new List<Expense>(new[]
            {
                new Expense
                {
                    Amount = 95M,
                    Bucket = new SavedUpForExpenseBucket(TestDataConstants.CarMtcBucketCode, "Car maintenance"),
                }, 
                new Expense
                {
                    Amount = 55M,
                    Bucket = new SpentMonthlyExpenseBucket(TestDataConstants.HairBucketCode, "Hair cuts"),
                },
                new Expense
                {
                    Amount = 175M,
                    Bucket = new SpentMonthlyExpenseBucket(TestDataConstants.PowerBucketCode, "Power"),
                }
            });

            var incomes = new List<Income>(new[] 
            {
                    new Income
                    {
                        Amount = 1500M,
                        Bucket = new IncomeBudgetBucket(TestDataConstants.IncomeBucketCode, "Pay"),
                    }
            });

            budget.Update(incomes, expenses);
            return budget;
        }

        /// <summary>
        /// A budget model that is effective from 20/1/14
        /// </summary>
        public static BudgetModel CreateTestData2()
        {
            var budget = new BudgetModel
            {
                EffectiveFrom = new DateTime(2014, 01, 20),
                Name = TestDataConstants.Budget2Name,
            };

            var expenses = new List<Expense>(new[]
            {
                new Expense
                {
                    Amount = 100M,
                    Bucket = new SavedUpForExpenseBucket(TestDataConstants.CarMtcBucketCode, "Car maintenance"),
                }, 
                new Expense
                {
                    Amount = 65M,
                    Bucket = new SpentMonthlyExpenseBucket(TestDataConstants.HairBucketCode, "Hair cuts"),
                },
                new Expense
                {
                    Amount = 185M,
                    Bucket = new SpentMonthlyExpenseBucket(TestDataConstants.PowerBucketCode, "Power"),
                }
            });

            var incomes = new List<Income>(new[] 
            {
                new Income
                {
                    Amount = 1600M,
                    Bucket = new IncomeBudgetBucket(TestDataConstants.IncomeBucketCode, "Pay"),
                }
            });

            budget.Update(incomes, expenses);
            return budget;
        }
    }
}
