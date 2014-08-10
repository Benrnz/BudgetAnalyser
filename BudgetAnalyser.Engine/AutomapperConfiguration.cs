using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine
{
    /// <summary>
    /// A class to run all AutoMapper configuration in this assembly.  Any class that implements <see cref="ILocalAutoMapperConfiguration"/> will be dependency injected into this class 
    /// and all <see cref="ILocalAutoMapperConfiguration.RegisterMappings"/> methods on each are called.  The IoC container takes care of finding all the instances that implement the interface.
    /// </summary>
    public class AutoMapperConfiguration
    {
        private readonly IEnumerable<ILocalAutoMapperConfiguration> autoMapperRegistrations;

        public AutoMapperConfiguration([NotNull] IEnumerable<ILocalAutoMapperConfiguration> autoMapperRegistrations)
        {
            if (autoMapperRegistrations == null)
            {
                throw new ArgumentNullException("autoMapperRegistrations");
            }

            this.autoMapperRegistrations = autoMapperRegistrations;
        }

        internal IEnumerable<ILocalAutoMapperConfiguration> Registrations
        {
            get { return this.autoMapperRegistrations; }
        }

        /// <summary>
        /// Register all configurations on all known instances. This only needs to be done once during application startup.
        /// </summary>
        public AutoMapperConfiguration Configure()
        {
            // Warning! Use of this static mapping configuration has a chance of intermittently failing parallel test runs, (which they are in this project).
            // This may need to change to improve consistency of test results. Failure does seem to be very rare however.
            // In addition use of dependencies within the mapping effectively makes the dependencies "static-shared" as well, making it difficult to mock these
            // dependencies.  Will probably work ok if testing of mappers are isolated properly.  Ie, no integration tests that consume real mappers.

            foreach (var configuration in autoMapperRegistrations)
            {
                configuration.RegisterMappings();
            }

            return this;
        }
    }
}