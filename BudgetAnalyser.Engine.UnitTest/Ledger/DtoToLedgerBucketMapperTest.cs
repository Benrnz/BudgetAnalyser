using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Ledger
{
    [TestClass]
    public class DtoToLedgerBucketMapperTest
    {
        private LedgerBucket Result { get; set; }

        private LedgerBucketDto TestData => new LedgerBucketDto
        {
            BucketCode = TestDataConstants.RegoBucketCode,
            StoredInAccount = TestDataConstants.ChequeAccountName
        };

        [TestMethod]
        public void ShouldMapBankAccount()
        {
            Assert.AreEqual(StatementModelTestData.ChequeAccount.Name, Result.StoredInAccount.Name);
        }

        [TestMethod]
        public void ShouldMapBudgetBucketCode()
        {
            Assert.AreEqual(TestDataConstants.RegoBucketCode, Result.BudgetBucket.Code);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            var bucketRepo = new BucketBucketRepoAlwaysFind();
            var accountRepo = new InMemoryAccountTypeRepository();
            var subject = new MapperLedgerBucketDto2LedgerBucket(bucketRepo, accountRepo, new LedgerBucketFactory(bucketRepo, accountRepo));
            Result = subject.ToModel(TestData);
        }
    }
}
