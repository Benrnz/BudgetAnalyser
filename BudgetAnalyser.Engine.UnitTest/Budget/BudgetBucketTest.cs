using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Budget
{
    [TestClass]
    public class BudgetBucketTest
    {
        private const string NotSpecified = "NotSpecified";

        [TestMethod]
        public void BudgetBucketPropertiesShouldBeMapped()
        {
            var properties = typeof(BudgetBucket).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite);
            Assert.AreEqual(3, properties.Count());
        }

        [TestMethod]
        public void Comparable_HairBucketIsLessThanPower()
        {
            var hairBucket = StatementModelTestData.HairBucket;
            var powerBucket = StatementModelTestData.PowerBucket;

            Assert.IsTrue(hairBucket.CompareTo(powerBucket) < 0);
        }

        [TestMethod]
        public void Comparable_PhoneBucketIsEqualToAnotherInstance()
        {
            var phoneBucket = StatementModelTestData.PhoneBucket;
            var phooneBucket2 = Arrange(TestDataConstants.PhoneBucketCode, "Foo");

            Assert.IsTrue(phoneBucket.CompareTo(phooneBucket2) == 0);
        }

        [TestMethod]
        public void Comparable_PhoneBucketIsGreaterThanHair()
        {
            var hairBucket = StatementModelTestData.HairBucket;
            var phoneBucket = StatementModelTestData.PhoneBucket;

            Assert.IsTrue(phoneBucket.CompareTo(hairBucket) > 0);
        }

        [TestMethod]
        public void CtorShouldAllocateUpperCaseCode()
        {
            var subject = Arrange("Foo", "Bar");
            Assert.AreEqual("FOO", subject.Code);
            Assert.AreNotEqual("Foo", subject.Code);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowWhenCodeIsNull()
        {
            Arrange(NotSpecified, "Something");
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowWhenNameIsNull()
        {
            Arrange("Something", NotSpecified);
            Assert.Fail();
        }

        [TestMethod]
        public void SettingCodeShouldConvertToUpperCase()
        {
            var subject = Arrange("Foo", "Bar");
            subject.Code = "White";
            Assert.AreNotEqual("White", subject.Code);
            Assert.AreEqual("WHITE", subject.Code);
        }

        [TestMethod]
        public void TwoBucketsAreDifferentIfCodesAreDifferent()
        {
            var subject1 = Arrange("Foo1", "Name");
            var subject2 = Arrange("Foo2", "Name");
            Assert.AreNotEqual(subject1, subject2);
            Assert.IsTrue(subject1 != subject2);
            Assert.AreNotEqual(subject1.GetHashCode(), subject2.GetHashCode());
        }

        [TestMethod]
        public void TwoBucketsAreTheSameIfCodesAreEqual()
        {
            var subject1 = Arrange("Foo", "Name");
            var subject2 = Arrange("Foo", "Name");
            Assert.AreEqual(subject1, subject2);
            Assert.IsTrue(subject1 == subject2);
            Assert.AreEqual(subject1.GetHashCode(), subject2.GetHashCode());
        }

        [TestMethod]
        public void TwoReferencesToDifferentObjectsAreNotEqual()
        {
            var subject1 = Arrange("Foo", "Name");
            var subject2 = Arrange("Ben", "Is Awesome");
            Assert.AreNotEqual(subject1, subject2);
            Assert.IsTrue(subject1 != subject2);
            Assert.AreNotEqual(subject1.GetHashCode(), subject2.GetHashCode());
        }

        [TestMethod]
        public void TwoReferencesToTheSameObjectAreEqual()
        {
            var subject1 = Arrange("Foo", "Name");
            var subject2 = subject1;
            Assert.AreEqual(subject1, subject2);
            Assert.IsTrue(subject1 == subject2);
            Assert.AreEqual(subject1.GetHashCode(), subject2.GetHashCode());
        }

        [TestMethod]
        public void ValidateShouldRetrunFalseGivenLongCode()
        {
            var subject = Arrange();
            subject.Code = "ABC345678";
            var result = subject.Validate(new StringBuilder());
            Assert.IsFalse(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateShouldThrowGivenNullStringBuilder()
        {
            var subject = Arrange();
            subject.Validate(null);
            Assert.Fail();
        }

        [TestMethod]
        public void ValidateWillReturnFalseWhenCodeIsNull()
        {
            var subject = Arrange();
            subject.Description = "Foo bar";
            var builder = new StringBuilder();
            Assert.IsFalse(subject.Validate(builder));
            Assert.IsTrue(builder.Length > 0);
        }

        [TestMethod]
        public void ValidateWillReturnFalseWhenCodeIsTooLong()
        {
            var subject = Arrange();
            subject.Description = "FooBarHo";
            var builder = new StringBuilder();
            Assert.IsFalse(subject.Validate(builder));
            Assert.IsTrue(builder.Length > 0);
        }

        [TestMethod]
        public void ValidateWillReturnFalseWhenNameIsNull()
        {
            var subject = Arrange();
            subject.Code = "Foo";
            var builder = new StringBuilder();
            Assert.IsFalse(subject.Validate(builder));
            Assert.IsTrue(builder.Length > 0);
        }

        private BudgetBucket Arrange(string code = NotSpecified, string name = NotSpecified)
        {
            if (code == NotSpecified && name == NotSpecified)
            {
                return new BudgetBucketTestHarness();
            }

            var code2 = code == NotSpecified ? null : code;
            var name2 = name == NotSpecified ? null : name;
            return new BudgetBucketTestHarness(code2, name2);
        }
    }
}
