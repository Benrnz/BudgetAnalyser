using BudgetAnalyser.Engine.BankAccount;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace BudgetAnalyser.UnitTest.Account
{
    [TestClass]
    public class VisaAccountTest
    {
        [TestMethod]
        public void CloneShouldGiveUseNameGiven()
        {
            VisaAccount subject = CreateSubject();
            Engine.BankAccount.Account clone = subject.Clone("CloneVisa");

            Assert.AreEqual("CloneVisa", clone.Name);
            Assert.AreNotEqual("CloneVisa", subject.Name);
        }

        [TestMethod]
        public void CloneShouldNotJustCopyReference()
        {
            VisaAccount subject = CreateSubject();
            Engine.BankAccount.Account clone = subject.Clone("CloneVisa");

            Assert.IsFalse(ReferenceEquals(subject, clone));
        }

        [TestMethod]
        public void KeywordsShouldContainElements()
        {
            VisaAccount subject = CreateSubject();

            Assert.IsTrue(subject.KeyWords.Length > 0);
        }

        [TestMethod]
        public void KeywordsShouldNotBeNull()
        {
            VisaAccount subject = CreateSubject();

            Assert.IsNotNull(subject.KeyWords);
        }

        [TestMethod]
        public void NameShouldBeSomething()
        {
            VisaAccount subject = CreateSubject();
            Assert.IsFalse(string.IsNullOrWhiteSpace(subject.Name));
        }

        private VisaAccount CreateSubject()
        {
            return new VisaAccount("VisaTest");
        }
    }
}