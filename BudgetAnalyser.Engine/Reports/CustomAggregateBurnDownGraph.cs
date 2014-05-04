using System.Collections.Generic;

namespace BudgetAnalyser.Engine.Reports
{
    public class CustomAggregateBurnDownGraph
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification="Required for serialisation")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Required for serialisation")]
        public List<string> BucketIds { get; set; }

        public string Name { get; set; }
    }
}