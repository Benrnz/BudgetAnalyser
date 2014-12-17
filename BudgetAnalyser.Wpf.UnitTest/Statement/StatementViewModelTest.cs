using System;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Statement;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;

namespace BudgetAnalyser.Wpf.UnitTest.Statement
{
    [TestClass]
    public class StatementViewModelTest
    {
        private Mock<ITransactionManagerService> mockTransactionService;
        private StatementController FakeStatetmentController { get; set; }
        private Mock<IBudgetBucketRepository> MockBucketRepo { get; set; }
        private Mock<IUiContext> MockUiContext { get; set; }

        [TestMethod]
        public void BudgetBucketsShouldIncludeBlank()
        {
            var subject = Arrange();
            Assert.IsTrue(subject.FilterBudgetBuckets.Any(string.IsNullOrWhiteSpace));
        }

        [TestMethod]
        public void EditingTransactionFromFullListShouldSyncWithGroupedList()
        {
            var subject = Arrange2();
            subject.SortByBucket = true;
            subject.UpdateGroupedByBucket();

            var transactionFromFullList = GetPhoneTxnFromFullList(subject);
            var transactionFromGroupedList = GetPhoneTxnFromGroupedList(subject);

            transactionFromFullList.Amount = -999.99M;

            Assert.AreEqual(-999.99M, transactionFromGroupedList.Amount);
        }

        [TestMethod]
        public void EditingTransactionFromGroupedListShouldSyncWithFullList()
        {
            var subject = Arrange2();
            subject.SortByBucket = true;
            subject.UpdateGroupedByBucket();

            var transactionFromFullList = GetPhoneTxnFromFullList(subject);
            var transactionFromGroupedList = GetPhoneTxnFromGroupedList(subject);

            transactionFromGroupedList.Amount = -999.99M;

            Assert.AreEqual(-999.99M, transactionFromFullList.Amount);
        }

        [TestMethod]
        public void FilterBudgetBucketsShouldIncludeUncategorisedItem()
        {
            var subject = Arrange();
            Assert.IsTrue(subject.FilterBudgetBuckets.Any(b => b == TransactionManagerService.UncategorisedFilter));
        }

        [TestMethod]
        public void GivenNoDataHasTransactionsShouldBeFalse()
        {
            var subject = new StatementViewModel(this.mockTransactionService.Object);
            Assert.IsFalse(subject.HasTransactions);
        }

        [TestMethod]
        public void GivenNoDataStatementNameShouldBeNoTransactionsLoaded()
        {
            var subject = new StatementViewModel(this.mockTransactionService.Object);
            Assert.AreEqual("[No Transactions Loaded]", subject.StatementName);
        }

        [TestMethod]
        public void GivenSortByBucketSortByDateShouldBeFalse()
        {
            var subject = Arrange();
            subject.SortByBucket = true;
            Assert.IsFalse(subject.SortByDate);
        }

        [TestMethod]
        public void GivenSortByBucketUpdateGroupedByBucketShouldUpdateGroupedList()
        {
            var subject = Arrange();
            subject.SortByDate = true;
            subject.SortByBucket = true;
            subject.UpdateGroupedByBucket();
            Assert.IsTrue(subject.GroupedByBucket.Any());
        }

        [TestMethod]
        public void GivenSortByDateSortByBucketShouldBeFalse()
        {
            var subject = Arrange();
            subject.SortByDate = true;
            Assert.IsFalse(subject.SortByBucket);
        }

        [TestMethod]
        public void GivenSortByDateUpdateGroupedByBucketShouldNotUpdateGroupedList()
        {
            var subject = Arrange();
            subject.SortByBucket = true;
            subject.SortByDate = true;
            subject.UpdateGroupedByBucket();
            Assert.IsFalse(subject.GroupedByBucket.Any());
        }

        [TestMethod]
        public void GivenTestData1AverageDebitShouldBe115()
        {
            var subject = Arrange();
            Assert.AreEqual(-115.25M, decimal.Round(subject.AverageDebit, 2));
        }

        [TestMethod]
        public void GivenTestData1BucketFilterShouldBeNullByDefault()
        {
            var subject = Arrange();
            Assert.IsNull(subject.BucketFilter);
        }

        [TestMethod]
        public void GivenTestData1HasTransactionsShouldBeTrue()
        {
            var subject = Arrange();
            Assert.IsTrue(subject.HasTransactions);
        }

        [TestMethod]
        public void GivenTestData1MinDateShouldBe01Sep13()
        {
            var subject = Arrange();
            Assert.AreEqual(new DateTime(2013, 09, 1), subject.MaxTransactionDate);
        }

        [TestMethod]
        public void GivenTestData1MinDateShouldBe20Jul13()
        {
            var subject = Arrange();
            Assert.AreEqual(new DateTime(2013, 07, 15), subject.MinTransactionDate);
        }

        [TestMethod]
        public void GivenTestData1StatementNameShouldBeFooStatement()
        {
            var subject = Arrange();
            Assert.AreEqual("FooStatement", subject.StatementName);
        }

