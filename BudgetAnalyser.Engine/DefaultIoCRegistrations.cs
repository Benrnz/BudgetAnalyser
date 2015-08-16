using System.Reflection;
using Autofac;
using Autofac.Features.ResolveAnything;

namespace BudgetAnalyser.Engine
{
    /// <summary>
    ///     A class to record all default mappings for IoC in this assembly.
    /// </summary>
    public static class DefaultIoCRegistrations
    {
        /// <summary>
        ///     Register all IoC mappings in this assembly.
        /// </summary>
        /// <param name="builder">
        ///     Autofac's builder object. AUTOFAC IS ISOLATED TO THIS CLASS AND METHOD ONLY, DONT USE IT ANYWHERE
        ///     ELSE.
        /// </param>
        public static void RegisterDefaultMappings(ContainerBuilder builder)
        {
            Assembly thisAssembly = typeof(DefaultIoCRegistrations).Assembly;
            AutoRegisterWithIoCProcessor.RegisterAutoMappingsFromAssembly(builder, thisAssembly);

            // Everything else
            builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
        }
    }
}