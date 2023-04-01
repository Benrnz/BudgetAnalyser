using System.Collections.Generic;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Ledger
{
    [TestClass]
    public class LedgerBucketTest
    {
        [TestMethod]
        public void DictionaryTest()
        {
            LedgerBucket instance1 = LedgerBookTestData.HairLedger;

            var dictionary = new Dictionary<LedgerBucket, LedgerBucket>
            {
                { instance1, instance1 }
            };

            LedgerBucket instance2 = new SavedUpForLedger
            {
                BudgetBucket = StatementModelTestData.HairBucket,
                StoredInAccount = LedgerBookTestData.ChequeAccount
            };

            // Should already contain this, its the same code.
            Assert.IsTrue(dictionary.ContainsKey(instance2));

            LedgerBucket instance3 = new SavedUpForLedger
            {
                BudgetBucket = new SavedUpForExpenseBucket("HAIRCUT", "Foo bar"),
                StoredInAccount = LedgerBookTestData.ChequeAccount
            };

            Assert.IsTrue(dictionary.ContainsKey(instance3));
        }

        [TestMethod]
        public void TwoInstancesWithEquivelantBucketAndAccountAreEqual()
        {
            var bucket1 = new SpentPerPeriodExpenseBucket("Foo1", "Foo bar");
            var bucket2 = new SpentPerPeriodExpenseBucket("Foo1", "Foo bar");

            var instance1 = new SpentPerPeriodLedger
            {
                BudgetBucket = bucket1,
                StoredInAccount = StatementModelTestData.SavingsAccount
            };
            var instance2 = new SpentPerPeriodLedger
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
            var instance1 = new SavedUpForLedger
            {
                BudgetBucket = StatementModelTestData.CarMtcBucket,
                StoredInAccount = StatementModelTestData.SavingsAccount
            };
            var instance2 = new SavedUpForLedger
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

            var instance1 = new SavedUpForLedger
            {
                BudgetBucket = StatementModelTestData.CarMtcBucket,
                StoredInAccount = account1
            };
            var instance2 = new SavedUpForLedger
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
            var instance1 = new SavedUpForLedger
            {
                BudgetBucket = StatementModelTestData.CarMtcBucket,
                StoredInAccount = null
            };
            var instance2 = new SavedUpForLedger
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
            var instance1 = new SavedUpForLedger
            {
                BudgetBucket = StatementModelTestData.CarMtcBucket,
                StoredInAccount = null
            };
            var instance2 = new SavedUpForLedger
            {
                BudgetBucket = StatementModelTestData.CarMtcBucket,
                StoredInAccount = null
            };
            Assert.AreEqual(instance1.GetHashCode(), instance2.GetHashCode());
        }
    }
}