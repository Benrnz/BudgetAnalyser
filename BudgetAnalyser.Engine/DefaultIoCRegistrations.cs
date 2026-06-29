using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace BudgetAnalyser.Engine;

/// <summary>
///     An initialisation class to discover all default IoC registrations required in the specified assembly.
/// </summary>
public static class DefaultIoCRegistrations
{
    /// <summary>
    ///     Registers all types decorated with <see cref="AutoRegisterWithIoCAttribute" /> found in the given assembly into
    ///     the service collection. Each type is registered as itself and as each of its directly implemented interfaces.
    ///     <see cref="AutoRegisterWithIoCAttribute.SingleInstance" /> maps to singleton lifetime; the default is transient.
    ///     Named registrations (via <see cref="AutoRegisterWithIoCAttribute.Named" />) are also registered against the
    ///     implemented interfaces so they are included when <see cref="IEnumerable{T}" /> is injected; the correct instance
    ///     is then selected at runtime via <see cref="GetNamedInstance{T}" />.
    /// </summary>
    /// <param name="services">The service collection to register types into.</param>
    /// <param name="assembly">The assembly to scan for <see cref="AutoRegisterWithIoCAttribute" /> decorated types.</param>
    /// <returns>The same <see cref="IServiceCollection" /> to enable method chaining.</returns>
    public static IServiceCollection AddAutoRegistrations(this IServiceCollection services, Assembly assembly)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        var dependencies = RegisterAutoMappingsFromAssembly(assembly);
        foreach (var dependency in dependencies)
        {
            var type = dependency.Type;
            var interfaces = type.GetInterfaces();

            if (dependency.IsSingleInstance)
            {
                // Register as self so it can be resolved directly by concrete type.
                services.AddSingleton(type);

                // Register each implemented interface pointing to the self-registration factory so all
                // interface resolutions share the same singleton instance.
                foreach (var iface in interfaces)
                {
                    services.AddSingleton(iface, sp => sp.GetRequiredService(type));
                }
            }
            else
            {
                services.AddTransient(type);
                foreach (var iface in interfaces)
                {
                    services.AddTransient(iface, sp => sp.GetRequiredService(type));
                }
            }
        }

        return services;
    }

    /// <summary>
    ///     Registers all types decorated with <see cref="AutoRegisterWithIoCAttribute" /> in the Engine assembly with the
    ///     given service collection. Follows the Microsoft DI extension method pattern.
    /// </summary>
    /// <param name="services">The service collection to register engine types into.</param>
    /// <returns>The same <see cref="IServiceCollection" /> to enable method chaining.</returns>
    public static IServiceCollection AddEngineRegistrations(this IServiceCollection services)
    {
        return services.AddAutoRegistrations(typeof(DefaultIoCRegistrations).Assembly);
    }

    /// <summary>
    ///     Gets a named instance from all the given instances. This is used to find an instance of an interface with a specific name.
    /// </summary>
    /// <typeparam name="T">Any interface or abstract class</typeparam>
    /// <returns>The found named instance, or null if no instance matches the name.</returns>
    public static T GetNamedInstance<T>(IEnumerable<T> instances, string name) where T : class
    {
        if (instances is null)
        {
            throw new ArgumentNullException(nameof(instances));
        }

        foreach (var instance in instances)
        {
            var attributes = instance.GetType().GetTypeInfo().GetCustomAttributes<AutoRegisterWithIoCAttribute>();
            var attribute = attributes.FirstOrDefault();
            if (attribute is not null)
            {
                if (attribute.Named == name)
                {
                    return instance;
                }
            }
        }

        throw new NotSupportedException($"No instance found with the specified name '{name}'.");
    }

    /// <summary>
    ///     Enumerates through all static types in the given assembly and finds static properties decorated with <see cref="PropertyInjectionAttribute" />.
    ///     DO NOT USE THIS. Except as a last resort, property injection is a bad pattern, but is sometimes required for UI data bindings. Do not use it for any other reason.
    /// </summary>
    /// <param name="assembly">The assembly in which to search for automatic registrations.</param>
    public static IEnumerable<PropertyInjectionDependencyRequirement> ProcessPropertyInjection(Assembly assembly)
    {
        if (assembly is null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        var allTypes = assembly.GetTypes()
            .Where(t =>
            {
                var typeInfo = t.GetTypeInfo();
                return typeInfo is { IsClass: true, IsAbstract: true, IsSealed: true } && typeInfo.GetCustomAttribute<AutoRegisterWithIoCAttribute>() is not null;
            })
            .ToArray();
        foreach (var type in allTypes)
        {
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Static))
            {
                var injectionAttribute = property.GetCustomAttribute<PropertyInjectionAttribute>();
                if (injectionAttribute is not null)
                {
                    yield return new PropertyInjectionDependencyRequirement { Type = property.PropertyType, PropertyInjectionAssignment = instance => property.SetValue(null, instance) };
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
    public static IEnumerable<DependencyRegistrationRequirement> RegisterAutoMappingsFromAssembly(Assembly assembly)
    {
        if (assembly is null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        var allTypes = assembly.GetTypes()
            .Where(t =>
            {
                var typeInfo = t.GetTypeInfo();
                return typeInfo is { IsAbstract: false, IsClass: true } && typeInfo.GetCustomAttribute<AutoRegisterWithIoCAttribute>() is not null;
            })
            .ToArray();

        return from type in allTypes
               let autoRegisterAttribute = type.GetTypeInfo().GetCustomAttribute<AutoRegisterWithIoCAttribute>()
               select new DependencyRegistrationRequirement { Type = type, IsSingleInstance = autoRegisterAttribute.SingleInstance, NamedInstanceName = autoRegisterAttribute.Named };
    }
}
