using System.CodeDom.Compiler;
using System.Linq;
using System.Reflection;
using Rees.TangyFruitMapper;
using BudgetAnalyser.Engine.Budget;
using System.Collections.Generic;
using System;
using BudgetAnalyser.Engine.Budget.Data;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Budget.Data
{
    [GeneratedCode("1.0", "Tangy Fruit Mapper")]
    internal class Mapper_BudgetModelDto_BudgetModel : IDtoMapper<BudgetModelDto, BudgetModel>
    {
        private readonly IBudgetBucketRepository bucketRepo;

        public Mapper_BudgetModelDto_BudgetModel([NotNull] IBudgetBucketRepository bucketRepo)
        {
            if (bucketRepo == null) throw new ArgumentNullException(nameof(bucketRepo));
            this.bucketRepo = bucketRepo;
        }

        public BudgetModel ToModel(BudgetModelDto dto)
        {
            var model = new BudgetModel();
            var modelType = model.GetType();
            var effectiveFrom1 = dto.EffectiveFrom;
            model.EffectiveFrom = effectiveFrom1;
            var mapper1 = new Mapper_ExpenseDto_Expense(this.bucketRepo);
            var expenses2 = dto.Expenses.Select(mapper1.ToModel).ToList();
            modelType.GetProperty("Expenses").SetValue(model, expenses2);
            var mapper2 = new Mapper_IncomeDto_Income(this.bucketRepo);
            var incomes3 = dto.Incomes.Select(mapper2.ToModel).ToList();
            modelType.GetProperty("Incomes").SetValue(model, incomes3);
            var lastModified4 = dto.LastModified;
            model.LastModified = lastModified4 ?? DateTime.Now;
            var lastModifiedComment5 = dto.LastModifiedComment;
            model.LastModifiedComment = lastModifiedComment5;
            var name6 = dto.Name;
            model.Name = name6;
            return model;
        } // End ToModel Method

        public BudgetModelDto ToDto(BudgetModel model)
        {
            var dto = new BudgetModelDto();
            var effectiveFrom8 = model.EffectiveFrom;
            dto.EffectiveFrom = effectiveFrom8;
            var mapper3 = new Mapper_ExpenseDto_Expense(this.bucketRepo);
            var expenses9 = model.Expenses.Select(mapper3.ToDto).ToList();
            dto.Expenses = expenses9;
            var mapper4 = new Mapper_IncomeDto_Income(this.bucketRepo);
            var incomes10 = model.Incomes.Select(mapper4.ToDto).ToList();
            dto.Incomes = incomes10;
            var lastModified11 = model.LastModified;
            dto.LastModified = lastModified11;
            var lastModifiedComment12 = model.LastModifiedComment;
            dto.LastModifiedComment = lastModifiedComment12;
            var name13 = model.Name;
            dto.Name = name13;
            return dto;
        } // End ToDto Method
    } // End Class

    [GeneratedCode("1.0", "Tangy Fruit Mapper")]
    internal class Mapper_ExpenseDto_Expense : IDtoMapper<ExpenseDto, Expense>
    {
        private readonly IBudgetBucketRepository bucketRepo;

        public Mapper_ExpenseDto_Expense([NotNull] IBudgetBucketRepository bucketRepo)
        {
            if (bucketRepo == null) throw new ArgumentNullException(nameof(bucketRepo));
            this.bucketRepo = bucketRepo;
        }

        public Expense ToModel(ExpenseDto dto)
        {
            var model = new Expense();
            var modelType = model.GetType();
            var amount14 = dto.Amount;
            model.Amount = amount14;
            var bucket15 = this.bucketRepo.GetByCode(dto.BudgetBucketCode);
            model.Bucket = bucket15;
            return model;
        } // End ToModel Method

        public ExpenseDto ToDto(Expense model)
        {
            var dto = new ExpenseDto();
            var amount17 = model.Amount;
            dto.Amount = amount17;
            var budgetBucketCode18 = model.Bucket.Code;
            dto.BudgetBucketCode = budgetBucketCode18; 
            return dto;
        } // End ToDto Method
    } // End Class

    [GeneratedCode("1.0", "Tangy Fruit Mapper")]
    internal class Mapper_IncomeDto_Income : IDtoMapper<IncomeDto, Income>
    {
        private readonly IBudgetBucketRepository bucketRepo;

        public Mapper_IncomeDto_Income([NotNull] IBudgetBucketRepository bucketRepo)
        {
            if (bucketRepo == null) throw new ArgumentNullException(nameof(bucketRepo));
            this.bucketRepo = bucketRepo;
        }

        public Income ToModel(IncomeDto dto)
        {
            var model = new Income();
            var modelType = model.GetType();
            var amount19 = dto.Amount;
            model.Amount = amount19;
            var bucket20 = this.bucketRepo.GetByCode(dto.BudgetBucketCode);
            model.Bucket = bucket20; 
            return model;
        } // End ToModel Method

        public IncomeDto ToDto(Income model)
        {
            var dto = new IncomeDto();
            var amount22 = model.Amount;
            dto.Amount = amount22;
            var budgetBucketCode23 = model.Bucket.Code;
            dto.BudgetBucketCode = budgetBucketCode23;
            return dto;
        } // End ToDto Method
    } // End Class

} // End Namespace