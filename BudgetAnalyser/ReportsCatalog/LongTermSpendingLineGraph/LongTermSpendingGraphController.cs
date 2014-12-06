using System;
using System.Linq;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using Rees.Wpf;

namespace BudgetAnalyser.ReportsCatalog.LongTermSpendingLineGraph
{
    public sealed class LongTermSpendingGraphController : ControllerBase
    {
        private readonly ILongTermSpendingChartService chartService;
        private decimal doNotUseGraphMaximumValue;
        private decimal doNotUseGraphMinimumValue;
        private bool doNotUseToggleAll;
        private SeriesData doNotUseSelectedSeriesData;
        private DatedGraphPlot doNotUseSelectedPlotPoint;

        public LongTermSpendingGraphController([NotNull] ILongTermSpendingChartService chartService)
        {
            if (chartService == null)
            {
                throw new ArgumentNullException("chartService");
            }
        
            this.chartService = chartService;
            ToggleAll = true;
        }

        public SeriesData SelectedSeriesData
        {
            get { return this.doNotUseSelectedSeriesData; }
            set
            {
                this.doNotUseSelectedSeriesData = value;
                RaisePropertyChanged(() => SelectedSeriesData);
            }
        }

        public DatedGraphPlot SelectedPlotPoint
        {
            get { return this.doNotUseSelectedPlotPoint; }
            set
            {
                this.doNotUseSelectedPlotPoint = value;
                RaisePropertyChanged(() => SelectedPlotPoint);
            }
        }

        public GraphData Graph { get; private set; }

        public decimal GraphMaximumValue
        {
            get { return this.doNotUseGraphMaximumValue; }
            set
            {
                this.doNotUseGraphMaximumValue = value;
                RaisePropertyChanged(() => GraphMaximumValue);
            }
        }

        public decimal GraphMinimumValue
        {
            get { return this.doNotUseGraphMinimumValue; }
            set
            {
                this.doNotUseGraphMinimumValue = value;
                RaisePropertyChanged(() => GraphMinimumValue);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for data binding")]
        public string Title
        {
            get { return "Long Term Spending Line Graph"; }
        }

        public bool ToggleAll
        {
            get { return this.doNotUseToggleAll; }
            set
            {
                this.doNotUseToggleAll = value;
                RaisePropertyChanged(() => ToggleAll);
                ToggleAllLinesVisibility();
            }
        }

        public void Load(StatementModel statementModel, GlobalFilterCriteria criteria)
        {
            Graph = this.chartService.BuildChart(statementModel, criteria);
            GraphMaximumValue = Graph.Series.Max(s => s.Plots.Max(p => p.Amount));
            GraphMinimumValue = Graph.Series.Min(s => s.MinimumValue);
        }

        public void NotifyOfClose()
        {
            ToggleAll = true;
        }

        private void ToggleAllLinesVisibility()
        {
            if (Graph == null)
            {
                return;
            }

            Graph.Series.ToList().ForEach(s => s.Visible = ToggleAll);
        }
    }
}