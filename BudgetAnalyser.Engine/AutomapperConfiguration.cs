using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine
{
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

        public AutoMapperConfiguration Configure()
        {
            // Warning! Use of this static mapping configuration has a chance of intermittently failing tests, if the tests are run in parallel (which they are in this project).
            // This may need to change to improve consistency of test results. Failure does seem to be very rare however.

            foreach (var configuration in autoMapperRegistrations)
            {
                configuration.RegisterMappings();
            }

            return this;
        }
    }
}