using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.UnitTest.Helper;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rees.UnitTestUtilities;

namespace BudgetAnalyser.Engine.UnitTest.Services
{
    [TestClass]
    public class TransactionManagerServiceTest
    {
        private readonly ApplicationDatabase testAppDb = new ApplicationDatabase();
        private Mock<IBudgetBucketRepository> mockBudgetBucketRepo;
        private IBudgetBucketRepository budgetBucketRepo; // By default set to return mockBudgetBucketRepo.Object;
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
            new TransactionManagerService(null, this.mockStatementRepo.Object, new FakeLogger(), new FakeMonitorableDependencies());
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_ShouldThrow_GivenNullLogger()
        {
            new TransactionManagerService(this.mockBudgetBucketRepo.Object, this.mockStatementRepo.Object, null, new FakeMonitorableDependencies());
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_ShouldThrow_GivenNullStatementRepo()
        {
            new TransactionManagerService(this.mockBudgetBucketRepo.Object, null, new FakeLogger(), new FakeMonitorableDependencies());
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
            this.subject = CreateSubject();
            Assert.IsNull(this.subject.DetectDuplicateTransactions());
        }

        [TestMethod]
        public void DetectDuplicateTransactions_ShouldSummaryText_GivenTestData4()
        {
            this.testData = StatementModelTestData.TestData4();
            Arrange();

            string result = this.subject.DetectDuplicateTransactions();
            Console.WriteLine(result);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result));
        }

        [TestMethod]
        public void FilterableBuckets_ShouldContainABlankElement_GivenAnyBucketList()
        {
            Assert.IsTrue(this.subject.FilterableBuckets().Any(string.IsNullOrWhiteSpace));
        }

        [TestMethod]
        public void FilterableBuckets_ShouldContainUncatergorised_GivenAnyBucketList()
        {
            Assert.IsTrue(this.subject.FilterableBuckets().Any(b => b == TransactionConstants.UncategorisedFilter));
        }

        [TestMethod]
        public void FilterByBucket_ShouldReturnAllBuckets_GivenNullBucketCode()
        {
            this.budgetBucketRepo = new BucketBucketRepoAlwaysFind();
            Arrange();

            var result = this.subject.FilterByBucket(null);

            Assert.AreEqual(10, result.Count());
        }

        [TestMethod]
        public void FilterByBucket_ShouldReturnAllBuckets_GivenEmptyBucketCode()
        {
            this.budgetBucketRepo = new BucketBucketRepoAlwaysFind();
            Arrange();

            var result = this.subject.FilterByBucket(string.Empty);

            Assert.AreEqual(10, result.Count());
        }

        [TestMethod]
        public void FilterByBucket_ShouldReturn3Buckets_GivenIncomeBucketCode()
        {
            this.budgetBucketRepo = new BucketBucketRepoAlwaysFind();
            Arrange();

            var result = this.subject.FilterByBucket(StatementModelTestData.IncomeBucket.Code);

            Assert.AreEqual(3, result.Count());
        }

