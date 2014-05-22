using Rees.Wpf;

namespace BudgetAnalyser.Budget
{
    public class LastBudgetLoadedV1 : IPersistent
    {
        public object Model { get; set; }
        public int Sequence { get { return 10; } }

        public T AdaptModel<T>()
        {
            return (T)Model;
        }
    }
}