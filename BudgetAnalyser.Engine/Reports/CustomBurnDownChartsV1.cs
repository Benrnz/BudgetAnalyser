using System.Collections.Generic;
using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser.Engine.Reports
{
    public class CustomBurnDownChartsV1 : IPersistent
    {
        public List<CustomAggregateBurnDownGraph> Charts { get; set; }

        public int LoadSequence { get { return 100; } }
    }
}