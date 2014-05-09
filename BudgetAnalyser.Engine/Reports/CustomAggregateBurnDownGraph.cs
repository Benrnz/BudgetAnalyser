using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Reports
{
    public class CustomAggregateBurnDownGraph
    {
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Required for serialisation")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Required for serialisation")]
        public List<string> BucketIds { get; set; }

        public string Name { get; set; }
    }
}