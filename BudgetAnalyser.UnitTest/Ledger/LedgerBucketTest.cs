using System.Collections.Generic;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Ledger
{
    [TestClass]
    public class LedgerBucketTest
    {
        [TestMethod]
        public void DictionaryTest()
        {
            var instance1 = new LedgerBucket
            {
                BudgetBucket = StatementModelTestData.HairBucket,
                StoredInAccount = new ChequeAccount("Foo1")
            };

            var dictionary = new Dictionary<LedgerBucket, LedgerBucket>
            {
                { instance1, instance1 }
            };

            var instance2 = new LedgerBucket
            {
                BudgetBucket = StatementModelTestData.HairBucket,
                StoredInAccount = new ChequeAccount("Foo1")
            };

            Assert.IsTrue(dictionary.ContainsKey(instance2));

            var instance3 = new LedgerBucket
            {
                BudgetBucket = new SpentMonthlyExpenseBucket("HAIRCUT", "Foo bar"),
                StoredInAccount = new ChequeAccount("Foo1")
            };

            Assert.IsTrue(dictionary.ContainsKey(instance3));
        }

        [TestMethod]
        public void TwoInstancesWithEquivelantBucketAndAccountAreEqual()
        {
            var bucket1 = new SpentMonthlyExpenseBucket("Foo1", "Foo bar");
            var bucket2 = new SpentMonthlyExpenseBucket("Foo1", "Foo bar");

            var instance1 = new LedgerBucket
            {
                BudgetBucket = bucket1,
                StoredInAccount = StatementModelTestData.SavingsAccount
            };
            var instance2 = new LedgerBucket
            {
                BudgetBucket = bucket2,
                StoredInAccount = StatementModelTestData.SavingsAccount
            };
            Assert.AreEqual(instance1, instance2);
            Assert.IsTrue(instance1 == instance2);
        }

        [TestMethod]
        public void TwoInstancesWithSameBucketAndAccountAreEqual()
        {
            var instance1 = new LedgerBucket
            {
                BudgetBucket = StatementModelTestData.CarMtcBucket,
                StoredInAccount = StatementModelTestData.SavingsAccount
            };
            var instance2 = new LedgerBucket
            {
                BudgetBucket = StatementModelTestData.CarMtcBucket,
                StoredInAccount = StatementModelTestData.SavingsAccount
            };
            Assert.AreEqual(instance1, instance2);
            Assert.IsTrue(instance1 == instance2);
        }

        [TestMethod]
        public void TwoInstancesWithSameBucketAndEquivelantAccountAreEqual()
        {
            var account1 = new SavingsAccount("Foo");
            var account2 = new SavingsAccount("Foo");

            var instance1 = new LedgerBucket
            {
                BudgetBucket = StatementModelTestData.CarMtcBucket,
                StoredInAccount = account1
            };
            var instance2 = new LedgerBucket
            {
                BudgetBucket = StatementModelTestData.CarMtcBucket,
                StoredInAccount = account2
            };
            Assert.AreEqual(instance1, instance2);
            Assert.IsTrue(instance1 == instance2);
        }

        [TestMethod]
        public void TwoInstancesWithSameBucketAndNullAccountAreEqual()
        {
            var instance1 = new LedgerBucket
            {
                BudgetBucket = StatementModelTestData.CarMtcBucket,
                StoredInAccount = null
            };
            var instance2 = new LedgerBucket
            {
                BudgetBucket = StatementModelTestData.CarMtcBucket,
                StoredInAccount = null
            };
            Assert.AreEqual(instance1, instance2);
            Assert.IsTrue(instance1 == instance2);
        }

        [TestMethod]
        public void TwoInstancesWithSameBucketAndNullAccountHaveTheSameHashCode()
        {
            var instance1 = new LedgerBucket
            {
                BudgetBucket = StatementModelTestData.CarMtcBucket,
                StoredInAccount = null
            };
            var instance2 = new LedgerBucket
            {
                BudgetBucket = StatementModelTestData.CarMtcBucket,
                StoredInAccount = null
            };
            Assert.AreEqual(instance1.GetHashCode(), instance2.GetHashCode());
        }
    }
}