using Rees.Wpf;

namespace BudgetAnalyser.BurnDownGraphs
{
    public class CustomBurnDownChartsV1 : IPersistent
    {
        public object Model { get; set; }

        public T AdaptModel<T>()
        {
            return (T)Model;
        }
    }
}