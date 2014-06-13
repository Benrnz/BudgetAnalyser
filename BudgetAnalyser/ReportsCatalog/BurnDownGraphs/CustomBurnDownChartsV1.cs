using Rees.Wpf;

namespace BudgetAnalyser.ReportsCatalog.BurnDownGraphs
{
    public class CustomBurnDownChartsV1 : IPersistent
    {
        public object Model { get; set; }

        public int Sequence { get { return 100; } }

        public T AdaptModel<T>()
        {
            return (T)this.Model;
        }
    }
}