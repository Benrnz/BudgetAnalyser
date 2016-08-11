using System;
using System.Collections.Generic;
using BudgetAnalyser.Encryption;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Persistence;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BudgetAnalyser.Engine.UnitTest.Encryption
{
    [TestClass]
    public class LocalDiskReaderWriterSelectorTest
    {
        private LocalDiskReaderWriterSelector subject;

        [TestMethod]
        public void SelectReaderWriter_ShouldReturnEncrypted_GivenTrue()
        {
            var mockCredentials = new Mock<ICredentialStore>().Object;
            var mockFileEncryptor = new Mock<IFileEncryptor>().Object;
            this.subject = new LocalDiskReaderWriterSelector(
                new List<IFileReaderWriter>
                {
                    new EncryptedLocalDiskReaderWriter(mockFileEncryptor, mockCredentials),
                    new LocalDiskReaderWriter()
                });

            var result = this.subject.SelectReaderWriter(true);

            Assert.IsInstanceOfType(result, typeof(EncryptedLocalDiskReaderWriter));
        }

        [TestMethod]
        public void SelectReaderWriter_ShouldReturnUnprotected_GivenFalse()
        {
            var mockCredentials = new Mock<ICredentialStore>().Object;
            var mockFileEncryptor = new Mock<IFileEncryptor>().Object;
            this.subject = new LocalDiskReaderWriterSelector(
                new List<IFileReaderWriter>
                {
                    new EncryptedLocalDiskReaderWriter(mockFileEncryptor, mockCredentials),
                    new LocalDiskReaderWriter()
                });

            var result = this.subject.SelectReaderWriter(false);

            Assert.IsInstanceOfType(result, typeof(LocalDiskReaderWriter));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SelectReaderWriter_ShouldThrow_GivenNoReaderWriters()
        {
            this.subject = new LocalDiskReaderWriterSelector(new List<IFileReaderWriter>());
            this.subject.SelectReaderWriter(true);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SelectReaderWriter_ShouldThrow_GivenOnlyEncryptedRegisteredAndFalse()
        {
            var mockCredentials = new Mock<ICredentialStore>().Object;
            var mockFileEncryptor = new Mock<IFileEncryptor>().Object;
            this.subject = new LocalDiskReaderWriterSelector(new List<IFileReaderWriter> { new EncryptedLocalDiskReaderWriter(mockFileEncryptor, mockCredentials) });

            this.subject.SelectReaderWriter(false);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SelectReaderWriter_ShouldThrow_GivenOnlyUnprotectedRegisteredAndTrue()
        {
            this.subject = new LocalDiskReaderWriterSelector(new List<IFileReaderWriter> { new LocalDiskReaderWriter() });
            this.subject.SelectReaderWriter(true);
            Assert.Fail();
        }
    }
}