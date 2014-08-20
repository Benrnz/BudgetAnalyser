using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Statement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BudgetAnalyser.UnitTest.Statement
{
    [TestClass]
    public class BankStatementImporterRepositoryTest
    {
        private IList<Mock<IBankStatementImporter>> Importers { get; set; }
        private BankStatementImporterRepository Subject { get; set; }

        [TestMethod]
        public void CanImportShouldReturnFalseGivenNoImportersCanImport()
        {
            Assert.IsFalse(Subject.CanImport("Foo.bar"));
        }

        [TestMethod]
        public void CanImportShouldReturnTrueGivenOneImporterCanImport()
        {
            Importers[1].Setup(m => m.TasteTest("Foo.bar")).Returns(true);
            Assert.IsTrue(Subject.CanImport("Foo.bar"));
        }

        [TestMethod]
        public void CtorShouldConstructGivenValidListOfImporters()
        {
            BankStatementImporterRepository subject = CreateSubject();
            Assert.IsNotNull(subject);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenEmptyListOfImporters()
        {
            new BankStatementImporterRepository(new List<IBankStatementImporter>());
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullListOfImporters()
        {
            new BankStatementImporterRepository(null);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void ImportShouldThrowGivenNoImportersCanImport()
        {
            var model = Subject.Import("Foo.bar", new ChequeAccount("Cheque"));
        }

        [TestInitialize]
        public void TestInitialise()
        {
            Importers = new[]
            {
                new Mock<IBankStatementImporter>(),
                new Mock<IBankStatementImporter>()
            };
            Subject = CreateSubject();
        }

        private BankStatementImporterRepository CreateSubject()
        {
            return new BankStatementImporterRepository(Importers.Select(i => i.Object));
        }
    }
}