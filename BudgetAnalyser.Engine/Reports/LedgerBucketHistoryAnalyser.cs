using System.Linq;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Ledger;

namespace BudgetAnalyser.Engine.Reports
{
    public class LedgerBucketHistoryAnalyser
    {
        public GraphData GraphData { get; private set; }

        [UsedImplicitly]
        public SeriesData BalanceLine
        {
            get { return GraphData.Series.FirstOrDefault(); }
        }

        public void Analyse(LedgerBucket ledger, LedgerBook book)
        {
            GraphData = new GraphData
            {
                GraphName = "Bucket Balance History",
            };

            var series = new SeriesData { Description = "The actual bank balance of the Ledger Bucket over time.", SeriesName = "Balance" };
            GraphData.SeriesList.Add(series);

            foreach (var reconciliation in book.Reconciliations.Take(24).Reverse())
            {
                var entry = reconciliation.Entries.FirstOrDefault(e => e.LedgerBucket == ledger);
                var plot = new DatedGraphPlot
                {
                    Date = reconciliation.Date, 
                    Amount = entry == null ? 0 : entry.Balance,
                };
                series.PlotsList.Add(plot);
            }
        }
    }
}
