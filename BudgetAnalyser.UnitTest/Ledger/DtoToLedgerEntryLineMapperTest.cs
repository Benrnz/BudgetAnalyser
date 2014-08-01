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
    public class DtoToLedgerEntryLineMapperTest
    {
        private LedgerEntryLine Result { get; set; }

        private LedgerEntryLineDto TestData
        {
            get
            {
                var book = LedgerBookDtoTestData.TestData3();
                return book.DatedEntries.First();
            }
        }

        [TestMethod]
        public void ShouldMapBankBalanceAdjustments()
        {
            Assert.AreEqual(1, Result.BankBalanceAdjustments.Count());
        }

        [TestMethod]
        public void ShouldMapBankBalances()
        {
            Assert.AreEqual(2, Result.BankBalances.Count());
        }

        [TestMethod]
        public void ShouldMapSurplus()
        {
            Assert.AreEqual(1498.99M, Result.CalculatedSurplus);
        }

        [TestMethod]
        public void ShouldMapDate()
        {
            Assert.AreEqual(new DateTime(2014, 2, 20), Result.Date);
        }

        [TestMethod]
        public void ShouldMapEntries()
        {
            Assert.AreEqual(3, Result.Entries.Count());
        }

        [TestMethod]
        public void ShouldMapLedgerBalance()
        {
            Assert.AreEqual(1902.44M, Result.LedgerBalance);
        }

        [TestMethod]
        public void ShouldMapRemarks()
        {
            Assert.AreEqual("Lorem ipsum dolor. Mit solo darte.", Result.Remarks);
        }

        [TestMethod]
        public void ShouldMapTotalBalanceAdjustments()
        {
            Assert.AreEqual(-100.01M, Result.TotalBalanceAdjustments);
        }

        [TestMethod]
        public void ShouldMapTotalBankBalance()
        {
            Assert.AreEqual(2002.45M, Result.TotalBankBalance);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            AutoMapperConfigurationTest.AutoMapperConfiguration();

            Result = Mapper.Map<LedgerEntryLine>(TestData);
        }
    }
}