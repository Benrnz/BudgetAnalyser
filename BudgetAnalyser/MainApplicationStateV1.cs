using Rees.Wpf;

namespace BudgetAnalyser
{
    public class MainApplicationStateV1 : IPersistent
    {
        public object Model { get; set; }

        public int Sequence
        {
            get { return 1; }
        }

        public T AdaptModel<T>()
        {
            return (T)Model;
        }
    }
}