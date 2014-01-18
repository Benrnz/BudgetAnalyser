using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook
{
    public class LastLedgerBookLoadedV1 : IPersistent
    {
        public object Model { get; set; }

        public T AdaptModel<T>()
        {
            return (T) Model;
        }
    }
}