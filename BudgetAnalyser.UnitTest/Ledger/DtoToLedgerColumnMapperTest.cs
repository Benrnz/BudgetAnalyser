using AutoMapper;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Ledger
{
    [TestClass]
    public class DtoToLedgerColumnMapperTest
    {
        private LedgerColumnDto Result { get; set; }

        private LedgerColumn TestData
        {
            get
            {
                return new LedgerColumn
                {
                    BudgetBucket = StatementModelTestData.RegoBucket,
                    StoredInAccount = StatementModelTestData.ChequeAccount,
                };
            }
        }

        [TestMethod]
        public void ShouldMapBudgetBucketCode()
        {
            Assert.AreEqual(TestDataConstants.RegoBucketCode, Result.BucketCode);
        }

        [TestMethod]
        public void ShouldMapBankAccount()
        {
            Assert.AreEqual(StatementModelTestData.ChequeAccount.Name, Result.StoredInAccount);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            Result = Mapper.Map<LedgerColumnDto>(TestData);
        }
    }
}