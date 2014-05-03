using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Reports;

namespace BudgetAnalyser.Engine.Budget
{
    [AutoRegisterWithIoC]
    public class BudgetCollectionToDataBudgetCollectionMapper
    {
        private readonly BudgetModelToDataBudgetModelMapper budgetModelMapper;

        public BudgetCollectionToDataBudgetCollectionMapper([NotNull] BudgetModelToDataBudgetModelMapper budgetModelMapper)
        {
            if (budgetModelMapper == null)
            {
                throw new ArgumentNullException("budgetModelMapper");
            }

            this.budgetModelMapper = budgetModelMapper;
        }

        public DataBudgetCollection Map(BudgetCollection budgetCollection)
        {
            var collection = new DataBudgetCollection
            {
                FileName = budgetCollection.FileName,
                Budgets = budgetCollection.Select(b => this.budgetModelMapper.Map(b)).ToList()
            };

            // Rationalise the buckets to ensure the same object is used for repeated uses of the same bucket. Only one bucket can exist with each unique code.
            var bucketRegister = new Dictionary<string, BudgetBucket>();
            foreach (var model in collection.Budgets)
            {
                foreach (var income in model.Incomes)
                {
                    IncludeBucket(bucketRegister, income.Bucket);
                }

                foreach (var expense in model.Expenses)
                {
                    IncludeBucket(bucketRegister, expense.Bucket);
                }
            }

            foreach (var model in collection.Budgets)
            {
                foreach (var income in model.Incomes)
                {
                    income.Bucket = bucketRegister[income.Bucket.Code];
                }

                foreach (var expense in model.Expenses)
                {
                    expense.Bucket = bucketRegister[expense.Bucket.Code];
                }
            }

            return collection;
        }

        private static void IncludeBucket(Dictionary<string, BudgetBucket> bucketRegister, BudgetBucket bucket)
        {
            if (!bucketRegister.ContainsKey(bucket.Code))
            {
                bucketRegister.Add(bucket.Code, bucket);
            }
        }
    }
}
