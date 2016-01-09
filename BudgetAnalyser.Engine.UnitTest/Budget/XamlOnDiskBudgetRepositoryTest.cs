using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xaml;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.Engine.UnitTest.Helper;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rees.TangyFruitMapper;
using Rees.UnitTestUtilities;

namespace BudgetAnalyser.Engine.UnitTest.Budget
{
    [TestClass]
    public class XamlOnDiskBudgetRepositoryTest
    {
        [TestMethod]
        public async Task CreateNewShouldPopulateFileName()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            subject.WriteToDiskMock = (f, d) => { };
            SetPrivateBudgetCollection(subject);
            var filename = "FooBar.xml";
            BudgetCollection collection = await subject.CreateNewAndSaveAsync(filename);
            Assert.AreEqual(filename, collection.StorageKey);
        }

        [TestMethod]
        public async Task CreateNewShouldReturnCollectionWithOneBudgetInIt()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            subject.WriteToDiskMock = (f, d) => { };
            SetPrivateBudgetCollection(subject);
            var filename = "FooBar.xml";
            BudgetCollection collection = await subject.CreateNewAndSaveAsync(filename);

            Assert.IsTrue(collection.Count == 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateNewShouldThrowGivenEmptyFileName()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            await subject.CreateNewAndSaveAsync(string.Empty);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateNewShouldThrowGivenNullFileName()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            await subject.CreateNewAndSaveAsync(null);
            Assert.Fail();
        }

        [TestMethod]
        public async Task CreateNewShouldWriteToDisk()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            var writeToDiskCalled = false;
            subject.WriteToDiskMock = (f, d) => { writeToDiskCalled = true; };
            SetPrivateBudgetCollection(subject);
            var filename = "FooBar.xml";
            await subject.CreateNewAndSaveAsync(filename);
            Assert.IsTrue(writeToDiskCalled);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullMapper()
        {
            new XamlOnDiskBudgetRepository(
                new BucketBucketRepoAlwaysFind(),
                null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowWhenBucketRepositoryIsNull()
        {
            new XamlOnDiskBudgetRepository(
                null,
                new DtoMapperStub<BudgetCollectionDto, BudgetCollection>());
            Assert.Fail();
        }

        [TestMethod]
        public async Task LoadShouldCallInitialiseOnTheBucketRepository()
        {
            var mockBucketRepository = new Mock<IBudgetBucketRepository>();
            mockBucketRepository.Setup(m => m.Initialise(null));

            var mapperMock = new Mock<IDtoMapper<BudgetCollectionDto, BudgetCollection>>();
            var subject = new XamlOnDiskBudgetRepositoryTestHarness(
                mockBucketRepository.Object,
                mapperMock.Object);
            mapperMock.Setup(m => m.ToModel(It.IsAny<BudgetCollectionDto>())).Returns(BudgetModelTestData.CreateCollectionWith1And2);
            subject.FileExistsMock = f => true;

            subject.LoadFromDiskMock = OnLoadFromDiskMock;
            await subject.LoadAsync(TestDataConstants.BudgetCollectionTestDataFileName);

            mockBucketRepository.Verify();
        }

        [TestMethod]
        public async Task LoadShouldReturnACollectionAndSetFileName()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = OnLoadFromDiskMock;
            BudgetCollection collection = await subject.LoadAsync(TestDataConstants.BudgetCollectionTestDataFileName);

            Assert.AreEqual(TestDataConstants.BudgetCollectionTestDataFileName, collection.StorageKey);
        }

        [TestMethod]
        [ExpectedException(typeof(DataFormatException))]
        public async Task LoadShouldThrowIfAnyOtherExceptionIsThrownAndWrapItIntoFileFormatException()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = f => { throw new Exception(); };
            await subject.LoadAsync("SmellyPoo.xml");

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(DataFormatException))]
        public async Task LoadShouldThrowIfDeserialisedObjectIsNotBudgetCollection()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = f => new object();
            await subject.LoadAsync("SmellyPoo.xml");

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task LoadShouldThrowIfFileDoesntExist()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            subject.FileExistsMock = f => false;
            await subject.LoadAsync("SmellyPoo.xml");

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(DataFormatException))]
        public async Task LoadShouldThrowIfFileFormatIsInvalid()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = f => { throw new XamlObjectWriterException(); };
            await subject.LoadAsync("SmellyPoo.xml");

            Assert.Fail();
        }

        [TestMethod]
        public async Task MustBeAbleToLoadDemoBudgetFile()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = OnLoadFromDiskMock;
            BudgetCollection collection = await subject.LoadAsync(TestDataConstants.DemoBudgetFileName);

            Assert.AreEqual(TestDataConstants.DemoBudgetFileName, collection.StorageKey);
            Assert.AreEqual(1, collection.Count);
        }

        [TestMethod]
        public async Task MustBeAbleToLoadEmptyBudgetFile()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = OnLoadFromDiskMock;
            BudgetCollection collection = await subject.LoadAsync(TestDataConstants.EmptyBudgetFileName);

            Assert.AreEqual(TestDataConstants.EmptyBudgetFileName, collection.StorageKey);
            Assert.AreEqual(1, collection.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SaveShouldThrowIfLoadHasntBeenCalled()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            await subject.SaveAsync();
            Assert.Fail();
        }

        [TestMethod]
        public async Task SaveShouldWriteToDisk()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = Arrange();
            var writeToDiskCalled = false;
            subject.WriteToDiskMock = (filename, data) => { writeToDiskCalled = true; };
            SetPrivateBudgetCollection(subject);

            await subject.SaveAsync();

            Assert.IsTrue(writeToDiskCalled);
        }

        [TestInitialize]
        public void TestInitialise()
        {
        }

        private static BudgetCollectionDto OnLoadFromDiskMock(string f)
        {
            return EmbeddedResourceHelper.ExtractXaml<BudgetCollectionDto>(f);
        }

        private static void SetPrivateBudgetCollection(XamlOnDiskBudgetRepositoryTestHarness subject)
        {
            PrivateAccessor.SetField<XamlOnDiskBudgetRepository>(subject, "currentBudgetCollection", BudgetModelTestData.CreateCollectionWith1And2());
        }

        private XamlOnDiskBudgetRepositoryTestHarness Arrange(IBudgetBucketRepository bucketRepo = null)
        {
            if (bucketRepo == null)
            {
                bucketRepo = new InMemoryBudgetBucketRepository(new Mapper_BudgetBucketDto_BudgetBucket(new BudgetBucketFactory()));
            }

            return new XamlOnDiskBudgetRepositoryTestHarness(bucketRepo);
        }
    }
}