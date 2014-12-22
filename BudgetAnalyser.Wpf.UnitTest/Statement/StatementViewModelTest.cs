using System;
using System.Linq;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Statement;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser.Wpf.UnitTest.Statement
{
    [TestClass]
    public class StatementViewModelTest
    {
        private Mock<ITransactionManagerService> mockTransactionService;
        private Mock<IUiContext> mockUiContext;

        [TestMethod]
        public void GivenNoDataHasTransactionsShouldBeFalse()
        {
            var subject = CreateSubject();
            Assert.IsFalse(subject.HasTransactions);
        }

        [TestMethod]
        public void GivenNoDataStatementNameShouldBeNoTransactionsLoaded()
        {
            var subject = CreateSubject();
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
        public void GivenTestData1HasTransactionsShouldBeTrue()
        {
            var subject = Arrange();
            Assert.IsTrue(subject.HasTransactions);
        }

        [TestMethod]
        public void GivenTestData1StatementNameShouldBeFooStatement()
        {
            var subject = Arrange();
            Assert.AreEqual("FooStatement", subject.StatementName);
        }

        [TestMethod]
        public void GivenTestData1TotalCreditsShouldBe0()
        {
            var subject = Arrange();
            Assert.AreEqual(0, subject.TotalCredits);
        }

        [TestMethod]
        public void GivenTestData2OutputGroupedList()
        {
            var subject = Arrange();
            subject.SortByBucket = true;
            subject.UpdateGroupedByBucket();

            foreach (TransactionGroupedByBucketViewModel group in subject.GroupedByBucket)
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
        [ExpectedException(typeof(InvalidOperationException))]
        public void SetStatement_ShouldThrow_GivenInitialiseHasNotBeenCalled()
        {
            var subject = CreateSubject();
            subject.Statement = StatementModelTestData.TestData1();
            Assert.Fail();
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
            this.mockUiContext = new Mock<IUiContext>();
            this.mockTransactionService = new Mock<ITransactionManagerService>();
            this.mockTransactionService.Setup(m => m.DetectDuplicateTransactions()).Returns(string.Empty);
        }

        [TestMethod]
        public void TriggerRefreshTotalsRowShouldRaise10Events()
        {
            var subject = Arrange();
            var eventCount = 0;
            subject.PropertyChanged += (s, e) => eventCount++;
            subject.TriggerRefreshTotalsRow();

            Assert.AreEqual(8, eventCount);
        }

        private StatementViewModel Arrange()
        {
            var subject = CreateSubject().Initialise(this.mockTransactionService.Object);
            subject.Statement = StatementModelTestData.TestData1();
            return subject;
        }

        private StatementViewModel CreateSubject()
        {
            return new StatementViewModel(this.mockUiContext.Object);
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