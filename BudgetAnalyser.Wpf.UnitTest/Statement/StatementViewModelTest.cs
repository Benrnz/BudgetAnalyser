using System;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Statement;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BudgetAnalyser.Wpf.UnitTest.Statement
{
    [TestClass]
    public class StatementViewModelTest
    {
        [TestMethod]
        public void BudgetBucketsShouldIncludeBlank()
        {
            StatementViewModel subject = Arrange();
            Assert.IsTrue(subject.BudgetBuckets.Any(string.IsNullOrWhiteSpace));
        }

        [TestMethod]
        public void EditingTransactionFromFullListShouldSyncWithGroupedList()
        {
            StatementViewModel subject = Arrange2();
            subject.SortByBucket = true;
            subject.UpdateGroupedByBucket();

            Transaction transactionFromFullList = GetPhoneTxnFromFullList(subject);
            Transaction transactionFromGroupedList = GetPhoneTxnFromGroupedList(subject);

            transactionFromFullList.Amount = -999.99M;

            Assert.AreEqual(-999.99M, transactionFromGroupedList.Amount);
        }

        [TestMethod]
        public void EditingTransactionFromGroupedListShouldSyncWithFullList()
        {
            StatementViewModel subject = Arrange2();
            subject.SortByBucket = true;
            subject.UpdateGroupedByBucket();

            Transaction transactionFromFullList = GetPhoneTxnFromFullList(subject);
            Transaction transactionFromGroupedList = GetPhoneTxnFromGroupedList(subject);

            transactionFromGroupedList.Amount = -999.99M;

            Assert.AreEqual(-999.99M, transactionFromFullList.Amount);
        }

        [TestMethod]
        public void FilterBudgetBucketsShouldIncludeUncategorisedItem()
        {
            StatementViewModel subject = Arrange();
            Assert.IsTrue(subject.FilterBudgetBuckets.Any(b => b == StatementViewModel.UncategorisedFilter));
        }

        [TestMethod]
        public void GivenNoDataHasTransactionsShouldBeFalse()
        {
            var subject = new StatementViewModel(new Mock<IBudgetBucketRepository>().Object);
            Assert.IsFalse(subject.HasTransactions);
        }

        [TestMethod]
        public void GivenNoDataStatementNameShouldBeNoTransactionsLoaded()
        {
            var subject = new StatementViewModel(new Mock<IBudgetBucketRepository>().Object);
            Assert.AreEqual("[No Transactions Loaded]", subject.StatementName);
        }

        [TestMethod]
        public void GivenSortByBucketSortByDateShouldBeFalse()
        {
            StatementViewModel subject = Arrange();
            subject.SortByBucket = true;
            Assert.IsFalse(subject.SortByDate);
        }

        [TestMethod]
        public void GivenSortByBucketUpdateGroupedByBucketShouldUpdateGroupedList()
        {
            StatementViewModel subject = Arrange();
            subject.SortByDate = true;
            subject.SortByBucket = true;
            subject.UpdateGroupedByBucket();
            Assert.IsTrue(subject.GroupedByBucket.Any());
        }

        [TestMethod]
        public void GivenSortByDateSortByBucketShouldBeFalse()
        {
            StatementViewModel subject = Arrange();
            subject.SortByDate = true;
            Assert.IsFalse(subject.SortByBucket);
        }

        [TestMethod]
        public void GivenSortByDateUpdateGroupedByBucketShouldNotUpdateGroupedList()
        {
            StatementViewModel subject = Arrange();
            subject.SortByBucket = true;
            subject.SortByDate = true;
            subject.UpdateGroupedByBucket();
            Assert.IsFalse(subject.GroupedByBucket.Any());
        }

        [TestMethod]
        public void GivenTestData1AverageDebitShouldBe115()
        {
            StatementViewModel subject = Arrange();
            Assert.AreEqual(-115.25M, decimal.Round(subject.AverageDebit, 2));
        }

        [TestMethod]
        public void GivenTestData1BucketFilterShouldBeNullByDefault()
        {
            StatementViewModel subject = Arrange();
            Assert.IsNull(subject.BucketFilter);
        }

        [TestMethod]
        public void GivenTestData1HasTransactionsShouldBeTrue()
        {
            StatementViewModel subject = Arrange();
            Assert.IsTrue(subject.HasTransactions);
        }

        [TestMethod]
        public void GivenTestData1MinDateShouldBe01Sep13()
        {
            StatementViewModel subject = Arrange();
            Assert.AreEqual(new DateTime(2013, 09, 1), subject.MaxTransactionDate);
        }

        [TestMethod]
        public void GivenTestData1MinDateShouldBe20Jul13()
        {
            StatementViewModel subject = Arrange();
            Assert.AreEqual(new DateTime(2013, 07, 15), subject.MinTransactionDate);
        }

        [TestMethod]
        public void GivenTestData1StatementNameShouldBeFooStatement()
        {
            StatementViewModel subject = Arrange();
            Assert.AreEqual("FooStatement", subject.StatementName);
        }

        [TestMethod]
        public void GivenTestData1TotalCountShouldBe7()
        {
            StatementViewModel subject = Arrange();
            Assert.AreEqual(7, subject.TotalCount);
        }

        [TestMethod]
        public void GivenTestData1TotalCreditsShouldBe0()
        {
            StatementViewModel subject = Arrange();
            Assert.AreEqual(0, subject.TotalCredits);
        }

        [TestMethod]
        public void GivenTestData1TotalDebitsShouldBe806()
        {
            StatementViewModel subject = Arrange();
            Assert.AreEqual(-806.78M, subject.TotalDebits);
        }

        [TestMethod]
        public void GivenTestData2OutputGroupedList()
        {
            StatementViewModel subject = Arrange2();
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
                foreach (Transaction transaction in group.Transactions)
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
            StatementViewModel subject = Arrange2();
            Assert.AreEqual(2552.97M, subject.TotalCredits);
        }

        [TestMethod]
        public void GivenTestData2TotalDifferenceShouldBe1746()
        {
            StatementViewModel subject = Arrange2();
            Assert.AreEqual(1746.19M, subject.TotalDifference);
        }

        [TestMethod]
        public void RemovingTransactionShouldDeleteFromBothLists()
        {
            StatementViewModel subject = Arrange2();
            subject.SortByBucket = true;
            subject.UpdateGroupedByBucket();

            Transaction transactionFromFullList = GetPhoneTxnFromFullList(subject);
            int count = subject.Statement.Transactions.Count();

            subject.Statement.RemoveTransaction(transactionFromFullList);

            Assert.AreEqual(count - 1, subject.Statement.Transactions.Count());
            Assert.AreEqual(count - 1, subject.GroupedByBucket.SelectMany(g => g.Transactions).Count());
        }

        [TestMethod]
        public void SortByDateShouldBeDefaultSort()
        {
            StatementViewModel subject = Arrange();
            Assert.IsTrue(subject.SortByDate);
        }

        [TestMethod]
        public void TriggerRefreshTotalsRowShouldRaise10Events()
        {
            StatementViewModel subject = Arrange();
            int eventCount = 0;
            subject.PropertyChanged += (s, e) => eventCount++;
            subject.TriggerRefreshTotalsRow();

            Assert.AreEqual(10, eventCount);
        }

        private static Transaction GetPhoneTxnFromFullList(StatementViewModel subject)
        {
            Transaction transactionFromFullList = subject.Statement.Transactions
                .Single(t => t.BudgetBucket == StatementModelTestData.PhoneBucket && t.Date == new DateTime(2013, 07, 16));
            return transactionFromFullList;
        }

        private static Transaction GetPhoneTxnFromGroupedList(StatementViewModel subject)
        {
            Transaction transactionFromGroupedList = subject.GroupedByBucket.Single(g => g.Bucket == StatementModelTestData.PhoneBucket)
                .Transactions.Single(t => t.Date == new DateTime(2013, 07, 16));
            return transactionFromGroupedList;
        }

        private StatementViewModel Arrange()
        {
            var bucketRepositoryMock = new Mock<IBudgetBucketRepository>();

            return new StatementViewModel(bucketRepositoryMock.Object)
            {
                Statement = StatementModelTestData.TestData1(),
            };
        }

        private StatementViewModel Arrange2()
        {
            var bucketRepositoryMock = new Mock<IBudgetBucketRepository>();

            return new StatementViewModel(bucketRepositoryMock.Object)
            {
                Statement = StatementModelTestData.TestData2(),
            };
        }
    }
}