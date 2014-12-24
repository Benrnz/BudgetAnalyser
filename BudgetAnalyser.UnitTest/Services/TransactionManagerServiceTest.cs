using System;
using System.Linq;
using System.Threading.Tasks;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.UnitTest.TestData;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BudgetAnalyser.UnitTest.Services
{
    [TestClass]
    public class TransactionManagerServiceTest
    {
        private Mock<IBudgetBucketRepository> mockBudgetBucketRepo;
        private Mock<IStatementRepository> mockStatementRepo;
        private TransactionManagerService subject;
        private StatementModel testData;

        [TestMethod]
        public void AverageDebit_ShouldBe115_GivenTestData2()
        {
            Assert.AreEqual(-115.25M, decimal.Round(this.subject.AverageDebit, 2));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_ShouldThrow_GivenNullBucketRepo()
        {
            new TransactionManagerService(null, this.mockStatementRepo.Object, new FakeLogger());
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_ShouldThrow_GivenNullLogger()
        {
            new TransactionManagerService(this.mockBudgetBucketRepo.Object, this.mockStatementRepo.Object, null);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_ShouldThrow_GivenNullStatementRepo()
        {
            new TransactionManagerService(this.mockBudgetBucketRepo.Object, null, new FakeLogger());
            Assert.Fail();
        }

        [TestMethod]
        public void DetectDuplicateTransactions_ShouldReturnEmpty_GivenTestData2()
        {
            Assert.IsTrue(string.IsNullOrWhiteSpace(this.subject.DetectDuplicateTransactions()));
        }

        [TestMethod]
        public void DetectDuplicateTransactions_ShouldReturnNull_GivenNullStatementModel()
        {
            Assert.IsNull(this.subject.DetectDuplicateTransactions());
        }

        [TestMethod]
        public void DetectDuplicateTransactions_ShouldSummaryText_GivenTestData4()
        {
            this.testData = StatementModelTestData.TestData4();
            Arrange();

            var result = this.subject.DetectDuplicateTransactions();
            Console.WriteLine(result);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result));
        }

        [TestMethod]
        public async Task LoadStatementModelAsync_ShouldReturnAStatementModel_GivenValidStorageKey()
        {
            this.mockStatementRepo.Setup(m => m.LoadStatementModelAsync(It.IsAny<string>())).Returns(Task.FromResult(new StatementModel(new FakeLogger())));

            var statement = await this.subject.LoadStatementModelAsync(@"C:\Foo.csv");

            Assert.IsNotNull(statement);
        }

        [TestMethod]
        public void FilterableBuckets_ShouldContainABlankElement_GivenAnyBucketList()
        {
            Assert.IsTrue(this.subject.FilterableBuckets().Any(string.IsNullOrWhiteSpace));
        }

        [TestMethod]
        public void FilterableBuckets_ShouldContainUncatergorised_GivenAnyBucketList()
        {
            Assert.IsTrue(this.subject.FilterableBuckets().Any(b => b == TransactionManagerService.UncategorisedFilter));
        }

        [TestMethod]
        public void FilterTransactions_ShouldCallStatementModel_GivenFilterObject()
        {
            this.testData = new StatementModelTestHarness();
            var criteria = new GlobalFilterCriteria { BeginDate = new DateTime(2014, 07, 01), EndDate = new DateTime(2014, 08, 01) };

            Arrange();

            this.subject.FilterTransactions(criteria);

            Assert.AreEqual(1, ((StatementModelTestHarness)this.testData).FilterByCriteriaWasCalled);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FilterTransactions_ShouldThrow_GivenNullFilter()
        {
            this.subject.FilterTransactions((GlobalFilterCriteria)null);
            Assert.Fail();
        }

        [TestMethod]
        public void FilterTransactions_ShouldCallStatementModel_GivenSearchText()
        {
            this.testData = new StatementModelTestHarness();

            Arrange();

            this.subject.FilterTransactions("Fooey");

            Assert.AreEqual(1, ((StatementModelTestHarness)this.testData).FilterByTextWasCalled);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FilterTransactions_ShouldThrow_GivenNullSearchText()
        {
            this.testData = new StatementModelTestHarness();

            Arrange();

            this.subject.FilterTransactions(string.Empty);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ImportAndMergeBankStatement_ShouldThrow_GivenNullStorageKey()
        {
            this.subject.ImportAndMergeBankStatement(null, new ChequeAccount("Foo"));
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ImportAndMergeBankStatement_ShouldThrow_GivenNullAccount()
        {
            this.subject.ImportAndMergeBankStatement("Sticky Bag.csv", null);
            Assert.Fail();
        }

        [TestMethod]
        public void ImportAndMergeBankStatement_ShouldCallStatementRepo_GivenStorageKeyAndAccount()
        {
            this.mockStatementRepo
                .Setup(m => m.ImportAndMergeBankStatement(It.IsAny<string>(), It.IsAny<AccountType>()))
                .Returns(StatementModelTestData.TestData2)
                .Verifiable();

            this.subject.ImportAndMergeBankStatement("Sticky Bag.csv", StatementModelTestData.ChequeAccount);

            this.mockStatementRepo.Verify();
        }

        [TestMethod]
        public void ImportAndMergeBankStatement_ShouldMergeTheModel_GivenStorageKeyAndAccount()
        {
            this.testData = new StatementModelTestHarness();

            Arrange();

            this.mockStatementRepo
                .Setup(m => m.ImportAndMergeBankStatement(It.IsAny<string>(), It.IsAny<AccountType>()))
                .Returns(StatementModelTestData.TestData2)
                .Verifiable();

            this.subject.ImportAndMergeBankStatement("Sticky Bag.csv", StatementModelTestData.ChequeAccount);

            Assert.AreEqual(1, ((StatementModelTestHarness)this.testData).MergeWasCalled);
        }

        [TestInitialize]
        public void TestInit()
        {
            this.testData = StatementModelTestData.TestData2();

            this.mockBudgetBucketRepo = new Mock<IBudgetBucketRepository>();
            this.mockStatementRepo = new Mock<IStatementRepository>();

            this.mockStatementRepo.Setup(m => m.LoadStatementModelAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(this.testData))
                .Verifiable();

            this.mockBudgetBucketRepo
                .Setup(m => m.Buckets)
                .Returns(BudgetBucketTestData.BudgetModelTestData1Buckets);

            Arrange();
        }

        [TestMethod]
        public void PopulateGroupByBucketCollection_ShouldReturnListOf6_GivenStatementModelAndTrue()
        {
            var result = this.subject.PopulateGroupByBucketCollection(true);

            Assert.AreEqual(6, result.Count());
        }

        [TestMethod]
        public void PopulateGroupByBucketCollection_ShouldReturnListSortedByBucket_GivenStatementModelAndTrue()
        {
            var result = this.subject.PopulateGroupByBucketCollection(true);

            TransactionGroupedByBucket previous = null;
            foreach (var groupedByBucket in result)
            {
                if (previous == null)
                {
                    previous = groupedByBucket;
                    continue;
                }

                if (string.Compare(previous.Bucket.Code, groupedByBucket.Bucket.Code, StringComparison.Ordinal) >= 0)
                {
                    Assert.Fail("The grouped list should be sorted by Bucket Code in ascending order. {0} >= {1}", previous.Bucket.Code, groupedByBucket.Bucket.Code);
                }

                previous = groupedByBucket;
            }
        }

        [TestMethod]
        public void PopulateGroupByBucketCollection_ShouldReturnEmpty_GivenStatementModelNotLoaded()
        {
            this.subject = CreateSubject();
            var result = this.subject.PopulateGroupByBucketCollection(true);

            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public void PopulateGroupByBucketCollection_ShouldReturnEmpty_GivenFalse()
        {
            var result = this.subject.PopulateGroupByBucketCollection(false);

            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task LoadStatementModelAsync_ShouldThrow_GivenNullStorageKey()
        {
            await this.subject.LoadStatementModelAsync(null);
            Assert.Fail();
        }

        [TestMethod]
        public async Task LoadStatementModelAsync_ShouldCallStatementRepo_GivenValidStorageKey()
        {
            await this.subject.LoadStatementModelAsync(@"D:\kjgkjgkjgkgk.csv");
            this.mockStatementRepo.Verify();
        }

        [TestMethod]
        public void TotalCount_ShouldBe10_GivenTestData2()
        {
            Assert.AreEqual(10, this.subject.TotalCount);
        }

        [TestMethod]
        public void TotalCredits_ShouldBe2552_GivenTestData2()
        {
            Assert.AreEqual(2552.97M, this.subject.TotalCredits);
        }

        [TestMethod]
        public void TotalDebits_ShouldBe806_GivenTestData2()
        {
            Assert.AreEqual(-806.78M, this.subject.TotalDebits);
        }

        [TestMethod]
        public void TotalDifference_ShouldBe1746_GivenTestData2()
        {
            Assert.AreEqual(1746.19M, this.subject.TotalCredits + this.subject.TotalDebits);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemoveTransaction_ShouldThrow_GivenNullTransaction()
        {
            this.subject.RemoveTransaction(null);

            Assert.Fail();
        }

        [TestMethod]
        public void RemoveTransaction_ShouldCallStatementModelRemove_GivenATransaction()
        {
            this.testData = new StatementModelTestHarness();
            this.testData.LoadTransactions(StatementModelTestData.TestData2().Transactions);
            Arrange();
            var transaction = this.testData.Transactions.Skip(1).First();

            this.subject.RemoveTransaction(transaction);

            Assert.AreEqual(1, ((StatementModelTestHarness)this.testData).RemoveTransactionWasCalled);
        }

        private void Arrange()
        {
            this.mockStatementRepo
                .Setup(m => m.LoadStatementModelAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(this.testData));
            this.subject = CreateSubject();
            this.subject.LoadStatementModelAsync("Foo").Wait();
        }

        private TransactionManagerService CreateSubject()
        {
            return new TransactionManagerService(this.mockBudgetBucketRepo.Object, this.mockStatementRepo.Object, new FakeLogger());
        }
    }
}