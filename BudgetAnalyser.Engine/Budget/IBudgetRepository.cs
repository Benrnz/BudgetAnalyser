namespace BudgetAnalyser.Engine.Budget
{
    public interface IBudgetRepository
    {
        IBudgetBucketRepository BudgetBucketRepository { get; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification="Custom collection")]
        BudgetCollection CreateNew(string fileName);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Custom collection")]
        BudgetCollection Load(string fileName);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Custom collection")]
        void Save(BudgetCollection budget);
    }
}