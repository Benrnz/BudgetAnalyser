using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Uwp
{
    public class CompositionRoot
    {
        public ILogger Logger { get; set; }

        public ShellController ShellController { get; set; }

        public void ComposeAll()
        {
            var builder = new ContainerBuilder();
            var engineAssembly = typeof(StatementModel).GetTypeInfo().Assembly;
            var thisAssembly = GetType().GetTypeInfo().Assembly;
            ComposeTypesWithDefaultImplementations(engineAssembly, builder);
            ComposeTypesWithDefaultImplementations(thisAssembly, builder);

            IContainer container = BuildApplicationObjectGraph(builder, engineAssembly, thisAssembly);
        }

        private IContainer BuildApplicationObjectGraph(ContainerBuilder builder, params Assembly[] assemblies)
        {
            // Build Application Object Graph
            var container = builder.Build();
            Logger = container.Resolve<ILogger>();

            foreach (Assembly assembly in assemblies)
            {
                var requiredPropertyInjections = DefaultIoCRegistrations.ProcessPropertyInjection(assembly);
                foreach (PropertyInjectionDependencyRequirement requirement in requiredPropertyInjections)
                {
                    // Some reasonably awkard Autofac usage here to allow testibility.  (Extension methods aren't easy to test)
                    IComponentRegistration registration;
                    bool success = container.ComponentRegistry.TryGetRegistration(new TypedService(requirement.DependencyRequired), out registration);
                    if (success)
                    {
                        object dependency = container.ResolveComponent(registration, Enumerable.Empty<Parameter>());
                        requirement.PropertyInjectionAssignment(dependency); 
                    }
                }
            }

            // ConstructUiContext(container);

            ShellController = container.Resolve<ShellController>();

            return container;
        }

        private static void ComposeTypesWithDefaultImplementations(Assembly assembly, ContainerBuilder builder)
        {
            var dependencies = DefaultIoCRegistrations.RegisterAutoMappingsFromAssembly(assembly);
            foreach (var dependency in dependencies)
            {
                IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> registration;
                if (dependency.IsSingleInstance)
                {
                    // Singleton
                    registration = builder.RegisterType(dependency.DependencyRequired).SingleInstance();
                }
                else
                {
                    // Transient
                    registration = builder.RegisterType(dependency.DependencyRequired).InstancePerDependency();
                }

                if (!string.IsNullOrWhiteSpace(dependency.NamedInstanceName))
                {
                    // Named Dependency
                    registration = registration.Named(dependency.NamedInstanceName, dependency.DependencyRequired);
                }

                registration.AsImplementedInterfaces().AsSelf();

                // Register as custom type, other than its own class name, and directly implemented interfaces.
                if (dependency.AdditionalRegistrationType != null)
                {
                    registration.As(dependency.AdditionalRegistrationType);
                }
            }
        }
    }
}
