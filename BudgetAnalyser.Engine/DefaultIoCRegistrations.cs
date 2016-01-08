using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine
{
    /// <summary>
    ///     An intialisation class to discover all default IoC registrations required in the specified assembly.
    /// </summary>
    public static class DefaultIoCRegistrations
    {
        /// <summary>
        ///     Enumerates through all static types in the given assembly and finds static properties decorated with
        ///     <see cref="PropertyInjectionAttribute" />.
        ///     DO NOT USE THIS. Except as a last resort, property injection is a bad pattern, but is sometimes required for UI
        ///     data bindings. Do not use it for any other reason.
        /// </summary>
        /// <param name="assembly">The assembly in which to search for automatic registrations.</param>
        public static IEnumerable<PropertyInjectionDependencyRequirement> ProcessPropertyInjection([NotNull] Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            Type[] allTypes = assembly.GetTypes()
                .Where(t =>
                {
                    TypeInfo typeInfo = t.GetTypeInfo();
                    return typeInfo.IsClass && typeInfo.IsAbstract && typeInfo.IsSealed && typeInfo.GetCustomAttribute<AutoRegisterWithIoCAttribute>() != null;
                })
                .ToArray();
            foreach (Type type in allTypes)
            {
                foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Static))
                {
                    var injectionAttribute = property.GetCustomAttribute<PropertyInjectionAttribute>();
                    if (injectionAttribute != null)
                    {
                        yield return new PropertyInjectionDependencyRequirement
                        {
                            DependencyRequired = property.PropertyType,
                            PropertyInjectionAssignment = instance => property.SetValue(null, instance)
                        };
                        //// Some reasonably awkard Autofac usage here to allow testibility.  (Extension methods aren't easy to test)
                        //IComponentRegistration registration;
                        //bool success = container.ComponentRegistry.TryGetRegistration(new TypedService(property.PropertyType), out registration);
                        //if (success)
                        //{
                        //    object dependency = container.ResolveComponent(registration, Enumerable.Empty<Parameter>());
                        //    property.SetValue(null, dependency);
                        //}
                    }
                }
            }
        }

        /// <summary>
        ///     Finds all types that need to be registered with the hosting application's IoC container.  The host application
        ///     needs to register these with the container of its choosing.
        ///     See the attribute for registration options. No dependent assemblies are searched.
        /// </summary>
        /// <param name="assembly">The assembly in which to search for automatic registrations.</param>
        public static IEnumerable<DependencyRegistrationRequirement> RegisterAutoMappingsFromAssembly([NotNull] Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            Type[] allTypes = assembly.GetTypes()
                .Where(t =>
                {
                    TypeInfo typeInfo = t.GetTypeInfo();
                    return !typeInfo.IsAbstract && typeInfo.IsClass && typeInfo.GetCustomAttribute<AutoRegisterWithIoCAttribute>() != null;
                })
                .ToArray();

            return from type in allTypes
                   let autoRegisterAttribute = type.GetTypeInfo().GetCustomAttribute<AutoRegisterWithIoCAttribute>()
                   select new DependencyRegistrationRequirement
                   {
                       DependencyRequired = type,
                       IsSingleton = autoRegisterAttribute.SingleInstance,
                       NamedInstanceName = autoRegisterAttribute.Named,
                       AdditionalRegistrationType = autoRegisterAttribute.RegisterAs
                   };
            //IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> registration;
            //if (autoRegisterAttribute.SingleInstance)
            //{
            //    // Singleton
            //    registration = builder.RegisterType(type).SingleInstance();
            //}
            //else
            //{
            //    // Transient
            //    registration = builder.RegisterType(type).InstancePerDependency();
            //}

            //if (!string.IsNullOrWhiteSpace(autoRegisterAttribute.Named))
            //{
            //    // Named Dependency
            //    registration = registration.Named(autoRegisterAttribute.Named, type);
            //}

            //registration.AsImplementedInterfaces().AsSelf();

            //// Register as custom type, other than its own class name, and directly implemented interfaces.
            //if (autoRegisterAttribute.RegisterAs != null)
            //{
            //    registration.As(autoRegisterAttribute.RegisterAs);
            //}
        }
    }
}