using System;
using System.Security;
using BudgetAnalyser.Encryption;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Encryption
{
    [TestClass]
    public class CredentialStoreTest
    {
        private SecureStringCredentialStore subject;

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void Dispose_ShouldDisposeStoredObject()
        {
            var credential = CreateSecureString("Foo");
            this.subject.SetPasskey(credential);

            this.subject.Dispose();

            credential.AppendChar('c');

            Assert.Fail();
        }

        [TestMethod]
        public void RetrievePasskey_ShouldReturnNull_GivenEmptyWasStored()
        {
            this.subject.SetPasskey(CreateSecureString(""));

            var result = (SecureString) this.subject.RetrievePasskey();

            Assert.IsNull(result);
        }

        [TestMethod]
        public void RetrievePasskey_ShouldReturnNull_GivenNullWasStored()
        {
            var result = (SecureString) this.subject.RetrievePasskey();

            Assert.IsNull(result);
        }

        [TestMethod]
        public void RetrievePasskey_ShouldReturnSecuredString_GivenTextValue()
        {
            this.subject.SetPasskey(CreateSecureString("Foo"));

            var result = this.subject.RetrievePasskey();

            Assert.IsInstanceOfType(result, typeof(SecureString));
        }

        [TestMethod]
        public void RetrievePasskey_ShouldStoreForRetrieval_GivenTextValue()
        {
            this.subject.SetPasskey(CreateSecureString("Foo"));

            var result = (SecureString) this.subject.RetrievePasskey();

            Assert.AreEqual(3, result.Length);
        }

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void SetPasskey_ShouldDisposeStoredObject()
        {
            var credential = CreateSecureString("Foo");
            this.subject.SetPasskey(credential);

            this.subject.SetPasskey(CreateSecureString("Bar"));

            credential.AppendChar('c');

            Assert.Fail();
        }

        [TestInitialize]
        public void TestSetup()
        {
            this.subject = new SecureStringCredentialStore();
        }

        internal static SecureString CreateSecureString(string text)
        {
            var securedText = new SecureString();
            foreach (var c in text)
            {
                securedText.AppendChar(c);
            }

            return securedText;
        }
    }
}