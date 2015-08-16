using BudgetAnalyser.Engine.Account;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Account
{
    [TestClass]
    public class AmexAccountTest
    {
        [TestMethod]
        public void CloneShouldGiveUseNameGiven()
        {
            AmexAccount subject = CreateSubject();
            Engine.Account.Account clone = subject.Clone("CloneAmex");

            Assert.AreEqual("CloneAmex", clone.Name);
            Assert.AreNotEqual("CloneAmex", subject.Name);
        }

        [TestMethod]
        public void CloneShouldNotJustCopyReference()
        {
            AmexAccount subject = CreateSubject();
            Engine.Account.Account clone = subject.Clone("CloneAmex");

            Assert.IsFalse(ReferenceEquals(subject, clone));
        }

        [TestMethod]
        public void KeywordsShouldContainElements()
        {
            AmexAccount subject = CreateSubject();

            Assert.IsTrue(subject.KeyWords.Length > 0);
        }

        [TestMethod]
        public void KeywordsShouldNotBeNull()
        {
            AmexAccount subject = CreateSubject();

            Assert.IsNotNull(subject.KeyWords);
        }

        [TestMethod]
        public void NameShouldBeSomething()
        {
            AmexAccount subject = CreateSubject();
            Assert.IsFalse(string.IsNullOrWhiteSpace(subject.Name));
        }

        private AmexAccount CreateSubject()
        {
            return new AmexAccount("AmexTest");
        }
    }
}