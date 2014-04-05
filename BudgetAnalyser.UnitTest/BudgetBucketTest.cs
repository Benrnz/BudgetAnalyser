using System;
using System.Text;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest
{
    [TestClass]
    public class BudgetBucketTest
    {
        private const string NotSpecified = "NotSpecified";

        [TestMethod]
        public void CtorShouldAllocateIdThruCodeNameCtor()
        {
            BudgetBucket subject = CreateSubject("Foo", "Bar");
            Assert.AreNotEqual(Guid.Empty, subject.Id);
        }

        [TestMethod]
        public void CtorShouldAllocateIdThruDefaultCtor()
        {
            BudgetBucket subject = CreateSubject();
            Assert.AreNotEqual(Guid.Empty, subject.Id);
        }

        [TestMethod]
        public void CtorShouldAllocateUpperCaseCode()
        {
            BudgetBucket subject = CreateSubject("Foo", "Bar");
            Assert.AreEqual("FOO", subject.Code);
            Assert.AreNotEqual("Foo", subject.Code);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowWhenCodeIsNull()
        {
            CreateSubject(NotSpecified, "Something");
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowWhenNameIsNull()
        {
            CreateSubject("Something", NotSpecified);
            Assert.Fail();
        }

        [TestMethod]
        public void SettingCodeShouldConvertToUpperCase()
        {
            BudgetBucket subject = CreateSubject("Foo", "Bar");
            subject.Code = "White";
            Assert.AreNotEqual("White", subject.Code);
            Assert.AreEqual("WHITE", subject.Code);
        }

        [TestMethod]
        public void TwoBucketsAreDifferentIfCodesAreDifferent()
        {
            BudgetBucket subject1 = CreateSubject("Foo1", "Name");
            BudgetBucket subject2 = CreateSubject("Foo2", "Name");
            Assert.AreNotEqual(subject1, subject2);
            Assert.IsTrue(subject1 != subject2);
            Assert.AreNotEqual(subject1.GetHashCode(), subject2.GetHashCode());
        }

        [TestMethod]
        public void TwoBucketsAreTheSameIfCodesAreEqual()
        {
            BudgetBucket subject1 = CreateSubject("Foo", "Name");
            BudgetBucket subject2 = CreateSubject("Foo", "Name");
            Assert.AreEqual(subject1, subject2);
            Assert.IsTrue(subject1 == subject2);
            Assert.AreEqual(subject1.GetHashCode(), subject2.GetHashCode());
        }

        [TestMethod]
        public void TwoReferencesToDifferentObjectsAreNotEqual()
        {
            BudgetBucket subject1 = CreateSubject("Foo", "Name");
            BudgetBucket subject2 = CreateSubject("Ben", "Is Awesome");
            Assert.AreNotEqual(subject1, subject2);
            Assert.IsTrue(subject1 != subject2);
            Assert.AreNotEqual(subject1.GetHashCode(), subject2.GetHashCode());
        }

        [TestMethod]
        public void TwoReferencesToTheSameObjectAreEqual()
        {
            BudgetBucket subject1 = CreateSubject("Foo", "Name");
            BudgetBucket subject2 = subject1;
            Assert.AreEqual(subject1, subject2);
            Assert.IsTrue(subject1 == subject2);
            Assert.AreEqual(subject1.GetHashCode(), subject2.GetHashCode());
        }

        [TestMethod]
        public void ValidateWillReturnFalseWhenCodeIsNull()
        {
            BudgetBucket subject = CreateSubject();
            subject.Description = "Foo bar";
            var builder = new StringBuilder();
            Assert.IsFalse(subject.Validate(builder));
            Assert.IsTrue(builder.Length > 0);
        }

        [TestMethod]
        public void ValidateWillReturnFalseWhenCodeIsTooLong()
        {
            BudgetBucket subject = CreateSubject();
            subject.Description = "FooBarHo";
            var builder = new StringBuilder();
            Assert.IsFalse(subject.Validate(builder));
            Assert.IsTrue(builder.Length > 0);
        }

        [TestMethod]
        public void ValidateWillReturnFalseWhenNameIsNull()
        {
            BudgetBucket subject = CreateSubject();
            subject.Code = "Foo";
            var builder = new StringBuilder();
            Assert.IsFalse(subject.Validate(builder));
            Assert.IsTrue(builder.Length > 0);
        }

        private BudgetBucket CreateSubject(string code = NotSpecified, string name = NotSpecified)
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