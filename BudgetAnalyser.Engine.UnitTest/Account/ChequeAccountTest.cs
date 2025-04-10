﻿using BudgetAnalyser.Engine.BankAccount;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Account
{
    [TestClass]
    public class ChequeAccountTest
    {
        [TestMethod]
        public void CloneShouldGiveUseNameGiven()
        {
            var subject = CreateSubject();
            var clone = subject.Clone("CloneCheque");

            Assert.AreEqual("CloneCheque", clone.Name);
            Assert.AreNotEqual("CloneCheque", subject.Name);
        }

        [TestMethod]
        public void CloneShouldNotJustCopyReference()
        {
            var subject = CreateSubject();
            var clone = subject.Clone("CloneCheque");

            Assert.IsFalse(ReferenceEquals(subject, clone));
        }

        [TestMethod]
        public void KeywordsShouldContainElements()
        {
            var subject = CreateSubject();

            Assert.IsTrue(subject.KeyWords.Length > 0);
        }

        [TestMethod]
        public void KeywordsShouldNotBeNull()
        {
            var subject = CreateSubject();

            Assert.IsNotNull(subject.KeyWords);
        }

        [TestMethod]
        public void NameShouldBeSomething()
        {
            var subject = CreateSubject();
            Assert.IsFalse(string.IsNullOrWhiteSpace(subject.Name));
        }

        private ChequeAccount CreateSubject()
        {
            return new ChequeAccount("ChequeTest");
        }
    }
}
