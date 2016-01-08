using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BudgetAnalyser.Engine.Budget.Data
{
    internal partial class Mapper_BudgetModelDto_BudgetModel
    {
        private readonly IBudgetBucketRepository bucketRepo;

        public Mapper_BudgetModelDto_BudgetModel(IBudgetBucketRepository bucketRepo)
        {
            this.bucketRepo = bucketRepo;
        }

        partial void ToDtoPostprocessing(ref BudgetModelDto dto, BudgetModel model)
        {
            var mapper3 = new Mapper_ExpenseDto_Expense(this.bucketRepo);
            var expenses8 = model.Expenses.Select(mapper3.ToDto).ToList();
            dto.Expenses = expenses8;
            var mapper4 = new Mapper_IncomeDto_Income(this.bucketRepo);
            var incomes9 = model.Incomes.Select(mapper4.ToDto).ToList();
            dto.Incomes = incomes9;
        }

        partial void ToModelPostprocessing(BudgetModelDto dto, ref BudgetModel model)
        {
            Type modelType = model.GetType();
            var mapper1 = new Mapper_ExpenseDto_Expense(this.bucketRepo);
            var expenses2 = dto.Expenses.Select(mapper1.ToModel).ToList();
            model.LastModified = dto.LastModified ?? DateTime.Now;
            modelType.GetProperty("Expenses").SetValue(model, expenses2);
            var mapper2 = new Mapper_IncomeDto_Income(this.bucketRepo);
            var incomes3 = dto.Incomes.Select(mapper2.ToModel).ToList();
            modelType.GetProperty("Incomes").SetValue(model, incomes3);
        }
    }
}