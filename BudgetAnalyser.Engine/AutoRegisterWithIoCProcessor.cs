using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine
{
    public static class AutoRegisterWithIoCProcessor
    {
        public static void RegisterAutoMappingsFromAssembly([NotNull] ContainerBuilder builder, [NotNull] Assembly assembly)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
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

        public static void ProcessPropertyInjection([NotNull] IComponentContext container, [NotNull] Assembly assembly)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }

            Type[] allTypes = assembly.GetTypes()
                .Where(t => t.IsClass && t.IsAbstract && t.IsSealed && t.GetCustomAttribute<AutoRegisterWithIoCAttribute>() != null)
                .ToArray();
            foreach (Type type in allTypes)
            {
                foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Static))
                {
                    var injectionAttribute = property.GetCustomAttribute<PropertyInjectionAttribute>();
                    if (injectionAttribute != null)
                    {
                        // Some reasonably awkard Autofac usage here to allow testibility.  (Extension methods aren't easy to test)
                        IComponentRegistration registration;
                        var success = container.ComponentRegistry.TryGetRegistration(new TypedService(property.PropertyType), out registration);
                        if (success)
                        {
                            var dependency = container.ResolveComponent(registration, Enumerable.Empty<Parameter>());
                            property.SetValue(null, dependency);
                        }
                    }
                }
            }
        }
    }
}