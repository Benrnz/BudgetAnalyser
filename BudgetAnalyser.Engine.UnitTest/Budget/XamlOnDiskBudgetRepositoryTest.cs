using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xaml;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.Engine.Persistence;
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
        private Mock<IReaderWriterSelector> fileSelectorMock;

        [TestMethod]
        public async Task CreateNewShouldPopulateFileName()
        {
            XamlOnDiskBudgetRepository subject = Arrange();
            //subject.WriteToDiskMock = (f, d) => { };
            SetPrivateBudgetCollection(subject);
            var filename = "FooBar.xml";
            BudgetCollection collection = await subject.CreateNewAndSaveAsync(filename);
            Assert.AreEqual(filename, collection.StorageKey);
        }

        [TestMethod]
        public async Task CreateNewShouldReturnCollectionWithOneBudgetInIt()
        {
            XamlOnDiskBudgetRepository subject = Arrange();
            //subject.WriteToDiskMock = (f, d) => { };
            SetPrivateBudgetCollection(subject);
            var filename = "FooBar.xml";
            BudgetCollection collection = await subject.CreateNewAndSaveAsync(filename);

            Assert.IsTrue(collection.Count == 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateNewShouldThrowGivenEmptyFileName()
        {
            XamlOnDiskBudgetRepository subject = Arrange();
            await subject.CreateNewAndSaveAsync(string.Empty);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateNewShouldThrowGivenNullFileName()
        {
            XamlOnDiskBudgetRepository subject = Arrange();
            await subject.CreateNewAndSaveAsync(null);
            Assert.Fail();
        }

        [TestMethod]
        public async Task CreateNewShouldWriteToDisk()
        {
            XamlOnDiskBudgetRepository subject = Arrange();
            var writeToDiskCalled = false;
            //subject.WriteToDiskMock = (f, d) => { writeToDiskCalled = true; };
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
                null,
                this.fileSelectorMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowWhenBucketRepositoryIsNull()
        {
            new XamlOnDiskBudgetRepository(
                null,
                new DtoMapperStub<BudgetCollectionDto, BudgetCollection>(),
                this.fileSelectorMock.Object);
            Assert.Fail();
        }

        [TestMethod]
        public async Task LoadShouldCallInitialiseOnTheBucketRepository()
        {
            var mockBucketRepository = new Mock<IBudgetBucketRepository>();
            mockBucketRepository.Setup(m => m.Initialise(null));

            var mapperMock = new Mock<IDtoMapper<BudgetCollectionDto, BudgetCollection>>();
            var subject = new XamlOnDiskBudgetRepository(
                mockBucketRepository.Object,
                mapperMock.Object,
                this.fileSelectorMock.Object);
            mapperMock.Setup(m => m.ToModel(It.IsAny<BudgetCollectionDto>())).Returns(BudgetModelTestData.CreateCollectionWith1And2);
            //subject.FileExistsMock = f => true;

            //subject.LoadFromDiskMock = OnLoadFromDiskMock;
            await subject.LoadAsync(TestDataConstants.BudgetCollectionTestDataFileName, false);

            mockBucketRepository.Verify();
        }

        [TestMethod]
        public async Task LoadShouldReturnACollectionAndSetFileName()
        {
            XamlOnDiskBudgetRepository subject = Arrange();
            //subject.FileExistsMock = f => true;
            //subject.LoadFromDiskMock = OnLoadFromDiskMock;
            BudgetCollection collection = await subject.LoadAsync(TestDataConstants.BudgetCollectionTestDataFileName, false);

            Assert.AreEqual(TestDataConstants.BudgetCollectionTestDataFileName, collection.StorageKey);
        }

        [TestMethod]
        [ExpectedException(typeof(DataFormatException))]
        public async Task LoadShouldThrowIfAnyOtherExceptionIsThrownAndWrapItIntoFileFormatException()
        {
            XamlOnDiskBudgetRepository subject = Arrange();
            //subject.FileExistsMock = f => true;
            //subject.LoadFromDiskMock = f => { throw new Exception(); };
            await subject.LoadAsync("SmellyPoo.xml", false);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(DataFormatException))]
        public async Task LoadShouldThrowIfDeserialisedObjectIsNotBudgetCollection()
        {
            XamlOnDiskBudgetRepository subject = Arrange();
            //subject.FileExistsMock = f => true;
            //subject.LoadFromDiskMock = f => new object();
            await subject.LoadAsync("SmellyPoo.xml", false);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task LoadShouldThrowIfFileDoesntExist()
        {
            XamlOnDiskBudgetRepository subject = Arrange();
            //subject.FileExistsMock = f => false;
            await subject.LoadAsync("SmellyPoo.xml", false);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(DataFormatException))]
        public async Task LoadShouldThrowIfFileFormatIsInvalid()
        {
            XamlOnDiskBudgetRepository subject = Arrange();
            //subject.FileExistsMock = f => true;
            //subject.LoadFromDiskMock = f => { throw new XamlObjectWriterException(); };
            await subject.LoadAsync("SmellyPoo.xml", false);

            Assert.Fail();
        }

        [TestMethod]
        public async Task MustBeAbleToLoadDemoBudgetFile()
        {
            XamlOnDiskBudgetRepository subject = Arrange();
            //subject.FileExistsMock = f => true;
            //subject.LoadFromDiskMock = OnLoadFromDiskMock;
            BudgetCollection collection = await subject.LoadAsync(TestDataConstants.DemoBudgetFileName, false);

            Assert.AreEqual(TestDataConstants.DemoBudgetFileName, collection.StorageKey);
            Assert.AreEqual(1, collection.Count);
        }

        [TestMethod]
        public async Task MustBeAbleToLoadEmptyBudgetFile()
        {
            XamlOnDiskBudgetRepository subject = Arrange();
            //subject.FileExistsMock = f => true;
            //subject.LoadFromDiskMock = OnLoadFromDiskMock;
            BudgetCollection collection = await subject.LoadAsync(TestDataConstants.EmptyBudgetFileName, false);

            Assert.AreEqual(TestDataConstants.EmptyBudgetFileName, collection.StorageKey);
            Assert.AreEqual(1, collection.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SaveShouldThrowIfLoadHasntBeenCalled()
        {
            XamlOnDiskBudgetRepository subject = Arrange();
            await subject.SaveAsync();
            Assert.Fail();
        }

        [TestMethod]
        public async Task SaveShouldWriteToDisk()
        {
            XamlOnDiskBudgetRepository subject = Arrange();
            var writeToDiskCalled = false;
            //subject.WriteToDiskMock = (filename, data) => { writeToDiskCalled = true; };
            SetPrivateBudgetCollection(subject);

            await subject.SaveAsync();

            Assert.IsTrue(writeToDiskCalled);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            this.fileSelectorMock = new Mock<IReaderWriterSelector>();
        }

        private static BudgetCollectionDto OnLoadFromDiskMock(string f)
        {
            return EmbeddedResourceHelper.ExtractXaml<BudgetCollectionDto>(f);
        }

        private static void SetPrivateBudgetCollection(XamlOnDiskBudgetRepository subject)
        {
            PrivateAccessor.SetField<XamlOnDiskBudgetRepository>(subject, "currentBudgetCollection", BudgetModelTestData.CreateCollectionWith1And2());
        }

        private XamlOnDiskBudgetRepository Arrange(IBudgetBucketRepository bucketRepo = null)
        {
            if (bucketRepo == null)
            {
                bucketRepo = new InMemoryBudgetBucketRepository(new Mapper_BudgetBucketDto_BudgetBucket(new BudgetBucketFactory()));
            }

            return new XamlOnDiskBudgetRepository(
                bucketRepo, 
                new Mapper_BudgetCollectionDto_BudgetCollection(
                    bucketRepo, 
                    new Mapper_BudgetBucketDto_BudgetBucket(new BudgetBucketFactory()), 
                    new Mapper_BudgetModelDto_BudgetModel(bucketRepo)), 
                this.fileSelectorMock.Object);
        }
    }
}