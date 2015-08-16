using System;
using System.Linq;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Statement
{
    [TestClass]
    public class TransactionGroupedByBucketTest
    {
        [TestMethod]
        public void GivenStatementTestData1AverageDebitShouldBe92()
        {
            TransactionGroupedByBucket subject = Arrange();
            Assert.AreEqual(-92.15M, subject.AverageDebit);
        }

        [TestMethod]
        public void GivenStatementTestData1BucketShouldBePowerBucket()
        {
            TransactionGroupedByBucket subject = Arrange();
            Assert.AreEqual(StatementModelTestData.PowerBucket, subject.Bucket);
        }

        [TestMethod]
        public void GivenStatementTestData1HasTransactionsShouldBeTrue()
        {
            TransactionGroupedByBucket subject = Arrange();
            Assert.IsTrue(subject.HasTransactions);
        }

        [TestMethod]
        public void GivenStatementTestData1MaxTransactionDateShouldBe15Aug13()
        {
            TransactionGroupedByBucket subject = Arrange();
            Assert.AreEqual(new DateTime(2013, 08, 15), subject.MaxTransactionDate);
        }

        [TestMethod]
        public void GivenStatementTestData1MinTransactionDateShouldBe15Jul13()
        {
            TransactionGroupedByBucket subject = Arrange();
            Assert.AreEqual(new DateTime(2013, 07, 15), subject.MinTransactionDate);
        }

        [TestMethod]
        public void GivenStatementTestData1TotalCountShouldBe2()
        {
            TransactionGroupedByBucket subject = Arrange();
            Assert.AreEqual(2, subject.TotalCount);
        }

        [TestMethod]
        public void GivenStatementTestData1TotalCreditsShouldBe0()
        {
            TransactionGroupedByBucket subject = Arrange();
            Assert.AreEqual(0M, subject.TotalCredits);
        }

        [TestMethod]
        public void GivenStatementTestData1TotalDebitsShouldBe184()
        {
            TransactionGroupedByBucket subject = Arrange();
            Assert.AreEqual(-184.30M, subject.TotalDebits);
        }

        [TestMethod]
        public void GivenStatementTestData1TotalDifferenceShouldBe184()
        {
            TransactionGroupedByBucket subject = Arrange();
            Assert.AreEqual(-184.30M, subject.TotalDifference);
        }

        [TestMethod]
        public void GivenStatementTestData1TransactionsShouldNotBeNull()
        {
            TransactionGroupedByBucket subject = Arrange();
            Assert.AreEqual(2, subject.Transactions.Count());
        }

        [TestMethod]
        public void GivenStatementTestData1TransactionsShouldOnlyBePowerBucket()
        {
            TransactionGroupedByBucket subject = Arrange();
            Assert.IsFalse(subject.Transactions.Any(t => t.BudgetBucket != StatementModelTestData.PowerBucket));
        }

        [TestMethod]
        public void TriggerRefreshOfTotalsRowShouldRaise8Events()
        {
            TransactionGroupedByBucket subject = Arrange();
            var eventCount = 0;
            subject.PropertyChanged += (s, e) => eventCount++;
            subject.TriggerRefreshTotalsRow();
            Assert.AreEqual(8, eventCount);
        }

        private TransactionGroupedByBucket Arrange()
        {
            StatementModel statementModel = StatementModelTestData.TestData1();
            return new TransactionGroupedByBucket(statementModel.AllTransactions, StatementModelTestData.PowerBucket);
        }
    }
}