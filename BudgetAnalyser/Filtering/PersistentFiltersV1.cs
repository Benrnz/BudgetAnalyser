using Rees.Wpf;

namespace BudgetAnalyser.Filtering
{
    public class PersistentFiltersV1 : IPersistent
    {
        public object Model { get; set; }

        public int Sequence { get { return 50; } }

        public T AdaptModel<T>()
        {
            return (T)Model;
        }
    }
}