using System;
using System.Diagnostics;

namespace BudgetAnalyser.Engine
{
    /// <summary>
    ///     Specifies a required IoC registration that is needed for the application to function. This class is used to return
    ///     enough information back to the consuming UI code so it can register the dependency in its IoC container.
    ///     This class is IoC container independent.
    ///     By default all registrations should be self registered to its own class name as well as all directly implemented
    ///     interfaces.
    /// </summary>
    [DebuggerDisplay("DependencyRegistrationRequirement: DependencyReqd:{DependencyRequired} Named:{NamedInstanceName}")]
    public class DependencyRegistrationRequirement
    {
        /// <summary>
        ///     Get or sets an optional value to register the <see cref="DependencyRequired" /> against an additional type.
        ///     For example a super-class or interface.
        /// </summary>
        public Type AdditionalRegistrationType { get; set; }

        /// <summary>
        ///     Get or set a type describing the Type of the dependency required.
        /// </summary>
        public Type DependencyRequired { get; set; }

        /// <summary>
        ///     Get or sets a value indicating if the dependency instance needs to be a singleton.
        ///     If false, register as a transient, or per-call.
        ///     Intentionally not called Singleton, as it is possible to use more than one, but only one per IoC container.
        /// </summary>
        public bool IsSingleInstance { get; set; }

        /// <summary>
        ///     Get or sets an optional value to register under a specific name.
        ///     If null, it can be registered to its type and implemented interfaces.
        /// </summary>
        public string NamedInstanceName { get; set; }
    }
}