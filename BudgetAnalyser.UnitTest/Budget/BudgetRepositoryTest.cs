using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Xaml;
using System.Xml.Linq;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.UnitTest.Helper;
using BudgetAnalyser.UnitTest.TestData;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BudgetAnalyser.UnitTest.Budget
{
    [TestClass]
    public class BudgetRepositoryTest
    {
        private const string DemoBudgetFileName = @"BudgetAnalyser.UnitTest.TestData.DemoBudget.xml";

        private const string EmptyBudgetFileName = @"BudgetAnalyser.UnitTest.TestData.BudgetModel.xml";
        private const string FileName1 = @"BudgetAnalyser.UnitTest.TestData.BudgetCollectionTestData.xml";

        [TestMethod]
        public void CreateNewShouldPopulateFileName()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            subject.WriteToDiskMock = (f, d) => { };
            string filename = "FooBar.xml";
            BudgetCollection collection = subject.CreateNew(filename);
            Assert.AreEqual(filename, collection.FileName);
        }

        [TestMethod]
        public void CreateNewShouldReturnCollectionWithOneBudgetInIt()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            subject.WriteToDiskMock = (f, d) => { };
            string filename = "FooBar.xml";
            BudgetCollection collection = subject.CreateNew(filename);

            Assert.IsTrue(collection.Count == 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNewShouldThrowGivenEmptyFileName()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            subject.CreateNew(string.Empty);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNewShouldThrowGivenNullFileName()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            subject.CreateNew(null);
            Assert.Fail();
        }

        [TestMethod]
        public void CreateNewShouldWriteToDisk()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            bool writeToDiskCalled = false;
            subject.WriteToDiskMock = (f, d) => { writeToDiskCalled = true; };
            string filename = "FooBar.xml";
            subject.CreateNew(filename);
            Assert.IsTrue(writeToDiskCalled);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullDomainMapper()
        {
            new XamlOnDiskBudgetRepository(
                new BucketBucketRepoAlwaysFind(),
                new BasicMapperFake<BudgetCollection, BudgetCollectionDto>(),
                null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullDtoMapper()
        {
            new XamlOnDiskBudgetRepository(
                new BucketBucketRepoAlwaysFind(),
                null,
                new BasicMapperFake<BudgetCollectionDto, BudgetCollection>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowWhenBucketRepositoryIsNull()
        {
            new XamlOnDiskBudgetRepository(
                null,
                new BasicMapperFake<BudgetCollection, BudgetCollectionDto>(),
                new BasicMapperFake<BudgetCollectionDto, BudgetCollection>());
            Assert.Fail();
        }

        [TestMethod]
        public void LoadShouldCallInitialiseOnTheBucketRepository()
        {
            var mockBucketRepository = new Mock<IBudgetBucketRepository>();
            mockBucketRepository.Setup(m => m.Initialise(null));

            var toDomainMapper = new Mock<BasicMapper<BudgetCollectionDto, BudgetCollection>>();
            var subject = new XamlOnDiskBudgetRepositoryTestHarness(
                mockBucketRepository.Object,
                new BasicMapperFake<BudgetCollection, BudgetCollectionDto>(),
                toDomainMapper.Object);
            toDomainMapper.Setup(m => m.Map(It.IsAny<BudgetCollectionDto>())).Returns(BudgetModelTestData.CreateCollectionWith1And2);
            subject.FileExistsMock = f => true;

            subject.LoadFromDiskMock = OnLoadFromDiskMock;
            subject.Load(FileName1);

            mockBucketRepository.Verify();
        }

        [TestMethod]
        public void LoadShouldReturnACollectionAndSetFileName()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = OnLoadFromDiskMock;
            BudgetCollection collection = subject.Load(FileName1);

            Assert.AreEqual(FileName1, collection.FileName);
        }

        [TestMethod]
        [ExpectedException(typeof(FileFormatException))]
        public void LoadShouldThrowIfAnyOtherExceptionIsThrownAndWrapItIntoFileFormatException()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = f => { throw new Exception(); };
            subject.Load("SmellyPoo.xml");

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(FileFormatException))]
        public void LoadShouldThrowIfDeserialisedObjectIsNotBudgetCollection()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = f => new object();
            subject.Load("SmellyPoo.xml");

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void LoadShouldThrowIfFileDoesntExist()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            subject.FileExistsMock = f => false;
            subject.Load("SmellyPoo.xml");

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(FileFormatException))]
        public void LoadShouldThrowIfFileFormatIsInvalid()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = f => { throw new XamlObjectWriterException(); };
            subject.Load("SmellyPoo.xml");

            Assert.Fail();
        }

        [TestMethod]
        public void MustBeAbleToLoadDemoBudgetFile()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = OnLoadFromDiskMock;
            BudgetCollection collection = subject.Load(DemoBudgetFileName);

            Assert.AreEqual(DemoBudgetFileName, collection.FileName);
            Assert.AreEqual(1, collection.Count);
        }

        [TestMethod]
        public void MustBeAbleToLoadEmptyBudgetFile()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = OnLoadFromDiskMock;
            BudgetCollection collection = subject.Load(EmptyBudgetFileName);

            Assert.AreEqual(EmptyBudgetFileName, collection.FileName);
            Assert.AreEqual(1, collection.Count);
        }

        [TestMethod]
        public void SaveShouldWriteToDisk()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            bool writeToDiskCalled = false;
            subject.WriteToDiskMock = (filename, data) => { writeToDiskCalled = true; };
            subject.Save(BudgetModelTestData.CreateCollectionWith1And2());

            Assert.IsTrue(writeToDiskCalled);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            AutoMapperConfigurationTest.AutoMapperConfiguration();
        }

        private XamlOnDiskBudgetRepositoryTestHarness Arrange(IBudgetBucketRepository bucketRepo = null)
        {
            if (bucketRepo == null)
            {
                bucketRepo = new InMemoryBudgetBucketRepository(new DtoToBudgetBucketMapper());
            }

            return new XamlOnDiskBudgetRepositoryTestHarness(bucketRepo);
        }

        private static BudgetCollectionDto OnLoadFromDiskMock(string f)
        {
            return EmbeddedResourceHelper.Extract<BudgetCollectionDto>(f);
        }
    }
}