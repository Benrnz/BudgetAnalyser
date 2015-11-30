using AutoMapper;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Ledger
{
    [TestClass]
    public class LedgerBucketToDtoMapperTest
    {
        private LedgerBucketDto Result { get; set; }

        private LedgerBucket TestData => LedgerBookTestData.RegoLedger;

        [TestMethod]
        public void ShouldMapBankAccount()
        {
            Assert.AreEqual(StatementModelTestData.ChequeAccount.Name, Result.StoredInAccount);
        }

        [TestMethod]
        public void ShouldMapBudgetBucketCode()
        {
            Assert.AreEqual(TestDataConstants.RegoBucketCode, Result.BucketCode);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            Result = Mapper.Map<LedgerBucketDto>(TestData);
        }
    }
}