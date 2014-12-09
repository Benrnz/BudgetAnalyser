using BudgetAnalyser.Engine.Statement;
using Rees.Wpf;

namespace BudgetAnalyser.Statement
{
    public class StatementApplicationStateV1 : IPersistent
    {
        public object Model { get; set; }
        public int Sequence { get { return 20; } }

        public StatementApplicationState StatementApplicationState
        {
            get { return AdaptModel<StatementApplicationState>(); }
            set { Model = value; }
        }

        public T AdaptModel<T>()
        {
            return (T)Model;
        }
    }
}