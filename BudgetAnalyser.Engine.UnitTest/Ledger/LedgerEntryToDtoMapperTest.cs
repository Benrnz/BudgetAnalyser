using System.Linq;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Ledger
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
            var accountTypeRepo = new InMemoryAccountTypeRepository();
            var subject = new MapperLedgerEntryDto2LedgerEntry(new LedgerBucketFactory(new BucketBucketRepoAlwaysFind(), accountTypeRepo), new LedgerTransactionFactory(), accountTypeRepo);

            Result = subject.ToDto(TestData);
        }
    }
}
