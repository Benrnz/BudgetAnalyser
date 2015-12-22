using System.Linq;
using AutoMapper;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Ledger
{
    [TestClass]
    public class LedgerEntryToDtoMapperTest
    {
        private LedgerEntryDto Result { get; set; }
        private LedgerEntry TestData => LedgerBookTestData.TestData1().Reconciliations.First().Entries.First();

        [TestMethod]
        public void ShouldMapBalance()
        {
            Assert.AreEqual(120M, Result.Balance);
        }

        [TestMethod]
        public void ShouldMapBucketCode()
        {
            Assert.AreEqual(TestDataConstants.HairBucketCode, Result.BucketCode);
        }

        [TestMethod]
        public void ShouldMapCorrectNumberOfTransactions()
        {
            Assert.AreEqual(1, Result.Transactions.Count);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            Result = Mapper.Map<LedgerEntryDto>(TestData);
        }
    }
}