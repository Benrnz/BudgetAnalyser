using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Statement;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest
{
    [TestClass]
    public class DefaultIoCRegistrationsTest
    {
        private List<Type> ExemptionList
        {
            get
            {
                return new List<Type>
                {
                    typeof (IModelValidate), // Used to indicate support for standard validation.
                    typeof (IBankStatementImporter), // The implementations of this interface are discovered by reflection.
                };
            }
        }

        [TestMethod]
        public void EnsureAllInterfacesAreRegisteredWithIoC()
        {
            var builder = new ContainerBuilder();
            DefaultIoCRegistrations.RegisterDefaultMappings(builder);
            var container = builder.Build();

            var interfaces = typeof(FileFormatException).Assembly.GetTypes().Where(t => t.IsInterface);

            foreach (var interfaceType in interfaces.Except(ExemptionList))
            {
                Console.Write("Interface: {0}", interfaceType.Name);
                if (!container.IsRegistered(interfaceType))
                {
                    Assert.Fail("Interface: {0} is not registered.", interfaceType.Name);
                }

                Console.WriteLine(" registered.");
            }
        }
    }
}
