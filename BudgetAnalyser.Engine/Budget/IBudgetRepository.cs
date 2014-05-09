using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Budget
{
    public interface IBudgetRepository
    {
        IBudgetBucketRepository BudgetBucketRepository { get; }

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Custom collection")]
        BudgetCollection CreateNew(string fileName);

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Custom collection")]
        BudgetCollection Load(string fileName);

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Custom collection")]
        void Save(BudgetCollection budget);
    }
}