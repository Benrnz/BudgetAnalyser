using Rees.Wpf;

namespace BudgetAnalyser.Dashboard
{
    public class DashboardApplicationStateV1 : IPersistent
    {
        public object Model { get; set; }

        public int Sequence
        {
            get { return 100; }
        }

        public T AdaptModel<T>()
        {
            return (T)Model;
        }
    }
}