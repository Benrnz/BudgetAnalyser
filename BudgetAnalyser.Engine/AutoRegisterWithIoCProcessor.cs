using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Builder;

namespace BudgetAnalyser.Engine
{
    public static class AutoRegisterWithIoCProcessor
    {
        public static void RegisterAutoMappingsFromAssembly(ContainerBuilder builder, Assembly assembly)
        {
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