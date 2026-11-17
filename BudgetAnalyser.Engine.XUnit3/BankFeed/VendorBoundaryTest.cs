using System.Reflection;
using BudgetAnalyser.Encryption;
using BudgetAnalyser.Engine.BankFeed;
using BudgetAnalyser.Engine.Transactions;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.BankFeed;

/// <summary>
///     Enforces that the application is not tied to one bank feed vendor. Vendor specific types may only exist inside the adapter namespace, must not be public, and must not be
///     used by any type outside that namespace. The WPF application cannot reference them because they are internal and the Engine does not expose its internals to it.
/// </summary>
public class VendorBoundaryTest
{
    private const BindingFlags AllMembers = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
    private const string VendorName = "Akahu";
    private static readonly string AdapterNamespace = typeof(IBankFeedProvider).Namespace + "." + VendorName;

    private static readonly Assembly[] Assemblies = [typeof(TransactionsListModel).Assembly, typeof(SecureStringCredentialStore).Assembly];

    [Fact]
    public void AdapterNamespace_ShouldContainTheVendorTypes()
    {
        // Guards against this test silently passing because the adapter namespace was renamed.
        typeof(TransactionsListModel).Assembly.GetTypes().ShouldContain(t => t.Namespace == AdapterNamespace);
    }

    [Fact]
    public void NoPublicType_ShouldReferToTheVendor()
    {
        var offenders = Assemblies
            .SelectMany(a => a.GetExportedTypes())
            .Where(t => (t.FullName ?? string.Empty).Contains(VendorName, StringComparison.OrdinalIgnoreCase))
            .Select(t => t.FullName)
            .ToList();

        offenders.ShouldBeEmpty();
    }

    [Fact]
    public void NoTypeOutsideTheAdapter_ShouldUseAnAdapterType()
    {
        var offenders = new List<string>();
        foreach (var type in Assemblies.SelectMany(a => a.GetTypes()).Where(t => !IsInAdapter(t)))
        {
            foreach (var member in type.GetMembers(AllMembers))
            {
                if (ReferencedTypes(member).Any(UsesAdapterType))
                {
                    offenders.Add($"{type.FullName}.{member.Name}");
                }
            }

            if (UsesAdapterType(type.BaseType) || type.GetInterfaces().Any(UsesAdapterType))
            {
                offenders.Add(type.FullName ?? type.Name);
            }
        }

        offenders.ShouldBeEmpty();
    }

    private static bool IsInAdapter(Type type)
    {
        var ns = type.Namespace ?? string.Empty;
        return ns == AdapterNamespace || ns.StartsWith(AdapterNamespace + ".", StringComparison.Ordinal);
    }

    private static IEnumerable<Type> ReferencedTypes(MemberInfo member)
    {
        return member switch
        {
            FieldInfo field => [field.FieldType],
            PropertyInfo property => [property.PropertyType],
            MethodInfo method => method.GetParameters().Select(p => p.ParameterType).Append(method.ReturnType),
            ConstructorInfo constructor => constructor.GetParameters().Select(p => p.ParameterType),
            EventInfo { EventHandlerType: not null } evt => [evt.EventHandlerType],
            _ => []
        };
    }

    private static bool UsesAdapterType(Type? type)
    {
        if (type is null)
        {
            return false;
        }

        if (type.HasElementType)
        {
            return UsesAdapterType(type.GetElementType());
        }

        return IsInAdapter(type) || (type.IsGenericType && type.GetGenericArguments().Any(UsesAdapterType));
    }
}
