using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Budget
{
    [TestClass]
    public class BudgetItemTest
    {
        [TestMethod]
        public void ShouldHaveAKnownNumberOfProperties()
        {
            var properties = typeof(BudgetItem).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite);
            Assert.AreEqual(2, properties.Count());
        }

        [TestMethod]
        public void TwoBudgetItemsAreDifferentIfCodesAreDifferent()
        {
            var subject1 = CreateSubject1();
            var subject2 = CreateSubject2();

            Assert.AreNotEqual(subject1, subject2);
            Assert.IsTrue(subject1 != subject2);
            Assert.AreNotEqual(subject1.GetHashCode(), subject2.GetHashCode());
        }

        [TestMethod]
        public void TwoBudgetItemsAreDifferentIfCodesAreEqualButDifferentTypes()
        {
            var subject1 = CreateSubject1();
            var subject3 = CreateSubject3();

            Assert.AreNotEqual(subject1, subject3);
            Assert.IsTrue(subject1 != subject3);
            Assert.AreNotEqual(subject1.GetHashCode(), subject3.GetHashCode());
        }

        [TestMethod]
        public void TwoBudgetItemsAreTheSameIfCodesAreEqualAndSameType()
        {
            var subject1 = CreateSubject1();
            var subject2 = CreateSubject1();

            Assert.AreEqual(subject1, subject2);
            Assert.IsTrue(subject1 == subject2);
            Assert.AreEqual(subject1.GetHashCode(), subject2.GetHashCode());
        }

        [TestMethod]
        public void TwoReferencesToTheSameObjectAreEqual()
        {
            var subject1 = CreateSubject1();
            var subject2 = subject1;

            Assert.AreEqual(subject1, subject2);
            Assert.IsTrue(subject1 == subject2);
            Assert.AreEqual(subject1.GetHashCode(), subject2.GetHashCode());
        }

        private BudgetBucket CreateBucket1()
        {
            return new SavedUpForExpenseBucket(TestDataConstants.CarMtcBucketCode, "Foo bar");
        }

        private BudgetBucket CreateBucket2()
        {
            return new SavedUpForExpenseBucket(TestDataConstants.PowerBucketCode, "Foo bar");
        }

        private BudgetBucket CreateBucket3()
        {
            return new IncomeBudgetBucket(TestDataConstants.CarMtcBucketCode, "Foo bar");
        }

        private BudgetItem CreateSubject1()
        {
            return new Expense { Amount = 0.01M, Bucket = CreateBucket1() };
        }

        private BudgetItem CreateSubject2()
        {
            return new Expense { Amount = 0.01M, Bucket = CreateBucket2() };
        }

        private BudgetItem CreateSubject3()
        {
            return new Income { Amount = 0.01M, Bucket = CreateBucket3() };
        }
    }
}
