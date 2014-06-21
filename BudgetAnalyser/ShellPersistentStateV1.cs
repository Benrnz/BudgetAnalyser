using Rees.Wpf;

namespace BudgetAnalyser
{
    public class ShellPersistentStateV1 : IPersistent
    {
        public object Model { get; set; }

        public int Sequence
        {
            get { return 0; }
        }

        public T AdaptModel<T>()
        {
            return (T)Model;
        }
    }
}