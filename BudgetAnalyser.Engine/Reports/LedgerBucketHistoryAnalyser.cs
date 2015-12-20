using System;
using System.Linq;
using BudgetAnalyser.Engine.Ledger;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Reports
{
    internal class LedgerBucketHistoryAnalyser
    {
        [UsedImplicitly]
        public SeriesData BalanceLine => GraphData.Series.FirstOrDefault();

        public GraphData GraphData { get; private set; }

        public void Analyse([NotNull] LedgerBucket ledger, [NotNull] LedgerBook book)
        {
            if (ledger == null)
            {
                throw new ArgumentNullException(nameof(ledger));
            }

            if (book == null)
            {
                throw new ArgumentNullException(nameof(book));
            }

            GraphData = new GraphData
            {
                GraphName = "Bucket Balance History"
            };

            var series = new SeriesData
            {
                Description = "The actual bank balance of the Ledger Bucket over time.",
                SeriesName = "Balance"
            };
            GraphData.SeriesList.Add(series);

            foreach (var reconciliation in book.Reconciliations.Take(24).Reverse())
            {
                var entry = reconciliation.Entries.FirstOrDefault(e => e.LedgerBucket == ledger);
                var plot = new DatedGraphPlot
                {
                    Date = reconciliation.Date,
                    Amount = entry?.Balance ?? 0
                };
                series.PlotsList.Add(plot);
            }
        }
    }
}