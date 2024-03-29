﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BudgetAnalyser.Annotations;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using Rees.Wpf.Contracts;
using Rees.Wpf;

namespace BudgetAnalyser.ReportsCatalog.LongTermSpendingLineGraph
{
    public sealed class LongTermSpendingGraphController : ControllerBase
    {
        private readonly ILongTermSpendingChartService chartService;
        private readonly IUserMessageBox messageService;
        private decimal doNotUseGraphMaximumValue;
        private decimal doNotUseGraphMinimumValue;
        private DatedGraphPlot doNotUseSelectedPlotPoint;
        private SeriesData doNotUseSelectedSeriesData;
        private bool doNotUseToggleAll;

        public LongTermSpendingGraphController([NotNull] ILongTermSpendingChartService chartService, IUserMessageBox messageService)
        {
            if (chartService == null)
            {
                throw new ArgumentNullException(nameof(chartService));
            }

            if (messageService == null) throw new ArgumentNullException(nameof(messageService));

            this.chartService = chartService;
            this.messageService = messageService;
            ToggleAll = true;
        }

        public GraphData Graph { get; private set; }

        public decimal GraphMaximumValue
        {
            [UsedImplicitly] get { return this.doNotUseGraphMaximumValue; }
            set
            {
                this.doNotUseGraphMaximumValue = value;
                RaisePropertyChanged();
            }
        }

        public decimal GraphMinimumValue
        {
            [UsedImplicitly] get { return this.doNotUseGraphMinimumValue; }
            set
            {
                this.doNotUseGraphMinimumValue = value;
                RaisePropertyChanged();
            }
        }

        public DatedGraphPlot SelectedPlotPoint
        {
            [UsedImplicitly] get { return this.doNotUseSelectedPlotPoint; }
            set
            {
                this.doNotUseSelectedPlotPoint = value;
                RaisePropertyChanged();
            }
        }

        public SeriesData SelectedSeriesData
        {
            [UsedImplicitly] get { return this.doNotUseSelectedSeriesData; }
            set
            {
                this.doNotUseSelectedSeriesData = value;
                RaisePropertyChanged();
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for data binding")]
        [UsedImplicitly]
        public string Title => "Long Term Spending Line Graph";

        public bool ToggleAll
        {
            get { return this.doNotUseToggleAll; }
            set
            {
                this.doNotUseToggleAll = value;
                RaisePropertyChanged();
                ToggleAllLinesVisibility();
            }
        }

        public void Load(StatementModel statementModel, GlobalFilterCriteria criteria)
        {
            try
            {
                Graph = this.chartService.BuildChart(statementModel, criteria);
                GraphMaximumValue = Graph.Series.Max(s => s.Plots.Max(p => p.Amount));
                GraphMinimumValue = Graph.Series.Min(s => s.MinimumValue);
            }
            catch (ArgumentException ex)
            {
                Graph = null;
                this.messageService.Show($"Unable to compile data for this report.\n{ex.Message}", "Data Validation");
            }
        }

        public void NotifyOfClose()
        {
            ToggleAll = true;
        }

        private void ToggleAllLinesVisibility()
        {
            Graph?.Series.ToList().ForEach(s => s.Visible = ToggleAll);
        }
    }
}