using System;
using System.Linq;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.Statement.Data;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Statement
{
    [TestClass]
    public class StatementModelToDtoMapperTest
    {
        private TransactionSetDto Result { get; set; }
        private StatementModel TestData => StatementModelTestData.TestData2();

        [TestMethod]
        public void ShouldMapAllTransactions()
        {
            Assert.AreEqual(TestData.AllTransactions.Count(), Result.Transactions.Count());
        }

        [TestMethod]
        public void ShouldMapAllTransactionsAndHaveSameSum()
        {
            Assert.AreEqual(TestData.AllTransactions.Sum(t => t.Amount), Result.Transactions.Sum(t => t.Amount));
            Assert.AreEqual(TestData.AllTransactions.Sum(t => t.Date.Ticks), Result.Transactions.Sum(t => t.Date.Ticks));
        }

        [TestMethod]
        public void ShouldMapAllTransactionsEvenWhenFiltered()
        {
            StatementModel testData = TestData;
            testData.Filter(new GlobalFilterCriteria { BeginDate = new DateTime(2013, 07, 20), EndDate = new DateTime(2013, 08, 19) });
            Act(testData);

            Assert.AreEqual(TestData.AllTransactions.Count(), Result.Transactions.Count);
        }

        [TestMethod]
        public void ShouldMapFileName()
        {
            Assert.AreEqual(TestData.StorageKey, Result.StorageKey);
        }

        [TestMethod]
        public void ShouldMapLastImport()
        {
            Assert.AreEqual(TestData.LastImport, Result.LastImport);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            Act(TestData);
        }

        private void Act(StatementModel testData)
        {
            var subject = new Mapper_TransactionSetDto_StatementModel(
                new FakeLogger(),
                new Mapper_TransactionDto_Transaction(
                    new InMemoryAccountTypeRepository(),
                    new BucketBucketRepoAlwaysFind(),
                    new InMemoryTransactionTypeRepository()));
            Result = subject.ToDto(testData);
        }
    }
}