        [TestMethod]
        public void FilterByBucket_ShouldReturn2Buckets_GivenSurplusBucketCode()
        {
            var model2 = new StatementModelBuilder()
                .AppendTransaction(
                    new Transaction
                    {
                        Account = StatementModelTestData.ChequeAccount,
                        Amount = -255.65M,
                        BudgetBucket = StatementModelTestData.SurplusBucket,
                        Date = new DateTime(2013, 9, 10),
                    })
                .AppendTransaction(
                    new Transaction
                    {
                        Account = StatementModelTestData.ChequeAccount,
                        Amount = -1000M,
                        BudgetBucket = new FixedBudgetProjectBucket("FOO", "Bar", 2000M),
                        Date = new DateTime(2013, 9, 9)
                    })
                .Merge(this.testData)
                .Build();
            this.testData = model2;
            this.budgetBucketRepo = new BucketBucketRepoAlwaysFind();
            Arrange();

            var result = this.subject.FilterByBucket(SurplusBucket.SurplusCode);

            this.testData.Output(DateTime.MinValue);
            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public void FilterByBucket_ShouldReturn1Bucket_GivenUncatergorisedCode()
        {
            var model2 = new StatementModelBuilder()
                .AppendTransaction(
                    new Transaction
                    {
                        Account = StatementModelTestData.ChequeAccount,
                        Amount = -255.65M,
                        BudgetBucket = null,
                        Date = new DateTime(2013, 9, 10),
                    })
                .Merge(this.testData)
                .Build();
            this.testData = model2;
            Arrange();

            var result = this.subject.FilterByBucket(TransactionConstants.UncategorisedFilter);

            this.testData.Output(DateTime.MinValue);
            Assert.AreEqual(1, result.Count());
        }

        [TestMethod]
        public void FilterTransactions_ShouldCallStatementModel_GivenFilterObject()
        {
            this.testData = new StatementModelTestHarness();
            this.testData.LoadTransactions(new List<Transaction>());
            var criteria = new GlobalFilterCriteria { BeginDate = new DateTime(2014, 07, 01), EndDate = new DateTime(2014, 08, 01) };

            Arrange();

            this.subject.FilterTransactions(criteria);

            Assert.AreEqual(1, ((StatementModelTestHarness)this.testData).FilterByCriteriaWasCalled);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FilterTransactions_ShouldThrow_GivenNullFilter()
        {
            this.subject.FilterTransactions(null);
            Assert.Fail();
        }

        [TestMethod]
        public async Task ImportAndMergeBankStatement_ShouldCallStatementRepo_GivenStorageKeyAndAccount()
        {
            this.mockStatementRepo
                .Setup(m => m.ImportBankStatementAsync(It.IsAny<string>(), It.IsAny<Engine.BankAccount.Account>()))
                .Returns(Task.FromResult(StatementModelTestData.TestData3()))
                .Verifiable();

            await this.subject.ImportAndMergeBankStatementAsync("Sticky Bag.csv", StatementModelTestData.ChequeAccount);

            this.mockStatementRepo.Verify();
        }

        [TestMethod]
        public async Task ImportAndMergeBankStatement_ShouldMergeTheModel_GivenStorageKeyAndAccount()
        {
            this.testData = new StatementModelTestHarness();
            this.testData.LoadTransactions(new List<Transaction>());

            Arrange();

            this.mockStatementRepo
                .Setup(m => m.ImportBankStatementAsync(It.IsAny<string>(), It.IsAny<Engine.BankAccount.Account>()))
                .Returns(Task.FromResult(StatementModelTestData.TestData2()))
                .Verifiable();

            await this.subject.ImportAndMergeBankStatementAsync("Sticky Bag.csv", StatementModelTestData.ChequeAccount);

            Assert.AreEqual(1, ((StatementModelTestHarness)this.testData).MergeWasCalled);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ImportAndMergeBankStatement_ShouldThrow_GivenNullAccount()
        {
            await this.subject.ImportAndMergeBankStatementAsync("Sticky Bag.csv", null);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(TransactionsAlreadyImportedException))]
        public async Task ImportAndMergeBankStatement_ShouldThrow_GivenAlreadyImported()
        {
            this.testData = new StatementModelBuilder()
                .TestData2()
                .Build();

            Arrange();

            this.mockStatementRepo
                .Setup(m => m.ImportBankStatementAsync(It.IsAny<string>(), It.IsAny<Engine.BankAccount.Account>()))
                .Returns(Task.FromResult(StatementModelTestData.TestData2()))
                .Verifiable();

            await this.subject.ImportAndMergeBankStatementAsync("Sticky Bag.csv", StatementModelTestData.ChequeAccount);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ImportAndMergeBankStatement_ShouldThrow_GivenNullStorageKey()
        {
            await this.subject.ImportAndMergeBankStatementAsync(null, new ChequeAccount("Foo"));
            Assert.Fail();
        }

        [TestMethod]
        public async Task LoadStatementModelAsync_ShouldCallStatementRepo_GivenValidStorageKey()
        {
            await this.subject.LoadAsync(this.testAppDb);
            this.mockStatementRepo.Verify();
        }

        [TestMethod]
        public async Task LoadStatementModelAsync_ShouldReturnAStatementModel_GivenValidStorageKey()
        {
            StatementModel testStatement = new StatementModelTestHarness();
            testStatement.LoadTransactions(new List<Transaction>());

            this.mockStatementRepo.Setup(m => m.LoadAsync(It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult(testStatement));

            await this.subject.LoadAsync(this.testAppDb);

            Assert.IsNotNull(this.subject.StatementModel);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task LoadStatementModelAsync_ShouldThrow_GivenNullStorageKey()
        {
            await this.subject.LoadAsync(null);
            Assert.Fail();
        }

        [TestMethod]
        public void PopulateGroupByBucketCollection_ShouldReturnEmpty_GivenFalse()
        {
            IEnumerable<TransactionGroupedByBucket> result = this.subject.PopulateGroupByBucketCollection(false);

            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public void PopulateGroupByBucketCollection_ShouldReturnEmpty_GivenStatementModelNotLoaded()
        {
            this.subject = CreateSubject();
            IEnumerable<TransactionGroupedByBucket> result = this.subject.PopulateGroupByBucketCollection(true);

            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public void PopulateGroupByBucketCollection_ShouldReturnListOf6_GivenStatementModelAndTrue()
        {
            IEnumerable<TransactionGroupedByBucket> result = this.subject.PopulateGroupByBucketCollection(true);

            Assert.AreEqual(6, result.Count());
        }

        [TestMethod]
        public void PopulateGroupByBucketCollection_ShouldReturnListSortedByBucket_GivenStatementModelAndTrue()
        {
            IEnumerable<TransactionGroupedByBucket> result = this.subject.PopulateGroupByBucketCollection(true);

            TransactionGroupedByBucket previous = null;
            foreach (TransactionGroupedByBucket groupedByBucket in result)
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
        public void RemoveTransaction_ShouldCallStatementModelRemove_GivenATransaction()
        {
            this.testData = new StatementModelTestHarness();
            this.testData.LoadTransactions(StatementModelTestData.TestData2().Transactions);
            Arrange();
            Transaction transaction = this.testData.Transactions.Skip(1).First();

            this.subject.RemoveTransaction(transaction);

            Assert.AreEqual(1, ((StatementModelTestHarness)this.testData).RemoveTransactionWasCalled);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemoveTransaction_ShouldThrow_GivenNullTransaction()
        {
            this.subject.RemoveTransaction(null);

            Assert.Fail();
        }

        [TestMethod]
        public async Task Save_ShouldCallStatementRepo_GivenStatementModel()
        {
            var mockAppDb = new Mock<ApplicationDatabase>();
            mockAppDb.Setup(m => m.FullPath(It.IsAny<string>())).Returns("Foo");
            await this.subject.SaveAsync(mockAppDb.Object);

            this.mockStatementRepo.Verify(m => m.SaveAsync(It.IsAny<StatementModel>(), It.IsAny<bool>()), Times.Once);
        }

        [TestMethod]
        public async Task Save_ShouldNotCallStatementRepo_GivenNullStatementModel()
        {
            this.subject = CreateSubject();
            await this.subject.SaveAsync(It.IsAny<ApplicationDatabase>());

            this.mockStatementRepo.Verify(m => m.SaveAsync(It.IsAny<StatementModel>(), It.IsAny<bool>()), Times.Never);
        }

        [TestMethod]
        public void SplitTransaction_ShouldCallStatementModel_GivenValidParams()
        {
            this.testData = new StatementModelTestHarness();
            this.testData.LoadTransactions(new List<Transaction>());

            var transaction = new Transaction
            {
                Account = StatementModelTestData.VisaAccount
            };

            Arrange();

            this.subject.SplitTransaction(transaction, 100, 200, StatementModelTestData.CarMtcBucket, StatementModelTestData.HairBucket);

            Assert.AreEqual(1, ((StatementModelTestHarness)this.testData).SplitTransactionWasCalled);
        }

        [TestInitialize]
        public void TestInit()
        {
            PrivateAccessor.SetProperty(this.testAppDb, "StatementModelStorageKey", @"Foo.csv");
            PrivateAccessor.SetProperty(this.testAppDb, "FileName", @"C:\AppDb.bax");
            this.testData = StatementModelTestData.TestData2();

            this.mockBudgetBucketRepo = new Mock<IBudgetBucketRepository>();
            this.mockStatementRepo = new Mock<IStatementRepository>();

            this.mockStatementRepo.Setup(m => m.LoadAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(this.testData))
                .Verifiable();

            this.mockBudgetBucketRepo
                .Setup(m => m.Buckets)
                .Returns(BudgetBucketTestData.BudgetModelTestData1Buckets);

            this.budgetBucketRepo = this.mockBudgetBucketRepo.Object;
            Arrange();
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

        private void Arrange()
        {
            this.mockStatementRepo
                .Setup(m => m.LoadAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(this.testData));
            this.subject = CreateSubject();
            this.subject.LoadAsync(this.testAppDb).Wait();
        }

        private TransactionManagerService CreateSubject()
        {
            return new TransactionManagerService(this.budgetBucketRepo, this.mockStatementRepo.Object, new FakeLogger(), new FakeMonitorableDependencies());
        }
    }
}