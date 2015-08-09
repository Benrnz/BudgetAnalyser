using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine
{
    /// <summary>
    ///     An intialisation class to discover all automatic registrations in the specified assembly and register them with
    ///     Autofac.
    /// </summary>
    public static class AutoRegisterWithIoCProcessor
    {
        /// <summary>
        ///     Enumerates through all static types in the given assembly and populates static properties decorated with
        ///     <see cref="PropertyInjectionAttribute" /> with their instances
        ///     from the container.
        ///     DO NOT USE THIS. Except as a last resort, property injection is a bad pattern, but is sometimes required with UI
        ///     bindings. Do not use it for any other reason.
        /// </summary>
        /// <param name="container">
        ///     The populated and ready for use IoC container. It will be used to populate properties with
        ///     their dependency instances.
        /// </param>
        /// <param name="assembly">The assembly in which to search for automatic registrations.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void ProcessPropertyInjection([NotNull] IComponentContext container, [NotNull] Assembly assembly)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            Type[] allTypes = assembly.GetTypes()
                .Where(t => t.IsClass && t.IsAbstract && t.IsSealed && t.GetCustomAttribute<AutoRegisterWithIoCAttribute>() != null)
                .ToArray();
            foreach (Type type in allTypes)
            {
                foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Static))
                {
                    var injectionAttribute = property.GetCustomAttribute<PropertyInjectionAttribute>();
                    if (injectionAttribute != null)
                    {
                        // Some reasonably awkard Autofac usage here to allow testibility.  (Extension methods aren't easy to test)
                        IComponentRegistration registration;
                        bool success = container.ComponentRegistry.TryGetRegistration(new TypedService(property.PropertyType), out registration);
                        if (success)
                        {
                            object dependency = container.ResolveComponent(registration, Enumerable.Empty<Parameter>());
                            property.SetValue(null, dependency);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Enumerates through all types in the given assembly and registers those decorated with
        ///     <see cref="AutoRegisterWithIoCAttribute" /> with Autofac.
        ///     See the attribute for registration options. No dependent assemblies are searched.
        /// </summary>
        /// <param name="builder">The Autofac container builder object used to register type mappings.</param>
        /// <param name="assembly">The assembly in which to search for automatic registrations.</param>
        public static void RegisterAutoMappingsFromAssembly([NotNull] ContainerBuilder builder, [NotNull] Assembly assembly)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            Type[] allTypes = assembly.GetTypes()
                .Where(t => !t.IsAbstract && t.IsClass && t.GetCustomAttribute<AutoRegisterWithIoCAttribute>() != null)
                .ToArray();

            foreach (Type type in allTypes)
            {
                var autoRegisterAttribute = type.GetCustomAttribute<AutoRegisterWithIoCAttribute>();
                IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> registration;
                if (autoRegisterAttribute.SingleInstance)
                {
                    registration = builder.RegisterType(type).SingleInstance();
                }
                else
                {
                    registration = builder.RegisterType(type).InstancePerDependency();
                }

                if (!string.IsNullOrWhiteSpace(autoRegisterAttribute.Named))
                {
                    registration = registration.Named(autoRegisterAttribute.Named, type);
                }

                registration.AsImplementedInterfaces().AsSelf();

                if (autoRegisterAttribute.RegisterAs != null)
                {
                    registration.As(autoRegisterAttribute.RegisterAs);
                }
            }
        }
    }
}