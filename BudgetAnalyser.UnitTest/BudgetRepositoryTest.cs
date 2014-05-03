using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.Remoting.Messaging;
using System.Xaml;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BudgetAnalyser.UnitTest
{
    [TestClass]
    public class BudgetRepositoryTest
    {
        private const string FileName1 = @"BudgetAnalyser.UnitTest.TestData.BudgetCollectionTestData.xml";

        private const string DemoBudgetFileName = @"BudgetAnalyser.UnitTest.TestData.DemoBudget.xml";

        private const string EmptyBudgetFileName = @"BudgetAnalyser.UnitTest.TestData.BudgetModel.xml";

        private Mock<IBudgetBucketRepository> mockBucketRepository;

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowWhenBucketRepositoryIsNull()
        {
            new XamlOnDiskBudgetRepository(null, new BudgetCollectionToDataBudgetCollectionMapper(new BudgetModelToDataBudgetModelMapper()), new DataBudgetCollectionToBudgetCollectionMapper(new DataBudgetModelToBudgetModelMapper()));
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
            subject.LoadFromDiskMock = OnLoadFromDiskMock;
            var collection = subject.Load(FileName1);

            Assert.AreEqual(FileName1, collection.FileName);
        }

        [TestMethod]
        public void LoadShouldCallInitialiseOnTheBucketRepository()
        {
            var subject = CreateSubject();
            subject.FileExistsMock = f => true;
            this.mockBucketRepository.Setup(m => m.Initialise(null));

            subject.LoadFromDiskMock = OnLoadFromDiskMock;
            subject.Load(FileName1);

            this.mockBucketRepository.Verify();
        }

        [TestMethod]
        public void MustBeAbleToLoadDemoBudgetFile()
        {
            var subject = CreateSubject();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = OnLoadFromDiskMock;
            var collection = subject.Load(DemoBudgetFileName);

            Assert.AreEqual(DemoBudgetFileName, collection.FileName);
            Assert.AreEqual(1, collection.Count);
        }

        [TestMethod]
        public void MustBeAbleToLoadEmptyBudgetFile()
        {
            var subject = CreateSubject();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = OnLoadFromDiskMock;
            var collection = subject.Load(EmptyBudgetFileName);

            Assert.AreEqual(EmptyBudgetFileName, collection.FileName);
            Assert.AreEqual(1, collection.Count);
        }

        private DataBudgetCollection OnLoadFromDiskMock(string f)
        {
            // this line of code is useful to figure out the name Vs has given the resource! The name is case sensitive.
            Assembly.GetExecutingAssembly().GetManifestResourceNames().ToList().ForEach(n => Debug.WriteLine(n));
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(f))
            {
                if (stream == null)
                {
                    throw new MissingManifestResourceException("Cannot find resource named: " + f);
                }

                return (DataBudgetCollection)XamlServices.Load(new XamlXmlReader(stream));
            }
        }

        [TestInitialize]
        public void TestInitialise()
        {
            this.mockBucketRepository = new Mock<IBudgetBucketRepository>();
        }

        private XamlOnDiskBudgetRepositoryTestHarness CreateSubject()
        {
            return new XamlOnDiskBudgetRepositoryTestHarness(this.mockBucketRepository.Object);
        }
    }
}