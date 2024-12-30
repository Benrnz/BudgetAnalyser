using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Services;

namespace BudgetAnalyser.Engine.UnitTest.TestHarness
{
    internal class FakeMonitorableDependencies : MonitorableDependencies
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MonitorableDependencies"/> class.
        /// </summary>
        public FakeMonitorableDependencies() : base(new LedgerCalculation(new FakeLogger()), new FakeLogger())
        {
        }

        /// <summary>
        /// Notifies this service of dependency that has changed.
        /// </summary>
        /// <param name="dependency">The dependency.</param>
        /// <returns>A boolean value indicating if the dependency has significantly change, true if so, otherwise false.</returns>
        internal override bool NotifyOfDependencyChange<T>(T dependency)
        {
            return true;
        }

        /// <summary>
        /// Notifies this service of dependency that has changed.
        /// </summary>
        /// <param name="dependency">The dependency.</param>
        /// <param name="typeKey">The type key.</param>
        /// <returns>A boolean value indicating if the dependency has significantly change, true if so, otherwise false.</returns>
        internal override bool NotifyOfDependencyChange(object dependency, Type typeKey)
        {
            return true;
        }

        internal override object RetrieveDependency(Type key)
        {
            return null;
        }

        /// <summary>
        /// Gets a list of supported types
        /// </summary>
        internal override IEnumerable<Type> SupportedWidgetDependencyTypes
        {
            get
            {
                return new List<Type>();
            }
        }
    }
}