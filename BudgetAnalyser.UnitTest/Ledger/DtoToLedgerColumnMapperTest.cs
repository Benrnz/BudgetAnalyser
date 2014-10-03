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
        private LedgerColumn Result { get; set; }

        private LedgerColumnDto TestData
        {
            get
            {
                return new LedgerColumnDto
                {
                    BucketCode = TestDataConstants.RegoBucketCode,
                    StoredInAccount = TestDataConstants.ChequeAccountName,
                };
            }
        }

        [TestMethod]
        public void ShouldMapBudgetBucketCode()
        {
            Assert.AreEqual(TestDataConstants.RegoBucketCode, Result.BudgetBucket.Code);
        }

        [TestMethod]
        public void ShouldMapBankAccount()
        {
            Assert.AreEqual(StatementModelTestData.ChequeAccount.Name, Result.StoredInAccount.Name);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            Result = Mapper.Map<LedgerColumn>(TestData);
        }
    }
}