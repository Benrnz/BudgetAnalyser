using BudgetAnalyser.Engine.Account;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest
{
    [TestClass]
    public class AmexAccountTest
    {
        [TestMethod]
        public void KeywordsShouldNotBeNull()
        {
            var subject = CreateSubject();

            Assert.IsNotNull(subject.KeyWords);
        }

        [TestMethod]
        public void KeywordsShouldContainElements()
        {
            var subject = CreateSubject();

            Assert.IsTrue(subject.KeyWords.Length > 0);
        }

        [TestMethod]
        public void NameShouldBeSomething()
        {
            var subject = CreateSubject();
            Assert.IsFalse(string.IsNullOrWhiteSpace(subject.Name));
        }

        [TestMethod]
        public void CloneShouldNotJustCopyReference()
        {
            var subject = CreateSubject();
            var clone = subject.Clone("CloneAmex");

            Assert.IsFalse(ReferenceEquals(subject, clone));
        }

        [TestMethod]
        public void CloneShouldGiveUseNameGiven()
        {
            var subject = CreateSubject();
            var clone = subject.Clone("CloneAmex");

            Assert.AreEqual("CloneAmex", clone.Name);
            Assert.AreNotEqual("CloneAmex", subject.Name);
        }

        private AmexAccount CreateSubject()
        {
            return new AmexAccount("AmexTest");
        }
    }
}
