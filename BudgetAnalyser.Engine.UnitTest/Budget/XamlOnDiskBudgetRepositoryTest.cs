using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.UnitTest.Helper;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Portable.Xaml;
using Rees.TangyFruitMapper;
using Rees.UnitTestUtilities;

namespace BudgetAnalyser.Engine.UnitTest.Budget
{
    [TestClass]
    public class XamlOnDiskBudgetRepositoryTest
    {
        private Mock<IReaderWriterSelector> mockFileSelector;
        private Mock<IFileReaderWriter> mockReaderWriter;

        [TestMethod]
        public async Task CreateNewShouldPopulateFileName()
        {
            var subject = Arrange();
            SetPrivateBudgetCollection(subject);
            var filename = "FooBar.xml";
            var collection = await subject.CreateNewAndSaveAsync(filename);
            Assert.AreEqual(filename, collection.StorageKey);
        }

        [TestMethod]
        public async Task CreateNewShouldReturnCollectionWithOneBudgetInIt()
        {
            var subject = Arrange();
            //subject.WriteToDiskMock = (f, d) => { };
            SetPrivateBudgetCollection(subject);
            var filename = "FooBar.xml";
            var collection = await subject.CreateNewAndSaveAsync(filename);

            Assert.IsTrue(collection.Count == 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateNewShouldThrowGivenEmptyFileName()
        {
            var subject = Arrange();
            await subject.CreateNewAndSaveAsync(string.Empty);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateNewShouldThrowGivenNullFileName()
        {
            var subject = Arrange();
            await subject.CreateNewAndSaveAsync(null);
            Assert.Fail();
        }

        [TestMethod]
        public async Task CreateNewShouldWriteToDisk()
        {
            var subject = Arrange();
            SetPrivateBudgetCollection(subject);
            var filename = "FooBar.xml";

            await subject.CreateNewAndSaveAsync(filename);

            this.mockFileSelector.Verify(m => m.SelectReaderWriter(It.IsAny<bool>()));
            this.mockReaderWriter.Verify(m => m.WriteToDiskAsync(It.IsAny<string>(), It.IsAny<string>()));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullMapper()
        {
            new XamlOnDiskBudgetRepository(
                new BucketBucketRepoAlwaysFind(),
                null,
                this.mockFileSelector.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowWhenBucketRepositoryIsNull()
        {
            new XamlOnDiskBudgetRepository(
                null,
                new DtoMapperStub<BudgetCollectionDto, BudgetCollection>(),
                this.mockFileSelector.Object);
            Assert.Fail();
        }

        [TestMethod]
        public async Task LoadShouldCallInitialiseOnTheBucketRepository()
        {
            var mockBucketRepository = new Mock<IBudgetBucketRepository>();
            mockBucketRepository.Setup(m => m.Initialise(null));
            var subject = Arrange();

            var data = GetType().Assembly.ExtractEmbeddedResourceAsText(TestDataConstants.BudgetCollectionTestDataFileName);
            this.mockReaderWriter.Setup(m => m.FileExists(It.IsAny<string>())).Returns(true);
            this.mockReaderWriter.Setup(m => m.LoadFromDiskAsync(It.IsAny<string>())).ReturnsAsync(data);

            await subject.LoadAsync(TestDataConstants.BudgetCollectionTestDataFileName, false);

            mockBucketRepository.Verify();
        }

        [TestMethod]
        public async Task LoadShouldReturnACollectionAndSetFileName()
        {
            var subject = Arrange();
            var data = GetType().Assembly.ExtractEmbeddedResourceAsText(TestDataConstants.BudgetCollectionTestDataFileName);
            this.mockReaderWriter.Setup(m => m.FileExists(It.IsAny<string>())).Returns(true);
            this.mockReaderWriter.Setup(m => m.LoadFromDiskAsync(It.IsAny<string>())).ReturnsAsync(data);

            var collection = await subject.LoadAsync(TestDataConstants.BudgetCollectionTestDataFileName, false);

            Assert.AreEqual(TestDataConstants.BudgetCollectionTestDataFileName, collection.StorageKey);
        }

        [TestMethod]
        [ExpectedException(typeof(DataFormatException))]
        public async Task LoadShouldThrowIfAnyOtherExceptionIsThrownAndWrapItIntoFileFormatException()
        {
            var subject = Arrange();
            this.mockReaderWriter.Setup(m => m.FileExists(It.IsAny<string>())).Returns(true);

            await subject.LoadAsync("SmellyPoo.xml", false);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(DataFormatException))]
        public async Task LoadShouldThrowIfDeserialisedObjectIsNotBudgetCollection()
        {
            var subject = Arrange();
            this.mockReaderWriter.Setup(m => m.FileExists(It.IsAny<string>())).Returns(true);
            this.mockReaderWriter.Setup(m => m.LoadFromDiskAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

            await subject.LoadAsync("SmellyPoo.xml", false);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task LoadShouldThrowIfFileDoesntExist()
        {
            var subject = Arrange();
            //subject.FileExistsMock = f => false;
            await subject.LoadAsync("SmellyPoo.xml", false);

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(DataFormatException))]
        public async Task LoadShouldThrowIfFileFormatIsInvalid()
        {
            var subject = Arrange();
            this.mockReaderWriter.Setup(m => m.FileExists(It.IsAny<string>())).Returns(true);
            this.mockReaderWriter.Setup(m => m.LoadFromDiskAsync(It.IsAny<string>())).ThrowsAsync(new XamlObjectWriterException());

            await subject.LoadAsync("SmellyPoo.xml", false);

            Assert.Fail();
        }

        [TestMethod]
        public async Task MustBeAbleToLoadDemoBudgetFile()
        {
            var subject = Arrange();
            var data = GetType().Assembly.ExtractEmbeddedResourceAsText(TestDataConstants.DemoBudgetFileName);
            this.mockReaderWriter.Setup(m => m.FileExists(It.IsAny<string>())).Returns(true);
            this.mockReaderWriter.Setup(m => m.LoadFromDiskAsync(It.IsAny<string>())).ReturnsAsync(data);

            var collection = await subject.LoadAsync(TestDataConstants.DemoBudgetFileName, false);

            Assert.AreEqual(TestDataConstants.DemoBudgetFileName, collection.StorageKey);
            Assert.AreEqual(1, collection.Count);
        }

        [TestMethod]
        public async Task MustBeAbleToLoadEmptyBudgetFile()
        {
            var subject = Arrange();
            var data = GetType().Assembly.ExtractEmbeddedResourceAsText(TestDataConstants.EmptyBudgetFileName);
            this.mockReaderWriter.Setup(m => m.FileExists(It.IsAny<string>())).Returns(true);
            this.mockReaderWriter.Setup(m => m.LoadFromDiskAsync(It.IsAny<string>())).ReturnsAsync(data);

            var collection = await subject.LoadAsync(TestDataConstants.EmptyBudgetFileName, false);

            Assert.AreEqual(TestDataConstants.EmptyBudgetFileName, collection.StorageKey);
            Assert.AreEqual(1, collection.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SaveShouldThrowIfLoadHasntBeenCalled()
        {
            var subject = Arrange();
            await subject.SaveAsync();
            Assert.Fail();
        }

        [TestMethod]
        public async Task SaveShouldWriteToDisk()
        {
            var subject = Arrange();
            this.mockReaderWriter.Setup(m => m.WriteToDiskAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            SetPrivateBudgetCollection(subject);

            await subject.SaveAsync();

            this.mockReaderWriter.VerifyAll();
        }

        [TestInitialize]
        public void TestInitialise()
        {
            this.mockFileSelector = new Mock<IReaderWriterSelector>();
            this.mockReaderWriter = new Mock<IFileReaderWriter>();
            this.mockFileSelector.Setup(m => m.SelectReaderWriter(It.IsAny<bool>())).Returns(this.mockReaderWriter.Object);
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
                this.mockFileSelector.Object);
        }

        private static BudgetCollectionDto OnLoadFromDiskMock(string f)
        {
            return typeof(XamlOnDiskBudgetRepositoryTest).Assembly.ExtractEmbeddedResourceAsXamlObject<BudgetCollectionDto>(f);
        }

        private static void SetPrivateBudgetCollection(XamlOnDiskBudgetRepository subject)
        {
            PrivateAccessor.SetField<XamlOnDiskBudgetRepository>(subject, "currentBudgetCollection", BudgetModelTestData.CreateCollectionWith1And2());
        }
    }
}