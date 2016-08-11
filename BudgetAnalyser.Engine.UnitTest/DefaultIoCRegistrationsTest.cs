using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Encryption;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using BudgetAnalyser.Engine.Widgets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rees.TangyFruitMapper;

namespace BudgetAnalyser.Engine.UnitTest
{
    [TestClass]
    public class DefaultIoCRegistrationsTest
    {
        /// <summary>
        ///     The interface exclude list.
        ///     This is used to exclude interfaces that do not need to be registered with the IoC container.
        /// </summary>
        private List<Type> ExemptionList
        {
            get
            {
                return new List<Type>
                {
                    typeof(IModelValidate), // Used to indicate support for standard validation.
                    typeof(IBankStatementImporter), // The implementations of this interface are discovered by reflection.
                    typeof(IWidgetWithAdditionalImage), // Used only to give consistency when a second image is needed in a widget.
                    typeof(IUserDefinedWidget), // Used to mark a widget as being multi-instance as opposed to the ordinary single instance approach.
                    typeof(IDataChangeDetection), // Used to mark a type that can report back when data has changed. Similar to INotifyPropertyChange but across the whole type.
                    typeof(IBudgetCurrencyContext), // Used to wrap a Budget Model and how it relates to time - is it the most current, an old one, or a future one.
                    typeof(ICloneable<>), // Used to consistently implement cloning across multiple types.
                    typeof(IDtoMapper<,>), // Used to consistently implement mappers, mappers are internal and do not need to be registered in an IoC container.
                    typeof(IEnvironmentFolders), // Must be implemented in the UI project as it is platform dependent.
                    typeof(IPersistentApplicationStateObject) // Used to consistently implement a grain or persistent application data. This does not need to be registered with an IoC container.
                };
            }
        }

        [TestMethod]
        [Description("This test is not a functional test, but is designed to detect new interfaces that have not been assigned to a concrete type with the AutoRegisterWithIoCAttribute or added to" +
                     " the exclude list. This prevents runtime errors where the IoC container cannot resolve a concrete type for the new interface.")]
        public void EnsureAllInterfacesAreRegisteredWithIoC()
        {
            List<DependencyRegistrationRequirement> dependencies = DefaultIoCRegistrations.RegisterAutoMappingsFromAssembly(typeof(StatementModel).Assembly).ToList();
            dependencies.AddRange(DefaultIoCRegistrations.RegisterAutoMappingsFromAssembly(typeof(SecureStringCredentialStore).Assembly));

            IEnumerable<Type> interfaces = typeof(StatementModel).Assembly.GetTypes().Where(t => t.IsInterface);

            List<string> exemptionListNames = ExemptionList.Select(e => e.FullName).ToList();
            foreach (var interfaceType in interfaces.Except(ExemptionList))
            {
                Console.Write("Interface: {0}", interfaceType.Name);
                if (exemptionListNames.Contains(interfaceType.FullName)) continue;
                if (!dependencies.Any(d => d.AdditionalRegistrationType == interfaceType || IsSelfRegistered(interfaceType, d)))
                {
                    Assert.Fail($"Interface: {interfaceType.FullName} is not registered.");
                }

                Console.WriteLine(" registered.");
            }
        }

        [TestMethod]
        public void ProcessPropertyInjection_ShouldBeAbleToAssignLoggerToProperty()
        {
            var logger = new FakeLogger();
            IEnumerable<PropertyInjectionDependencyRequirement> result = DefaultIoCRegistrations.ProcessPropertyInjection(GetType().Assembly);
            result.First().PropertyInjectionAssignment(logger);

            Assert.AreSame(logger, AutoRegisterWithIoCProcessorPropertyInjectionTestSource.Logger);
        }

        [TestMethod]
        public void ProcessPropertyInjection_ShouldFindOnePropertyInjectionDependency()
        {
            IEnumerable<PropertyInjectionDependencyRequirement> result = DefaultIoCRegistrations.ProcessPropertyInjection(GetType().Assembly);
            Assert.AreEqual(1, result.Count());
        }

        [TestMethod]
        public void ProcessPropertyInjection_ShouldFindStaticClassWithILoggerProperty()
        {
            IEnumerable<PropertyInjectionDependencyRequirement> result = DefaultIoCRegistrations.ProcessPropertyInjection(GetType().Assembly);
            Assert.AreEqual(typeof(ILogger), result.First().DependencyRequired);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProcessPropertyInjectionShouldThrowGivenNullAssembly()
        {
            IEnumerable<PropertyInjectionDependencyRequirement> result = DefaultIoCRegistrations.ProcessPropertyInjection(null);
            result.Any();
            Assert.Fail();
        }

        [TestMethod]
        public void RegisterAutoMappings_ShouldReturnFakeLoggerRegistration()
        {
            IEnumerable<DependencyRegistrationRequirement> result = DefaultIoCRegistrations.RegisterAutoMappingsFromAssembly(GetType().Assembly);
            var loggerRegistration = result.Last();
            Assert.AreEqual(typeof(FakeLogger), loggerRegistration.DependencyRequired);
            Assert.IsTrue(loggerRegistration.IsSingleInstance);
            Assert.AreEqual("Named Logger", loggerRegistration.NamedInstanceName);
        }

        [TestMethod]
        public void RegisterAutoMappings_ShouldReturnTwoGivenThisAssembly()
        {
            IEnumerable<DependencyRegistrationRequirement> result = DefaultIoCRegistrations.RegisterAutoMappingsFromAssembly(GetType().Assembly);
            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterAutoMappings_ShouldThrowGivenNullAssembly()
        {
            IEnumerable<DependencyRegistrationRequirement> result = DefaultIoCRegistrations.RegisterAutoMappingsFromAssembly(null);

            result.ToList();

            Assert.Fail();
        }

        [TestInitialize]
        public void TestInitialise()
        {
        }

        private bool IsSelfRegistered(Type interfaceType, DependencyRegistrationRequirement d)
        {
            if (interfaceType == d.DependencyRequired) return true;

            Type[] implementedInterfaces = d.DependencyRequired.GetInterfaces();
            return implementedInterfaces.Contains(interfaceType);
        }
    }
}