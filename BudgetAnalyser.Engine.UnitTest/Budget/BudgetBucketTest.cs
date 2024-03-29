﻿using System;
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
            IEnumerable<PropertyInfo> properties = typeof(BudgetBucket).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite);
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
            SpentPerPeriodExpenseBucket phoneBucket = StatementModelTestData.PhoneBucket;
            BudgetBucket phooneBucket2 = Arrange(TestDataConstants.PhoneBucketCode, "Foo");

            Assert.IsTrue(phoneBucket.CompareTo(phooneBucket2) == 0);
        }

        [TestMethod]
        public void Comparable_PhoneBucketIsGreaterThanHair()
        {
            var hairBucket = StatementModelTestData.HairBucket;
            SpentPerPeriodExpenseBucket phoneBucket = StatementModelTestData.PhoneBucket;

            Assert.IsTrue(phoneBucket.CompareTo(hairBucket) > 0);
        }

        [TestMethod]
        public void CtorShouldAllocateUpperCaseCode()
        {
            BudgetBucket subject = Arrange("Foo", "Bar");
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
            BudgetBucket subject = Arrange("Foo", "Bar");
            subject.Code = "White";
            Assert.AreNotEqual("White", subject.Code);
            Assert.AreEqual("WHITE", subject.Code);
        }

        [TestMethod]
        public void TwoBucketsAreDifferentIfCodesAreDifferent()
        {
            BudgetBucket subject1 = Arrange("Foo1", "Name");
            BudgetBucket subject2 = Arrange("Foo2", "Name");
            Assert.AreNotEqual(subject1, subject2);
            Assert.IsTrue(subject1 != subject2);
            Assert.AreNotEqual(subject1.GetHashCode(), subject2.GetHashCode());
        }

        [TestMethod]
        public void TwoBucketsAreTheSameIfCodesAreEqual()
        {
            BudgetBucket subject1 = Arrange("Foo", "Name");
            BudgetBucket subject2 = Arrange("Foo", "Name");
            Assert.AreEqual(subject1, subject2);
            Assert.IsTrue(subject1 == subject2);
            Assert.AreEqual(subject1.GetHashCode(), subject2.GetHashCode());
        }

        [TestMethod]
        public void TwoReferencesToDifferentObjectsAreNotEqual()
        {
            BudgetBucket subject1 = Arrange("Foo", "Name");
            BudgetBucket subject2 = Arrange("Ben", "Is Awesome");
            Assert.AreNotEqual(subject1, subject2);
            Assert.IsTrue(subject1 != subject2);
            Assert.AreNotEqual(subject1.GetHashCode(), subject2.GetHashCode());
        }

        [TestMethod]
        public void TwoReferencesToTheSameObjectAreEqual()
        {
            BudgetBucket subject1 = Arrange("Foo", "Name");
            BudgetBucket subject2 = subject1;
            Assert.AreEqual(subject1, subject2);
            Assert.IsTrue(subject1 == subject2);
            Assert.AreEqual(subject1.GetHashCode(), subject2.GetHashCode());
        }

        [TestMethod]
        public void ValidateShouldRetrunFalseGivenLongCode()
        {
            BudgetBucket subject = Arrange();
            subject.Code = "ABC345678";
            bool result = subject.Validate(new StringBuilder());
            Assert.IsFalse(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateShouldThrowGivenNullStringBuilder()
        {
            BudgetBucket subject = Arrange();
            subject.Validate(null);
            Assert.Fail();
        }

        [TestMethod]
        public void ValidateWillReturnFalseWhenCodeIsNull()
        {
            BudgetBucket subject = Arrange();
            subject.Description = "Foo bar";
            var builder = new StringBuilder();
            Assert.IsFalse(subject.Validate(builder));
            Assert.IsTrue(builder.Length > 0);
        }

        [TestMethod]
        public void ValidateWillReturnFalseWhenCodeIsTooLong()
        {
            BudgetBucket subject = Arrange();
            subject.Description = "FooBarHo";
            var builder = new StringBuilder();
            Assert.IsFalse(subject.Validate(builder));
            Assert.IsTrue(builder.Length > 0);
        }

        [TestMethod]
        public void ValidateWillReturnFalseWhenNameIsNull()
        {
            BudgetBucket subject = Arrange();
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

            string code2 = code == NotSpecified ? null : code;
            string name2 = name == NotSpecified ? null : name;
            return new BudgetBucketTestHarness(code2, name2);
        }
    }
}