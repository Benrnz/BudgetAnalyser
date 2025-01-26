namespace BudgetAnalyser.Engine.Budget.Data;

[AutoRegisterWithIoC]
internal partial class MapperBudgetModelDtoBudgetModel(IBudgetBucketRepository bucketRepo)
{
    partial void ToDtoPostprocessing(ref BudgetModelDto dto, BudgetModel model)
    {
        var mapper3 = new MapperExpenseDto2Expense(bucketRepo);
        var expenses8 = model.Expenses.Select(mapper3.ToDto).ToList();
        dto.Expenses = expenses8;
        var mapper4 = new MapperIncomeDto2Income(bucketRepo);
        var incomes9 = model.Incomes.Select(mapper4.ToDto).ToList();
        dto.Incomes = incomes9;
    }

    partial void ToModelPostprocessing(BudgetModelDto dto, ref BudgetModel model)
    {
        var modelType = model.GetType();
        var mapper1 = new MapperExpenseDto2Expense(bucketRepo);
        var expenses2 = dto.Expenses.Select(mapper1.ToModel).ToList();
        model.LastModified = dto.LastModified ?? DateTime.Now;
        modelType.GetProperty("Expenses")!.SetValue(model, expenses2);
        var mapper2 = new MapperIncomeDto2Income(bucketRepo);
        var incomes3 = dto.Incomes.Select(mapper2.ToModel).ToList();
        modelType.GetProperty("Incomes")!.SetValue(model, incomes3);
    }
}

internal partial class MapperExpenseDto2Expense
{
    partial void ModelFactory(ExpenseDto dto, ref Expense model)
    {
        var bucket = this.bucketRepo.GetByCode(dto.BudgetBucketCode) ?? throw new DataFormatException($"Budget Bucket {dto.BudgetBucketCode} not found in budget model.");
        model = new Expense { Bucket = bucket };
    }
}

internal partial class MapperIncomeDto2Income
{
    partial void ModelFactory(IncomeDto dto, ref Income model)
    {
        var bucket = this.bucketRepo.GetByCode(dto.BudgetBucketCode) ?? throw new DataFormatException($"Budget Bucket {dto.BudgetBucketCode} not found in budget model.");
        model = new Income { Bucket = bucket };
    }
}
