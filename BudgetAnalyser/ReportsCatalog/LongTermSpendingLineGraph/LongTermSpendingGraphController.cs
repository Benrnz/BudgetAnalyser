using System;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Statement;
using Rees.Wpf;

namespace BudgetAnalyser.ReportsCatalog.LongTermSpendingLineGraph
{
    public class LongTermSpendingGraphController : ControllerBase
    {
        private readonly LongTermSpendingTrendAnalyser analyser;

        public LongTermSpendingGraphController([NotNull] LongTermSpendingTrendAnalyser analyser)
        {
            if (analyser == null)
            {
                throw new ArgumentNullException("analyser");
            }

            this.analyser = analyser;
        }

        public GraphData Graph
        {
            get { return this.analyser.Graph; }
        }

        public string Title
        {
            get { return "Long Term Spending Line Graph"; }
        }

        public void Load(StatementModel statementModel, GlobalFilterCriteria criteria)
        {
            this.analyser.Analyse(statementModel, criteria);
            RaisePropertyChanged(() => Graph);
        }
    }
}
