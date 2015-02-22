using System;
using System.IO;
using System.Xaml;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.UnitTest.Helper;
using BudgetAnalyser.UnitTest.TestData;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rees.TestUtilities;

namespace BudgetAnalyser.UnitTest.Budget
{
    [TestClass]
    public class XamlOnDiskBudgetRepositoryTest
    {
        private const string DemoBudgetFileName = @"BudgetAnalyser.UnitTest.TestData.DemoBudget.xml";
        private const string EmptyBudgetFileName = @"BudgetAnalyser.UnitTest.TestData.BudgetModel.xml";
        private const string FileName1 = @"BudgetAnalyser.UnitTest.TestData.BudgetCollectionTestData.xml";

        private XamlOnDiskBudgetRepositoryTestHarness Arrange(IBudgetBucketRepository bucketRepo = null)
        {
            if (bucketRepo == null)
            {
                bucketRepo = new InMemoryBudgetBucketRepository(new DtoToBudgetBucketMapper());
            }

            return new XamlOnDiskBudgetRepositoryTestHarness(bucketRepo);
        }

        [TestMethod]
        public void CreateNewShouldPopulateFileName()
        {
            var subject = Arrange();
            subject.WriteToDiskMock = (f, d) => { };
            SetPrivateBudgetCollection(subject);
            var filename = "FooBar.xml";
            var collection = subject.CreateNew(filename);
            Assert.AreEqual(filename, collection.FileName);
        }

        [TestMethod]
        public void CreateNewShouldReturnCollectionWithOneBudgetInIt()
        {
            var subject = Arrange();
            subject.WriteToDiskMock = (f, d) => { };
            SetPrivateBudgetCollection(subject);
            var filename = "FooBar.xml";
            var collection = subject.CreateNew(filename);

            Assert.IsTrue(collection.Count == 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNewShouldThrowGivenEmptyFileName()
        {
            var subject = Arrange();
            subject.CreateNew(string.Empty);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNewShouldThrowGivenNullFileName()
        {
            var subject = Arrange();
            subject.CreateNew(null);
            Assert.Fail();
        }

        [TestMethod]
        public void CreateNewShouldWriteToDisk()
        {
            var subject = Arrange();
            var writeToDiskCalled = false;
            subject.WriteToDiskMock = (f, d) => { writeToDiskCalled = true; };
            SetPrivateBudgetCollection(subject);
            var filename = "FooBar.xml";
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
            subject.LoadAsync(FileName1);

            mockBucketRepository.Verify();
        }

        [TestMethod]
        public void LoadShouldReturnACollectionAndSetFileName()
        {
            var subject = Arrange();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = OnLoadFromDiskMock;
            var collection = subject.LoadAsync(FileName1);

            Assert.AreEqual(FileName1, collection.FileName);
        }

        [TestMethod]
        [ExpectedException(typeof(DataFormatException))]
        public void LoadShouldThrowIfAnyOtherExceptionIsThrownAndWrapItIntoFileFormatException()
        {
            var subject = Arrange();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = f => { throw new Exception(); };
            subject.LoadAsync("SmellyPoo.xml");

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(DataFormatException))]
        public void LoadShouldThrowIfDeserialisedObjectIsNotBudgetCollection()
        {
            var subject = Arrange();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = f => new object();
            subject.LoadAsync("SmellyPoo.xml");

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void LoadShouldThrowIfFileDoesntExist()
        {
            var subject = Arrange();
            subject.FileExistsMock = f => false;
            subject.LoadAsync("SmellyPoo.xml");

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(DataFormatException))]
        public void LoadShouldThrowIfFileFormatIsInvalid()
        {
            var subject = Arrange();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = f => { throw new XamlObjectWriterException(); };
            subject.LoadAsync("SmellyPoo.xml");

            Assert.Fail();
        }

        [TestMethod]
        public void MustBeAbleToLoadDemoBudgetFile()
        {
            var subject = Arrange();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = OnLoadFromDiskMock;
            var collection = subject.LoadAsync(DemoBudgetFileName);

            Assert.AreEqual(DemoBudgetFileName, collection.FileName);
            Assert.AreEqual(1, collection.Count);
        }

        [TestMethod]
        public void MustBeAbleToLoadEmptyBudgetFile()
        {
            var subject = Arrange();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = OnLoadFromDiskMock;
            var collection = subject.LoadAsync(EmptyBudgetFileName);

            Assert.AreEqual(EmptyBudgetFileName, collection.FileName);
            Assert.AreEqual(1, collection.Count);
        }

        [TestMethod]
        public void SaveShouldWriteToDisk()
        {
            var subject = Arrange();
            var writeToDiskCalled = false;
            subject.WriteToDiskMock = (filename, data) => { writeToDiskCalled = true; };
            SetPrivateBudgetCollection(subject);

            subject.Save(BudgetModelTestData.CreateCollectionWith1And2());

            Assert.IsTrue(writeToDiskCalled);
        }

        private static void SetPrivateBudgetCollection(XamlOnDiskBudgetRepositoryTestHarness subject)
        {
            PrivateAccessor.SetField<XamlOnDiskBudgetRepository>(subject, "currentBudgetCollection", BudgetModelTestData.CreateCollectionWith1And2());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SaveShouldThrowIfLoadHasntBeenCalled()
            {
            var subject = Arrange();
            subject.Save();
            Assert.Fail();
            }

        [TestInitialize]
        public void TestInitialise()
        {
        }

        private static BudgetCollectionDto OnLoadFromDiskMock(string f)
        {
            return EmbeddedResourceHelper.ExtractXaml<BudgetCollectionDto>(f);
        }
    }
}