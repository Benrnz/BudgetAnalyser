using System;
using System.Collections;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using BudgetAnalyser.Engine;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BudgetAnalyser.UnitTest
{
    [TestClass]
    public class AutoRegisterWithIoCProcessorTest
    {
        public Mock<IComponentRegistry> ComponentRegistryMock { get; set; }
        private Mock<IComponentContext> ContainerMock { get; set; }

        [TestMethod]
        public void ProcessPropertyInjectionShouldFindStaticClassWithProperty()
        {
            IComponentRegistration registration = null;
            ComponentRegistryMock.Setup(m => m.TryGetRegistration(It.IsAny<TypedService>(), out registration)).Returns(true);
            ContainerMock.Setup(m => m.ComponentRegistry).Returns(ComponentRegistryMock.Object);
            ContainerMock.Setup(m => m.ResolveComponent(registration, It.IsAny<IEnumerable<Parameter>>())).Returns(new FakeLogger());
            AutoRegisterWithIoCProcessor.ProcessPropertyInjection(ContainerMock.Object, GetType().Assembly);

            Assert.IsNotNull(AutoRegisterWithIoCProcessorPropertyInjectionTestSource.Logger);
            Assert.IsNull(AutoRegisterWithIoCProcessorPropertyInjectionTestSource.NotInjected);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProcessPropertyInjectionShouldThrowGivenNullAssembly()
        {
            AutoRegisterWithIoCProcessor.ProcessPropertyInjection(ContainerMock.Object, null);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProcessPropertyInjectionShouldThrowGivenNullContainer()
        {
            AutoRegisterWithIoCProcessor.ProcessPropertyInjection(null, GetType().Assembly);
            Assert.Fail();
        }

        [TestInitialize]
        public void TestInitialise()
        {
            ComponentRegistryMock = new Mock<IComponentRegistry>();
            ContainerMock = new Mock<IComponentContext>();
        }
    }
}