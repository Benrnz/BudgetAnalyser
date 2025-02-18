using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.Budget.Data;

[AutoRegisterWithIoC]
public class MapperExpenseToDto2(IBudgetBucketRepository bucketRepo) : IDtoMapper<ExpenseDto, Expense>
{
    private readonly IBudgetBucketRepository bucketRepo = bucketRepo ?? throw new ArgumentNullException(nameof(bucketRepo));

    public ExpenseDto ToDto(Expense model)
    {
        return new ExpenseDto { Amount = model.Amount, BudgetBucketCode = model.Bucket.Code };
    }

    public Expense ToModel(ExpenseDto dto)
    {
        return new Expense
        {
            Bucket = this.bucketRepo.GetByCode(dto.BudgetBucketCode) ?? throw new DataFormatException($"Invalid Expense Budget Bucket '{dto.BudgetBucketCode}' found in BudgetModel file."),
            Amount = dto.Amount
        };
    }
}
