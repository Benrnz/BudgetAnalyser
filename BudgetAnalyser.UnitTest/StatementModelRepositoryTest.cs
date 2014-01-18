using System;
using System.Security.AccessControl;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Statement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BudgetAnalyser.UnitTest
{
    [TestClass]
    public class StatementModelRepositoryTest
    {
        [TestMethod]
        public void IsValidWithFileNameShouldCallImporter()
        {
            var subject = CreateSubject();
            string fileName = "Blahblah.csv";
            ImporterMock.Setup(m => m.IsValidFile(fileName)).Returns(true);

            subject.IsValidFile(fileName);

            ImporterMock.Verify();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IsValidWithNullShouldThrow()
        {
            var subject = CreateSubject();
            string fileName = null;

            subject.IsValidFile(fileName);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IsValidWithEmptyShouldThrow()
        {
            var subject = CreateSubject();
            string fileName = string.Empty;

            subject.IsValidFile(fileName);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LoadWithNullShouldThrow()
        {
            var subject = CreateSubject();
            string fileName = null;

            subject.Load(fileName);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LoadWithEmptyShouldThrow()
        {
            var subject = CreateSubject();
            string fileName = string.Empty;

            subject.Load(fileName);

            Assert.Fail();
        }

        [TestMethod]
        public void LoadWithFileNameShouldCallImporter()
        {
            var subject = CreateSubject();
            string fileName = "Blahblah.csv";
            ImporterMock.Setup(m => m.Load(fileName)).Returns(CreateStatementModelTestData());

            subject.Load(fileName);

            ImporterMock.Verify();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SaveWithEmptyFileNameShouldThrow()
        {
            var subject = CreateSubject();
            string fileName = string.Empty;
            var testData = CreateStatementModelTestData();

            subject.Save(testData, fileName);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SaveWithNullFileNameShouldThrow()
        {
            var subject = CreateSubject();
            string fileName = null;
            var testData = CreateStatementModelTestData();

            subject.Save(testData, fileName);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SaveWithNullModelShouldThrow()
        {
            var subject = CreateSubject();
            string fileName = "Blakhskgh.csv";

            subject.Save(null, fileName);

            Assert.Fail();
        }

        [TestMethod]
        public void SaveWithFileNameShouldCallImporter()
        {
            var subject = CreateSubject();
            string fileName = "Blahblah.csv";
            var testData = CreateStatementModelTestData();
            ImporterMock.Setup(m => m.Save(testData, fileName));

            subject.Save(testData, fileName);

            ImporterMock.Verify();
        }

        private StatementModel CreateStatementModelTestData()
        {
            return new StatementModel();
        }

        private Mock<IVersionedStatementModelImporter> ImporterMock { get; set; }

        private BudgetAnalyserStatementModelRepository CreateSubject()
        {
            ImporterMock = new Mock<IVersionedStatementModelImporter>();
            return new BudgetAnalyserStatementModelRepository(ImporterMock.Object);
        }
    }
}
