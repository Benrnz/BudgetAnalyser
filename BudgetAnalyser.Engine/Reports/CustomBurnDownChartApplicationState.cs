using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.Reports
{
    /// <summary>
    ///     An application state Dto to persist custom burn down chart user preferences.
    /// </summary>
    /// <seealso cref="IPersistentApplicationStateObject" />
    public class CustomBurnDownChartApplicationState : IPersistentApplicationStateObject
    {
        // TODO These custom burn down charts should probably be saved with Application model data rather than state data.
        /// <summary>
        ///     Gets or sets the charts.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Required for serialisation")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Required for serialisation")]
        public List<CustomAggregateBurnDownGraph> Charts { get; set; }

        /// <summary>
        ///     Gets the order in which this object should be loaded.
        /// </summary>
        public int LoadSequence => 100;
    }
}