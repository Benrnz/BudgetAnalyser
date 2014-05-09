using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;

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

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Custom collection")]
        public DataBudgetCollection Map([NotNull] BudgetCollection budgetCollection)
        {
            if (budgetCollection == null)
            {
                throw new ArgumentNullException("budgetCollection");
            }

            var collection = new DataBudgetCollection
            {
                FileName = budgetCollection.FileName,
                Budgets = budgetCollection.Select(b => this.budgetModelMapper.Map(b)).ToList()
            };

            // Rationalise the buckets to ensure the same object is used for repeated uses of the same bucket. Only one bucket can exist with each unique code.
            var bucketRegister = new Dictionary<string, BudgetBucket>();
            foreach (DataBudgetModel model in collection.Budgets)
            {
                foreach (Income income in model.Incomes)
                {
                    IncludeBucket(bucketRegister, income.Bucket);
                }

                foreach (Expense expense in model.Expenses)
                {
                    IncludeBucket(bucketRegister, expense.Bucket);
                }
            }

            foreach (DataBudgetModel model in collection.Budgets)
            {
                foreach (Income income in model.Incomes)
                {
                    income.Bucket = bucketRegister[income.Bucket.Code];
                }

                foreach (Expense expense in model.Expenses)
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