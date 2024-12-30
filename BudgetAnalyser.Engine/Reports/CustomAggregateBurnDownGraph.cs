using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Reports
{
    /// <summary>
    ///     A data storage to store a burn down graph that combines multiple buckets.
    /// </summary>
    public class CustomAggregateBurnDownGraph
    {
        /// <summary>
        ///     Gets or sets the bucket codes.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "Required for serialisation")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists",
            Justification = "Required for serialisation")]
        public List<string> BucketIds { get; set; }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        public string Name { get; set; }
    }
}
