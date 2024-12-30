using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;

namespace BudgetAnalyser.Engine.UnitTest.TestData
{
    internal static class BudgetModelTestData
    {
        public static IEnumerable<BudgetBucketDto> CreateBudgetBucketDtoTestData1()
        {
            return new[]
            {
                new BudgetBucketDto { Code = TestDataConstants.RentBucketCode, Type = BucketDtoType.SpentPeriodicallyExpense, Description = "Rent for my bachelor pad" },
                new BudgetBucketDto { Code = TestDataConstants.FoodBucketCode, Type = BucketDtoType.SpentPeriodicallyExpense, Description = "Groceries, staples, and necessities" },
                new BudgetBucketDto { Code = TestDataConstants.WaterBucketCode, Type = BucketDtoType.SpentPeriodicallyExpense, Description = "Water Rates" },
                new BudgetBucketDto { Code = TestDataConstants.HairBucketCode, Type = BucketDtoType.SavedUpForExpense, Description = "Haircuts" },
                new BudgetBucketDto { Code = TestDataConstants.CarMtcBucketCode, Type = BucketDtoType.SavedUpForExpense, Description = "Car Maintenance" },
                new BudgetBucketDto { Code = TestDataConstants.PhoneBucketCode, Type = BucketDtoType.SavedUpForExpense, Description = "Phone and Internet" },
                new BudgetBucketDto { Code = TestDataConstants.IncomeBucketCode, Type = BucketDtoType.Income, Description = "Salary from Lawn Mowing business" }
            };
        }

        public static BudgetCollection CreateCollectionWith1And2()
        {
            var collection = new BudgetCollection(
                new[]
                {
                    CreateTestData1(),
                    CreateTestData2()
                })
            {
                StorageKey = @"C:\Temp\Foo.xaml"
            };
            return collection;
        }

        public static BudgetCollection CreateCollectionWith5()
        {
            return new BudgetCollection(new[] { BudgetModelTestData.CreateTestData5() });
        }

        /// <summary>
        ///     A budget model that is effective from 1/1/2013
        /// </summary>
        public static BudgetModel CreateTestData1()
        {
            var budget = new BudgetModel
            {
                EffectiveFrom = new DateTime(2013, 01, 01),
                Name = TestDataConstants.Budget1Name
            };

            var expenses = new List<Expense>(
                new[]
                {
                    new Expense
                    {
                        Amount = 95M,
                        Bucket = new SavedUpForExpenseBucket(TestDataConstants.CarMtcBucketCode, "Car maintenance")
                    },
                    new Expense
                    {
                        Amount = 55M,
                        Bucket = new SpentPerPeriodExpenseBucket(TestDataConstants.HairBucketCode, "Hair cuts")
                    },
                    new Expense
                    {
                        Amount = 175M,
                        Bucket = new SpentPerPeriodExpenseBucket(TestDataConstants.PowerBucketCode, "Power")
                    }
                });

            var incomes = new List<Income>(
                new[]
                {
                    new Income
                    {
                        Amount = 1500M,
                        Bucket = new IncomeBudgetBucket(TestDataConstants.IncomeBucketCode, "Pay")
                    }
                });

            budget.Update(incomes, expenses);
            return budget;
        }

        /// <summary>
        ///     A budget model that is effective from 20/1/14
        /// </summary>
        public static BudgetModel CreateTestData2()
        {
            var budget = new BudgetModel
            {
                EffectiveFrom = new DateTime(2014, 01, 20),
                Name = TestDataConstants.Budget2Name
            };

            var expenses = new List<Expense>(
                new[]
                {
                    new Expense
                    {
                        Amount = 120M,
                        Bucket = new SavedUpForExpenseBucket(TestDataConstants.PhoneBucketCode, "Phone/Internet")
                    },
                    new Expense
                    {
                        Amount = 100M,
                        Bucket = new SavedUpForExpenseBucket(TestDataConstants.CarMtcBucketCode, "Car maintenance")
                    },
                    new Expense
                    {
                        Amount = 65M,
                        Bucket = new SpentPerPeriodExpenseBucket(TestDataConstants.HairBucketCode, "Hair cuts")
                    },
                    new Expense
                    {
                        Amount = 185M,
                        Bucket = new SpentPerPeriodExpenseBucket(TestDataConstants.PowerBucketCode, "Power")
                    }
                });

            var incomes = new List<Income>(
                new[]
                {
                    new Income
                    {
                        Amount = 1600M,
                        Bucket = new IncomeBudgetBucket(TestDataConstants.IncomeBucketCode, "Pay")
                    }
                });

            budget.Update(incomes, expenses);
            return budget;
        }

        /// <summary>
        ///     A budget model that is effective from 01/1/13.
        ///     Includes InsHome, and PhoneInternet, CarMtc, HairCut, Power
        ///     Designed for use with LedgerBookTestData5 and StatementModelTestData5
        /// </summary>
        public static BudgetModel CreateTestData5()
        {
            var budget = new BudgetModel
            {
                EffectiveFrom = new DateTime(2013, 01, 01),
                Name = TestDataConstants.Budget5Name
            };

            var expenses = new List<Expense>(
                new[]
                {
                    new Expense
                    {
                        Amount = 300M,
                        Bucket = new SavedUpForExpenseBucket(TestDataConstants.InsuranceHomeBucketCode, "Home Insurance")
                    },
                    new Expense
                    {
                        Amount = 120M,
                        Bucket = new SavedUpForExpenseBucket(TestDataConstants.PhoneBucketCode, "Phone/Internet")
                    },
                    new Expense
                    {
                        Amount = 100M,
                        Bucket = new SavedUpForExpenseBucket(TestDataConstants.CarMtcBucketCode, "Car maintenance")
                    },
                    new Expense
                    {
                        Amount = 65M,
                        Bucket = new SpentPerPeriodExpenseBucket(TestDataConstants.HairBucketCode, "Hair cuts")
                    },
                    new Expense
                    {
                        Amount = 185M,
                        Bucket = new SpentPerPeriodExpenseBucket(TestDataConstants.PowerBucketCode, "Power")
                    }
                });

            var incomes = new List<Income>(
                new[]
                {
                    new Income
                    {
                        Amount = 2600M,
                        Bucket = new IncomeBudgetBucket(TestDataConstants.IncomeBucketCode, "Pay")
                    }
                });

            budget.Update(incomes, expenses);
            return budget;
        }
    }
}
