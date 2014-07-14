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
            XamlOnDiskBudgetRepositoryTestHarness subject = CreateSubject();
            subject.WriteToDiskMock = (f, d) => { };
            string filename = "FooBar.xml";
            BudgetCollection collection = subject.CreateNew(filename);
            Assert.AreEqual(filename, collection.FileName);
        }

        [TestMethod]
        public void CreateNewShouldReturnCollectionWithOneBudgetInIt()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = CreateSubject();
            subject.WriteToDiskMock = (f, d) => { };
            string filename = "FooBar.xml";
            BudgetCollection collection = subject.CreateNew(filename);

            Assert.IsTrue(collection.Count == 1);
        }

        [TestMethod]
        public void CreateNewShouldWriteToDisk()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = CreateSubject();
            bool writeToDiskCalled = false;
            subject.WriteToDiskMock = (f, d) => { writeToDiskCalled = true; };
            string filename = "FooBar.xml";
            subject.CreateNew(filename);
            Assert.IsTrue(writeToDiskCalled);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowWhenBucketRepositoryIsNull()
        {
            new XamlOnDiskBudgetRepository(
                null,
                new BudgetCollectionToDtoMapper(new BudgetModelToDtoMapper(), new BucketBucketRepoAlwaysFind(), new BudgetBucketToDtoMapper(new BudgetBucketFactory())),
                new DtoToBudgetCollectionMapper(new DtoToBudgetModelMapper(new BucketBucketRepoAlwaysFind())));
            Assert.Fail();
        }

        [TestMethod]
        public void LoadShouldCallInitialiseOnTheBucketRepository()
        {
            var mockBucketRepository = new Mock<IBudgetBucketRepository>();
            mockBucketRepository.Setup(m => m.Initialise(null));

            XamlOnDiskBudgetRepositoryTestHarness subject = CreateSubject(mockBucketRepository.Object);
            subject.FileExistsMock = f => true;

            subject.LoadFromDiskMock = OnLoadFromDiskMock;
            subject.Load(FileName1);

            mockBucketRepository.Verify();
        }

        [TestMethod]
        public void LoadShouldReturnACollectionAndSetFileName()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = CreateSubject();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = OnLoadFromDiskMock;
            BudgetCollection collection = subject.Load(FileName1);

            Assert.AreEqual(FileName1, collection.FileName);
        }

        [TestMethod]
        [ExpectedException(typeof(FileFormatException))]
        public void LoadShouldThrowIfAnyOtherExceptionIsThrownAndWrapItIntoFileFormatException()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = CreateSubject();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = f => { throw new Exception(); };
            subject.Load("SmellyPoo.xml");

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(FileFormatException))]
        public void LoadShouldThrowIfDeserialisedObjectIsNotBudgetCollection()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = CreateSubject();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = f => new object();
            subject.Load("SmellyPoo.xml");

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void LoadShouldThrowIfFileDoesntExist()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = CreateSubject();
            subject.FileExistsMock = f => false;
            subject.Load("SmellyPoo.xml");

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(FileFormatException))]
        public void LoadShouldThrowIfFileFormatIsInvalid()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = CreateSubject();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = f => { throw new XamlObjectWriterException(); };
            subject.Load("SmellyPoo.xml");

            Assert.Fail();
        }

        [TestMethod]
        public void MustBeAbleToLoadDemoBudgetFile()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = CreateSubject();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = OnLoadFromDiskMock;
            BudgetCollection collection = subject.Load(DemoBudgetFileName);

            Assert.AreEqual(DemoBudgetFileName, collection.FileName);
            Assert.AreEqual(1, collection.Count);
        }

        [TestMethod]
        public void MustBeAbleToLoadEmptyBudgetFile()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = CreateSubject();
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = OnLoadFromDiskMock;
            BudgetCollection collection = subject.Load(EmptyBudgetFileName);

            Assert.AreEqual(EmptyBudgetFileName, collection.FileName);
            Assert.AreEqual(1, collection.Count);
        }

        [TestMethod]
        public void SaveShouldWriteOutBuckets()
        {
            var bucketRepo = new InMemoryBudgetBucketRepository(new DtoToBudgetBucketMapper(new BudgetBucketFactory()));
            var subject = new XamlOnDiskBudgetRepositoryTestHarness(bucketRepo);

            string savedData = null;
            subject.WriteToDiskMock = (filename, data) => { savedData = data; };
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = OnLoadFromDiskMock;

            BudgetCollection collection = subject.Load(DemoBudgetFileName);
            subject.Save(collection);

            XNamespace ns = "clr-namespace:BudgetAnalyser.Engine.Budget.Data;assembly=BudgetAnalyser.Engine";
            XDocument document = XDocument.Parse(savedData);
            IEnumerable<XElement> bucketCollectionElements = document.Descendants(ns + "BudgetBucketDto");
            foreach (XElement element in bucketCollectionElements)
            {
                Console.WriteLine(element.FirstAttribute);
            }

            Assert.AreEqual(10, bucketCollectionElements.Count());
        }

        [TestMethod]
        public void SaveShouldWriteOutBucketsXPath()
        {
            var bucketRepo = new InMemoryBudgetBucketRepository(new DtoToBudgetBucketMapper(new BudgetBucketFactory()));
            var subject = new XamlOnDiskBudgetRepositoryTestHarness(bucketRepo);

            string savedData = null;
            subject.WriteToDiskMock = (filename, data) => { savedData = data; };
            subject.FileExistsMock = f => true;
            subject.LoadFromDiskMock = OnLoadFromDiskMock;

            BudgetCollection collection = subject.Load(DemoBudgetFileName);
            subject.Save(collection);

            XDocument document = XDocument.Parse(savedData);

            //var bucketCollectionElement = document.XPathEvaluate("/BudgetCollectionDto/BudgetCollectionDto.Buckets/scg:List", document);

            //XElement bucketCollectionElement = document.Root.Elements().First(e => e.Name.LocalName == "BudgetCollectionDto.Buckets");
            //foreach (var element in bucketCollectionElement.Elements().Single().Elements())
            //{
            //    Console.WriteLine(element.FirstAttribute);
            //}

            //Assert.AreEqual(10, bucketCollectionElement.Elements().Single().Elements().Count());
        }

        [TestMethod]
        public void SaveShouldWriteToDisk()
        {
            XamlOnDiskBudgetRepositoryTestHarness subject = CreateSubject();
            bool writeToDiskCalled = false;
            subject.WriteToDiskMock = (filename, data) => { writeToDiskCalled = true; };
            subject.Save(BudgetModelTestData.CreateCollectionWith1And2());

            Assert.IsTrue(writeToDiskCalled);
        }

        private XamlOnDiskBudgetRepositoryTestHarness CreateSubject(IBudgetBucketRepository bucketRepo = null)
        {
            if (bucketRepo == null)
            {
                bucketRepo = new InMemoryBudgetBucketRepository(new DtoToBudgetBucketMapper(new BudgetBucketFactory()));
            }

            return new XamlOnDiskBudgetRepositoryTestHarness(bucketRepo);
        }

        private BudgetCollectionDto OnLoadFromDiskMock(string f)
        {
            // this line of code is useful to figure out the name Vs has given the resource! The name is case sensitive.
            Assembly.GetExecutingAssembly().GetManifestResourceNames().ToList().ForEach(n => Debug.WriteLine(n));
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(f))
            {
                if (stream == null)
                {
                    throw new MissingManifestResourceException("Cannot find resource named: " + f);
                }

                return (BudgetCollectionDto)XamlServices.Load(new XamlXmlReader(stream));
            }
        }
    }
}