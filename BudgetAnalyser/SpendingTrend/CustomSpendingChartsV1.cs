using Rees.Wpf;

namespace BudgetAnalyser.SpendingTrend
{
    public class CustomSpendingChartsV1 : IPersistent
    {
        public object Model { get; set; }

        public T AdaptModel<T>()
        {
            return (T) Model;
        }
    }
}