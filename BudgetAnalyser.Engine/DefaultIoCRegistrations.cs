using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Features.ResolveAnything;

namespace BudgetAnalyser.Engine
{
    /// <summary>
    /// A class to record all default mappings for IoC in this assembly.
    /// </summary>
    public static class DefaultIoCRegistrations
    {
        /// <summary>
        /// Register IoC mappings
        /// </summary>
        /// <param name="builder">Autofac's builder object. AUTOFAC IS ISOLATED TO THIS CLASS AND METHOD ONLY.</param>
        public static void RegisterDefaultMappings(ContainerBuilder builder)
        {
            IoC.RegisterAutoMappingsFromAssembly(builder, typeof(DefaultIoCRegistrations).Assembly);

            // Everything else
            builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
        }
    }
}
