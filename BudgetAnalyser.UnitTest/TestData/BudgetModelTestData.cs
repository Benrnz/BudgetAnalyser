using System;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.UnitTest.TestData
{
    internal static class BudgetModelTestData
    {
        /// <summary>
        /// A budget model that is effective from 1/1/2013
        /// </summary>
        public static BudgetModel CreateTestData1()
        {
            var budget = new BudgetModel
            {
                EffectiveFrom = new DateTime(2013, 01, 01),
                Id = Guid.NewGuid(),
                Name = TestDataConstants.Budget1Name,
            };

            budget.Expenses.AddRange(new[]
            {
                new Expense
                {
                    Amount = 95M,
                    Bucket = new SavedUpForExpense(TestDataConstants.CarMtcBucketCode, "Car maintenance"),
                }, 
                new Expense
                {
                    Amount = 55M,
                    Bucket = new SpentMonthlyExpense(TestDataConstants.HairBucketCode, "Hair cuts"),
                },
                new Expense
                {
                    Amount = 175M,
                    Bucket = new SpentMonthlyExpense(TestDataConstants.PowerBucketCode, "Power"),
                }
            });

            budget.Incomes.Add(
                new Income
                {
                    Amount = 1500M,
                    Bucket = new IncomeBudgetBucket("PAY", "Pay"),
                });

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
                Id = Guid.NewGuid(),
                Name = TestDataConstants.Budget2Name,
            };

            budget.Expenses.AddRange(new[]
            {
                new Expense
                {
                    Amount = 100M,
                    Bucket = new SavedUpForExpense(TestDataConstants.CarMtcBucketCode, "Car maintenance"),
                }, 
                new Expense
                {
                    Amount = 65M,
                    Bucket = new SpentMonthlyExpense(TestDataConstants.HairBucketCode, "Hair cuts"),
                },
                new Expense
                {
                    Amount = 185M,
                    Bucket = new SpentMonthlyExpense(TestDataConstants.PowerBucketCode, "Power"),
                }
            });

            budget.Incomes.Add(
                new Income
                {
                    Amount = 1600M,
                    Bucket = new IncomeBudgetBucket("PAY", "Pay"),
                });

            return budget;
        }
    }
}
