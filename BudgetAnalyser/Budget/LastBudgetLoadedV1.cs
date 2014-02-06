using Rees.Wpf;

namespace BudgetAnalyser.Budget
{
    public class LastBudgetLoadedV1 : IPersistent
    {
        public object Model { get; set; }

        public T AdaptModel<T>()
        {
            return (T)Model;
        }
    }
}