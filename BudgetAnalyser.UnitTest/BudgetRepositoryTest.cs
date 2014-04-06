using System;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Xaml;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BudgetAnalyser.UnitTest
{
    [TestClass]
    public class BudgetRepositoryTest
    {
        private Mock<IBudgetBucketRepository> mockBucketRepository;

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowWhenBucketRepositoryIsNull()
        {
            new BudgetRepository(null);
            Assert.Fail();
        }

        [TestMethod]
        public void SaveShouldWriteToDisk()
        {
            var subject = CreateSubject();
            bool writeToDiskCalled = false;
            subject.WriteToDiskMock = (filename, data) =>
            {
                writeToDiskCalled = true;
            };
            subject.Save(TestData.BudgetModelTestData.CreateCollectionWith1And2());

            Assert.IsTrue(writeToDiskCalled);
        }

        [TestMethod]
        public void CreateNewShouldPopulateFileName()
        {
            var subject = CreateSubject();
            subject.WriteToDiskMock = (f, d) => { };
            var filename = "FooBar.xml";
            var collection = subject.CreateNew(filename);
            Assert.AreEqual(filename, collection.FileName);
        }

        [TestMethod]
        public void CreateNewShouldWriteToDisk()
        {
            var subject = CreateSubject();
            bool writeToDiskCalled = false;
            subject.WriteToDiskMock = (f, d) =>
            {
                writeToDiskCalled = true;
            };
            var filename = "FooBar.xml";
            subject.CreateNew(filename);
            Assert.IsTrue(writeToDiskCalled);
        }

        [TestMethod]
        public void CreateNewShouldReturnCollectionWithOneBudgetInIt()
        {
            var subject = CreateSubject();
            subject.WriteToDiskMock = (f, d) => { };
            var filename = "FooBar.xml";
            var collection = subject.CreateNew(filename);

            Assert.IsTrue(collection.Count == 1);
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void LoadShouldThrowIfFileDoesntExist()
        {
            var subject = CreateSubject();
            subject.FileExistsMock = f => false;
            subject.Load("SmellyPoo.xml");

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(FileFormatException))]
        public void LoadShouldThrowIfFileFormatIsInvalid()
        {
            var subject = CreateSubject();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = f => { throw new XamlObjectWriterException(); };
            subject.Load("SmellyPoo.xml");

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(FileFormatException))]
        public void LoadShouldThrowIfAnyOtherExceptionIsThrownAndWrapItIntoFileFormatException()
        {
            var subject = CreateSubject();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = f => { throw new Exception(); };
            subject.Load("SmellyPoo.xml");

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(FileFormatException))]
        public void LoadShouldThrowIfDeserialisedObjectIsNotBudgetCollection()
        {
            var subject = CreateSubject();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = f => new object();
            subject.Load("SmellyPoo.xml");

            Assert.Fail();
        }

        [TestMethod]
        public void LoadShouldReturnACollectionAndSetFileName()
        {
            var subject = CreateSubject();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = f => TestData.BudgetModelTestData.CreateCollectionWith1And2();
            var collection = subject.Load("SmellyPoo.xml");

            Assert.AreEqual("SmellyPoo.xml", collection.FileName);
        }

        [TestMethod]
        public void LoadShouldCallInitialiseOnTheBucketRepository()
        {
            var subject = CreateSubject();
            subject.FileExistsMock = f => true;
            this.mockBucketRepository.Setup(m => m.Initialise(null));

            subject.LoadFromDiskMock = f => TestData.BudgetModelTestData.CreateCollectionWith1And2();
            subject.Load("SmellyPoo.xml");

            this.mockBucketRepository.Verify();
        }

        [TestInitialize]
        public void TestInitialise()
        {
            this.mockBucketRepository = new Mock<IBudgetBucketRepository>();
        }

        private BudgetRepositoryTestHarness CreateSubject()
        {
            return new BudgetRepositoryTestHarness(this.mockBucketRepository.Object);
        }
    }
}