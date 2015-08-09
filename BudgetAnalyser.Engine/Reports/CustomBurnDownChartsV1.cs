using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser.Engine.Reports
{
    public class CustomBurnDownChartsV1 : IPersistent
    {
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Required for serialisation")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Required for serialisation")]
        public List<CustomAggregateBurnDownGraph> Charts { get; set; }

        public int LoadSequence
        {
            get { return 100; }
        }
    }
}