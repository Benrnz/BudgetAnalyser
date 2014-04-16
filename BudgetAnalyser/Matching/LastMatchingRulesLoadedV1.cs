using Rees.Wpf;

namespace BudgetAnalyser.Matching
{
    public class LastMatchingRulesLoadedV1 : IPersistent
    {
        public object Model { get; set; }

        public T AdaptModel<T>()
        {
            return (T)this.Model;
        }
    }
}