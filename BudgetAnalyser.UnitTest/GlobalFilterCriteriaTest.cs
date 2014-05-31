using System;
using System.Text;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Account;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest
{
    [TestClass]
    public class GlobalFilterCriteriaTest
    {
        private VisaAccount testVisaAccount = new VisaAccount("Visa");

        [TestMethod]
        public void WhenNotFilteredClearedShouldBeTrue()
        {
            var subject1 = CreateSubject_NotFiltered();
            Assert.IsTrue(subject1.Cleared);
        }

        [TestMethod]
        public void NotValidForBeginDateToBeAfterEndDate()
        {
            var invalidEndDate = new DateTime(2014, 3, 1);
            var subject = new GlobalFilterCriteria
            {
                BeginDate = new DateTime(2014, 5, 1),
                EndDate = invalidEndDate,
            };

            Assert.AreNotEqual(invalidEndDate, subject.EndDate);
            Assert.IsTrue(subject.BeginDate < subject.EndDate);
        }

        [TestMethod]
        public void MinDateShouldGetTransalatedToNull()
        {
            var subject = new GlobalFilterCriteria
            {
                BeginDate = DateTime.MinValue,
                EndDate = DateTime.MinValue,
            };

            Assert.IsNull(subject.BeginDate);
            Assert.IsNull(subject.EndDate);
        }

        [TestMethod]
        public void CheckReferenceEquality()
        {
            var subject1 = CreateSubject_StandardPayMonth();
            var subject2 = subject1;

            Assert.AreEqual(subject1, subject2);
            Assert.IsFalse(subject1 != subject2);
            Assert.IsTrue(subject1 == subject2);
            Assert.IsTrue(subject1.Equals(subject2));
            Assert.AreSame(subject1, subject2);
            Assert.AreEqual(subject1.GetHashCode(), subject2.GetHashCode());
        }

        [TestMethod]
        public void CheckReferenceInequality()
        {
            var subject1 = CreateSubject_StandardPayMonth();
            var subject2 = CreateSubject_NotFiltered();

            Assert.AreNotEqual(subject1, subject2);
            Assert.IsTrue(subject1 != subject2);
            Assert.IsFalse(subject1 == subject2);
            Assert.IsFalse(subject1.Equals(subject2));
            Assert.AreNotSame(subject1, subject2);
            Assert.AreNotEqual(subject1.GetHashCode(), subject2.GetHashCode());
        }

        [TestMethod]
        public void TwoInstancesWithSameValuesAreEqual()
        {
            var subject1 = CreateSubject_StandardPayMonth();
            var subject2 = CreateSubject_StandardPayMonth();

            Assert.AreEqual(subject1, subject2);
            Assert.IsFalse(subject1 != subject2);
            Assert.IsTrue(subject1 == subject2);
            Assert.IsTrue(subject1.Equals(subject2));
            Assert.AreNotSame(subject1, subject2);
            Assert.AreEqual(subject1.GetHashCode(), subject2.GetHashCode());
        }

        [TestMethod]
        public void TwoInstancesWithDifferentValuesAreNotEqual()
        {
            var subject1 = CreateSubject_StandardPayMonth();
            var subject2 = CreateSubject_StandardPayMonthAndVisaFilter();

            Assert.AreNotEqual(subject1, subject2);
            Assert.IsTrue(subject1 != subject2);
            Assert.IsFalse(subject1 == subject2);
            Assert.IsFalse(subject1.Equals(subject2));
            Assert.AreNotSame(subject1, subject2);
            Assert.AreNotEqual(subject1.GetHashCode(), subject2.GetHashCode());
        }

        private GlobalFilterCriteria CreateSubject_StandardPayMonth()
        {
            return new GlobalFilterCriteria
            {
                AccountType = null,
                BeginDate = new DateTime(2014, 1, 20),
                EndDate = new DateTime(2014, 2, 19),
            };
        }

        private GlobalFilterCriteria CreateSubject_NotFiltered()
        {
            return new GlobalFilterCriteria
            {
                AccountType = null,
                BeginDate = null,
                EndDate = null,
            };
        }

        private GlobalFilterCriteria CreateSubject_StandardPayMonthAndVisaFilter()
        {
            return new GlobalFilterCriteria
            {
                AccountType = this.testVisaAccount,
                BeginDate = new DateTime(2014, 1, 20),
                EndDate = new DateTime(2014, 2, 19),
            };
        }

        private GlobalFilterCriteria CreateSubject_VisaFilterOnly()
        {
            return new GlobalFilterCriteria
            {
                AccountType = this.testVisaAccount,
                BeginDate = null,
                EndDate = null,
            };
        }
    }
}
