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
    public class LedgerEntryLineToDtoMapperTest
    {
        private LedgerEntryLineDto Result { get; set; }

        private LedgerEntryLine TestData
        {
            get
            {
                LedgerBook book = LedgerBookTestData.TestData4();
                return book.Reconciliations.First();
            }
        }

        [TestMethod]
        public void ShouldMapBankBalance()
        {
            Assert.AreEqual(2950M, Result.BankBalance);
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
        public void ShouldMapDate()
        {
            Assert.AreEqual(new DateTime(2013, 8, 15), Result.Date);
        }

        [TestMethod]
        public void ShouldMapEntries()
        {
            Assert.AreEqual(3, Result.Entries.Count());
        }

        [TestMethod]
        public void ShouldMapRemarks()
        {
            Assert.AreEqual("The quick brown fox jumped over the lazy dog", Result.Remarks);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            Result = Mapper.Map<LedgerEntryLineDto>(TestData);
        }
    }
}