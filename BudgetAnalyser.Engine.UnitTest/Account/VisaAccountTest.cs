using BudgetAnalyser.Engine.BankAccount;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Account
{
    [TestClass]
    public class VisaAccountTest
    {
        [TestMethod]
        public void CloneShouldGiveUseNameGiven()
        {
            var subject = CreateSubject();
            var clone = subject.Clone("CloneVisa");

            Assert.AreEqual("CloneVisa", clone.Name);
            Assert.AreNotEqual("CloneVisa", subject.Name);
        }

        [TestMethod]
        public void CloneShouldNotJustCopyReference()
        {
            var subject = CreateSubject();
            var clone = subject.Clone("CloneVisa");

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

        private VisaAccount CreateSubject()
        {
            return new VisaAccount("VisaTest");
        }
    }
}
