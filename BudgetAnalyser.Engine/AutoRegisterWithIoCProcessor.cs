using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Builder;
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
            }
        }
    }
}