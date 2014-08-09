using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Budget
{
    public interface IBudgetRepository
    {
        IBudgetBucketRepository BudgetBucketRepository { get; }

        BudgetCollection CreateNew(string fileName);

        BudgetCollection Load(string fileName);

        void Save(BudgetCollection budget);
    }
}