        [TestMethod]
        public void GivenTestData1TotalCountShouldBe7()
        {
            var subject = Arrange();
            Assert.AreEqual(7, subject.TotalCount);
        }

        [TestMethod]
        public void GivenTestData1TotalCreditsShouldBe0()
        {
            var subject = Arrange();
            Assert.AreEqual(0, subject.TotalCredits);
        }

        [TestMethod]
        public void GivenTestData1TotalDebitsShouldBe806()
        {
            var subject = Arrange();
            Assert.AreEqual(-806.78M, subject.TotalDebits);
        }

        [TestMethod]
        public void GivenTestData2OutputGroupedList()
        {
            var subject = Arrange2();
            subject.SortByBucket = true;
            subject.UpdateGroupedByBucket();

            foreach (TransactionGroupedByBucket group in subject.GroupedByBucket)
            {
                Console.WriteLine(
                    "{0}, AvgDr:{1:C} {2:d} {3:d} Count:{4} TotalCr:{5:C} TotalDr{6:C} Diff{7:C}",
                    group.Bucket,
                    group.AverageDebit,
                    group.MinTransactionDate,
                    group.MaxTransactionDate,
                    group.TotalCount,
                    group.TotalCredits,
                    group.TotalDebits,
                    group.TotalDifference);
                foreach (var transaction in group.Transactions)
                {
                    Console.WriteLine(
                        "     {0:d} {1:C} {2}",
                        transaction.Date,
                        transaction.Amount,
                        transaction.Description);
                }
            }
        }

        [TestMethod]
        public void GivenTestData2TotalCreditsShouldBe2552()
        {
            var subject = Arrange2();
            Assert.AreEqual(2552.97M, subject.TotalCredits);
        }

        [TestMethod]
        public void GivenTestData2TotalDifferenceShouldBe1746()
        {
            var subject = Arrange2();
            Assert.AreEqual(1746.19M, subject.TotalDifference);
        }

        [TestMethod]
        public void RemovingTransactionShouldDeleteFromBothLists()
        {
            var subject = Arrange2();
            subject.SortByBucket = true;
            subject.UpdateGroupedByBucket();

            var transactionFromFullList = GetPhoneTxnFromFullList(subject);
            var count = subject.Statement.Transactions.Count();

            subject.Statement.RemoveTransaction(transactionFromFullList);

            Assert.AreEqual(count - 1, subject.Statement.Transactions.Count());
            Assert.AreEqual(count - 1, subject.GroupedByBucket.SelectMany(g => g.Transactions).Count());
        }

        [TestMethod]
        public void SortByDateShouldBeDefaultSort()
        {
            var subject = Arrange();
            Assert.IsTrue(subject.SortByDate);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            MockBucketRepo = new Mock<IBudgetBucketRepository>();

            MockUiContext = new Mock<IUiContext>();
            // Todo Need message, yesnobox, waitcursorfactory, backgroundjob, appliedrulescontroller

            this.mockTransactionService = new Mock<ITransactionManagerService>();

            FakeStatetmentController = new StatementController(
                MockUiContext.Object,
                new StatementControllerFileOperations(
                    MockUiContext.Object,
                    new Mock<IStatementRepository>().Object,
                    new Mock<IRecentFileManager>().Object,
                    new DemoFileHelper(),
                    new Mock<LoadFileController>().Object,
                    MockBucketRepo.Object,
                    this.mockTransactionService.Object
                    ),
                this.mockTransactionService.Object
                );
        }

        [TestMethod]
        public void TriggerRefreshTotalsRowShouldRaise10Events()
        {
            var subject = Arrange();
            var eventCount = 0;
            subject.PropertyChanged += (s, e) => eventCount++;
            subject.TriggerRefreshTotalsRow();

            Assert.AreEqual(10, eventCount);
        }

        private StatementViewModel Arrange()
        {
            return new StatementViewModel(this.mockTransactionService.Object)
            {
                Statement = StatementModelTestData.TestData1()
            }.Initialise(FakeStatetmentController);
        }

        private StatementViewModel Arrange2()
        {
            return new StatementViewModel(this.mockTransactionService.Object)
            {
                Statement = StatementModelTestData.TestData2()
            }.Initialise(FakeStatetmentController);
        }

        private static Transaction GetPhoneTxnFromFullList(StatementViewModel subject)
        {
            var transactionFromFullList = subject.Statement.Transactions
                .Single(t => t.BudgetBucket == StatementModelTestData.PhoneBucket && t.Date == new DateTime(2013, 07, 16));
            return transactionFromFullList;
        }

        private static Transaction GetPhoneTxnFromGroupedList(StatementViewModel subject)
        {
            var transactionFromGroupedList = subject.GroupedByBucket.Single(g => g.Bucket == StatementModelTestData.PhoneBucket)
                .Transactions.Single(t => t.Date == new DateTime(2013, 07, 16));
            return transactionFromGroupedList;
        }

        private static IWaitCursor WaitCursorFactory()
        {
            return new Mock<IWaitCursor>().Object;
        }
    }
}