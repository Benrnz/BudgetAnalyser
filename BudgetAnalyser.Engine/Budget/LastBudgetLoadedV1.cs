using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser.Engine.Budget
{
    public class LastBudgetLoadedV1 : IPersistent
    {
        public string BudgetCollectionStorageKey { get; set; }
        public int LoadSequence { get { return 10; } }
    }
}