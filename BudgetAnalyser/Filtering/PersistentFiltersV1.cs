using Rees.Wpf;

namespace BudgetAnalyser.Filtering
{
    public class PersistentFiltersV1 : IPersistent
    {
        public object Model { get; set; }

        public T AdaptModel<T>()
        {
            return (T)Model;
        }
    }
}