using Rees.Wpf;

namespace BudgetAnalyser.Statement
{
    public class LastStatementLoadedV1 : IPersistent
    {
        public object Model { get; set; }
        public int Sequence { get { return 20; } }

        public T AdaptModel<T>()
        {
            return (T)Model;
        }
    }
}