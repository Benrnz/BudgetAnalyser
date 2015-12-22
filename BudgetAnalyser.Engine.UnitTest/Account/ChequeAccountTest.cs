using BudgetAnalyser.Engine.BankAccount;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Account
{
    [TestClass]
    public class ChequeAccountTest
    {
        [TestMethod]
        public void CloneShouldGiveUseNameGiven()
        {
            ChequeAccount subject = CreateSubject();
            Engine.BankAccount.Account clone = subject.Clone("CloneCheque");

            Assert.AreEqual("CloneCheque", clone.Name);
            Assert.AreNotEqual("CloneCheque", subject.Name);
        }

        [TestMethod]
        public void CloneShouldNotJustCopyReference()
        {
            ChequeAccount subject = CreateSubject();
            Engine.BankAccount.Account clone = subject.Clone("CloneCheque");

            Assert.IsFalse(ReferenceEquals(subject, clone));
        }

        [TestMethod]
        public void KeywordsShouldContainElements()
        {
            ChequeAccount subject = CreateSubject();

            Assert.IsTrue(subject.KeyWords.Length > 0);
        }

        [TestMethod]
        public void KeywordsShouldNotBeNull()
        {
            ChequeAccount subject = CreateSubject();

            Assert.IsNotNull(subject.KeyWords);
        }

        [TestMethod]
        public void NameShouldBeSomething()
        {
            ChequeAccount subject = CreateSubject();
            Assert.IsFalse(string.IsNullOrWhiteSpace(subject.Name));
        }

        private ChequeAccount CreateSubject()
        {
            return new ChequeAccount("ChequeTest");
        }
    }
}