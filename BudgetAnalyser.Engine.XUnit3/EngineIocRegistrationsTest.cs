using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using BudgetAnalyser.Encryption;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit;

public class EngineIocRegistrationsTest
{
    /// <summary>
    ///     The interface exclude list.
    ///     This is used to exclude interfaces that do not need to be registered with the IoC container.
    /// </summary>
    private List<Type> ExemptionList => new()
    {
        typeof(ILogger),
        typeof(IModelValidate),
        typeof(IBankExtractImporter),
        typeof(IWidgetWithAdditionalImage),
        typeof(IUserDefinedWidget),
        typeof(IDataChangeDetection),
        typeof(IBudgetCurrencyContext),
        typeof(ICloneable<>),
        typeof(IDtoMapper<,>),
        typeof(IEnvironmentFolders),
        typeof(IPersistentApplicationStateObject)
    };

    [Fact]
    [Description("This test is not a functional test, but is designed to detect new interfaces that have not been assigned to a concrete type with the AutoRegisterWithIoCAttribute or added to the exclude list. This prevents runtime errors where the IoC container cannot resolve a concrete type for the new interface.")]
    public void EnsureAllInterfacesAreRegisteredWithIoC()
    {
        try
        {
            var dependencies = EngineIocRegistrations.RegisterAutoMappingsFromAssembly(typeof(TransactionsListModel).Assembly).ToList();
            dependencies.AddRange(EngineIocRegistrations.RegisterAutoMappingsFromAssembly(typeof(SecureStringCredentialStore).Assembly));

            var interfaces = typeof(TransactionsListModel).Assembly.GetTypes().Where(t => t.IsInterface);

            var exemptionListNames = ExemptionList.Select(e => e.FullName).ToList();
            foreach (var interfaceType in interfaces.Except(ExemptionList))
            {
                Console.Write("Interface: {0}", interfaceType.Name);
                if (exemptionListNames.Contains(interfaceType.FullName!))
                {
                    continue;
                }

                if (!dependencies.Any(d => IsSelfRegistered(interfaceType, d)))
                {
                    throw new Xunit.Sdk.XunitException($"Interface: {interfaceType.FullName} is not registered.");
                }

                Console.WriteLine(" registered.");
            }
        }
        catch (ReflectionTypeLoadException ex)
        {
            foreach (var exception in ex.LoaderExceptions)
            {
                Debug.WriteLine(exception);
            }

            throw;
        }
    }

    [Fact]
    public void ProcessPropertyInjection_ShouldBeAbleToAssignLoggerToProperty()
    {
        var logger = new FakeLogger();
        var result = EngineIocRegistrations.ProcessPropertyInjection(GetType().Assembly);
        result.First().PropertyInjectionAssignment(logger);

        AutoRegisterWithIoCProcessorPropertyInjectionTestSource.Logger.ShouldBeSameAs(logger);
    }

    [Fact]
    public void ProcessPropertyInjection_ShouldFindOnePropertyInjectionDependency()
    {
        var result = EngineIocRegistrations.ProcessPropertyInjection(GetType().Assembly);
        result.Count().ShouldBe(1);
    }

    [Fact]
    public void ProcessPropertyInjection_ShouldFindStaticClassWithILoggerProperty()
    {
        var result = EngineIocRegistrations.ProcessPropertyInjection(GetType().Assembly);
        result.First().Type.ShouldBe(typeof(ILogger));
    }

    [Fact]
    public void ProcessPropertyInjectionShouldThrowGivenNullAssembly()
    {
        Should.Throw<ArgumentNullException>(() =>
            EngineIocRegistrations.ProcessPropertyInjection(null!).Any());
    }

    [Fact]
    public void RegisterAutoMappings_ShouldReturnFakeLoggerRegistration()
    {
        var result = EngineIocRegistrations.RegisterAutoMappingsFromAssembly(GetType().Assembly);
        var loggerRegistration = result.Last();
        loggerRegistration.Type.ShouldBe(typeof(FakeLogger));
        loggerRegistration.IsSingleInstance.ShouldBeTrue();
        loggerRegistration.NamedInstanceName.ShouldBe("Named Logger");
    }

    [Fact]
    public void RegisterAutoMappings_ShouldReturnTwoGivenThisAssembly()
    {
        var result = EngineIocRegistrations.RegisterAutoMappingsFromAssembly(GetType().Assembly);
        result.Count().ShouldBe(3);
    }

    [Fact]
    public void AddAutoRegistrations_ShouldResolveNamedInstancesByKey()
    {
        var services = new ServiceCollection();
        services.AddAutoRegistrations(GetType().Assembly);
        var provider = services.BuildServiceProvider();

        var encrypted = provider.GetRequiredKeyedService<IFileReaderWriter>(StorageConstants.EncryptedInstanceName);
        var unprotected = provider.GetRequiredKeyedService<IFileReaderWriter>(StorageConstants.UnprotectedInstanceName);

        encrypted.ShouldBeOfType<EmbeddedResourceFileReaderWriterEncrypted>();
        unprotected.ShouldBeOfType<EmbeddedResourceFileReaderWriter>();
    }

    [Fact]
    public void AddAutoRegistrations_ShouldAlsoResolveNamedInstancesUnkeyed()
    {
        var services = new ServiceCollection();
        services.AddAutoRegistrations(GetType().Assembly);
        var provider = services.BuildServiceProvider();

        var allReaderWriters = provider.GetServices<IFileReaderWriter>().ToList();

        allReaderWriters.Count.ShouldBe(2);
        allReaderWriters.ShouldContain(x => x is EmbeddedResourceFileReaderWriterEncrypted);
        allReaderWriters.ShouldContain(x => x is EmbeddedResourceFileReaderWriter);
    }

    [Fact]
    public void RegisterAutoMappings_ShouldThrowGivenNullAssembly()
    {
        Should.Throw<ArgumentNullException>(() =>
            EngineIocRegistrations.RegisterAutoMappingsFromAssembly(null!).ToList());
    }

    private bool IsSelfRegistered(Type interfaceType, DependencyRegistrationRequirement d)
    {
        if (interfaceType == d.Type)
        {
            return true;
        }

        var implementedInterfaces = d.Type.GetInterfaces();
        return implementedInterfaces.Contains(interfaceType);
    }
}
