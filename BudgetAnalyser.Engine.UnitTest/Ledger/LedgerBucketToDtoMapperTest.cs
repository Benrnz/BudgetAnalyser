using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Ledger
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
            var bucketRepo = new BucketBucketRepoAlwaysFind();
            var accountRepo = new InMemoryAccountTypeRepository();
            var subject = new Mapper_LedgerBucketDto_LedgerBucket(bucketRepo, accountRepo, new LedgerBucketFactory(bucketRepo, accountRepo));
            Result = subject.ToDto(TestData);
        }
    }
}