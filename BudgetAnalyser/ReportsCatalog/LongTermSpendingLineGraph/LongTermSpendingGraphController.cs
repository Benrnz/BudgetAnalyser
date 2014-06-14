using System;
using System.Linq;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Statement;
using Rees.Wpf;

namespace BudgetAnalyser.ReportsCatalog.LongTermSpendingLineGraph
{
    public class LongTermSpendingGraphController : ControllerBase
    {
        private readonly LongTermSpendingTrendAnalyser analyser;
        private readonly IBudgetBucketRepository bucketRepository;
        private bool doNotUseToggleAll;
        private decimal doNotUseGraphMinimumValue;
        private decimal doNotUseGraphMaximumValue;
        // private bool active;

        // public event EventHandler ChartUpdateRequired;

        public LongTermSpendingGraphController([NotNull] LongTermSpendingTrendAnalyser analyser, [NotNull] IBudgetBucketRepository bucketRepository)
        {
            if (analyser == null)
            {
                throw new ArgumentNullException("analyser");
            }

            if (bucketRepository == null)
            {
                throw new ArgumentNullException("bucketRepository");
            }

            this.analyser = analyser;
            this.bucketRepository = bucketRepository;
        }

        public GraphData Graph
        {
            get { return this.analyser.Graph; }
        }

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

        public void Load(StatementModel statementModel, GlobalFilterCriteria criteria)
        {
            this.analyser.Analyse(statementModel, criteria);
            GraphMaximumValue = Graph.Series.Max(s => s.Plots.Max(p => p.Amount));
            GraphMinimumValue = Graph.Series.Min(s => s.MinimumValue);
        }

        public void NotifyOfClose()
        {
            // this.active = false;
            this.analyser.Reset();
        }

        private void ToggleAllLinesVisibility()
        {
            Graph.Series.ToList().ForEach(s => s.Visible = ToggleAll);
        }
    }
}