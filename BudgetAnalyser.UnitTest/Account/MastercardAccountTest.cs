using BudgetAnalyser.Engine.Account;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Account
{
    [TestClass]
    public class MastercardAccountTest
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
            var clone = subject.Clone("CloneMastercard");

            Assert.IsFalse(ReferenceEquals(subject, clone));
        }

        [TestMethod]
        public void CloneShouldGiveUseNameGiven()
        {
            var subject = CreateSubject();
            var clone = subject.Clone("CloneMastercard");

            Assert.AreEqual("CloneMastercard", clone.Name);
            Assert.AreNotEqual("CloneMastercard", subject.Name);
        }

        private MastercardAccount CreateSubject()
        {
            return new MastercardAccount("MastercardTest");
        }
    }
}
