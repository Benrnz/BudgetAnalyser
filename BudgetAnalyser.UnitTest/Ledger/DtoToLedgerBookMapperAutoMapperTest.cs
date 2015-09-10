using System;
using System.Linq;
using AutoMapper;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Ledger
{
    [TestClass]
    public class DtoToLedgerBookMapperAutoMapperTest
    {
        private LedgerBook Result { get; set; }
        private LedgerBookDto TestData => LedgerBookDtoTestData.TestData3();

        [TestMethod]
        public void ShouldMapFileName()
        {
            Assert.AreEqual(@"C:\Folder\FooBook3.xml", Result.StorageKey);
        }

        [TestMethod]
        public void ShouldMapModified()
        {
            Assert.AreEqual(new DateTime(2013, 12, 14), Result.Modified);
        }

        [TestMethod]
        public void ShouldMapName()
        {
            Assert.AreEqual("Test Budget Ledger Book 3", Result.Name);
        }

        [TestMethod]
        public void ShouldMapReconciliations()
        {
            Assert.AreEqual(3, Result.Reconciliations.Count());
        }

        [TestInitialize]
        public void TestInitialise()
        {
            Result = Mapper.Map<LedgerBook>(TestData);
        }
    }
}