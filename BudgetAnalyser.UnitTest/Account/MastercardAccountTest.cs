using BudgetAnalyser.Engine.BankAccount;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace BudgetAnalyser.UnitTest.Account
{
    [TestClass]
    public class MastercardAccountTest
    {
        [TestMethod]
        public void CloneShouldGiveUseNameGiven()
        {
            MastercardAccount subject = CreateSubject();
            Engine.BankAccount.Account clone = subject.Clone("CloneMastercard");

            Assert.AreEqual("CloneMastercard", clone.Name);
            Assert.AreNotEqual("CloneMastercard", subject.Name);
        }

        [TestMethod]
        public void CloneShouldNotJustCopyReference()
        {
            MastercardAccount subject = CreateSubject();
            Engine.BankAccount.Account clone = subject.Clone("CloneMastercard");

            Assert.IsFalse(ReferenceEquals(subject, clone));
        }

        [TestMethod]
        public void KeywordsShouldContainElements()
        {
            MastercardAccount subject = CreateSubject();

            Assert.IsTrue(subject.KeyWords.Length > 0);
        }

        [TestMethod]
        public void KeywordsShouldNotBeNull()
        {
            MastercardAccount subject = CreateSubject();

            Assert.IsNotNull(subject.KeyWords);
        }

        [TestMethod]
        public void NameShouldBeSomething()
        {
            MastercardAccount subject = CreateSubject();
            Assert.IsFalse(string.IsNullOrWhiteSpace(subject.Name));
        }

        private MastercardAccount CreateSubject()
        {
            return new MastercardAccount("MastercardTest");
        }
    }
}