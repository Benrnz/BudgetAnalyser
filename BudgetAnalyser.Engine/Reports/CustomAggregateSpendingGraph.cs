using System;
using System.Collections.Generic;

namespace BudgetAnalyser.Engine.Reports
{
    public class CustomAggregateSpendingGraph
    {
        public List<Guid> BucketIds { get; set; }

        public string Name { get; set; }
    }
}
