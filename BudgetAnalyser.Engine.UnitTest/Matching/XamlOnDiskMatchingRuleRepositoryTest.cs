using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetAnalyser.Encryption;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Matching.Data;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BudgetAnalyser.Engine.UnitTest.Matching
{
    [TestClass]
    public class XamlOnDiskMatchingRuleRepositoryTest
    {
        private Mock<IFileReaderWriter> mockReaderWriter;
        private Mock<IReaderWriterSelector> mockSelector;

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowWhenGivenNullMapper()
        {
            new XamlOnDiskMatchingRuleRepository(null, new FakeLogger(), new Mock<IReaderWriterSelector>().Object);
            Assert.Fail();
        }

        [TestMethod]
        public async Task LoadFromDemoFileShouldReturnMatchingRules()
        {
            var subject = ArrangeUsingEmbeddedResources();
            IEnumerable<MatchingRule> results = await subject.LoadAsync(TestDataConstants.DemoRulesFileName, false);

            Assert.IsNotNull(results);
            Assert.IsTrue(results.Any());
        }

        [TestMethod]
        public async Task LoadShouldReturnMatchingRules()
        {
            var subject = ArrangeUsingEmbeddedResources();
            subject.LoadFromDiskOveride = fileName => MatchingRulesTestData.RawTestData1().ToList();
            IEnumerable<MatchingRule> results = await subject.LoadAsync("foo.bar", false);

            Assert.IsNotNull(results);
            Assert.IsTrue(results.Any());
        }

        [TestMethod]
        [ExpectedException(typeof(DataFormatException))]
        public async Task LoadShouldThrowGivenBadFileFormat()
        {
            var subject = ArrangeUsingEmbeddedResources();
            subject.LoadFromDiskOveride = fileName => { throw new Exception(); };
            await subject.LoadAsync("foo.bar", false);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task LoadShouldThrowGivenNullFileName()
        {
            var subject = ArrangeUsingEmbeddedResources();
            await subject.LoadAsync(null, false);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task LoadShouldThrowIfFileNotFound()
        {
            var subject = ArrangeUsingMocks();
            subject.ExistsOveride = filename => false;
            await subject.LoadAsync("Foo.bar", false);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(DataFormatException))]
        public async Task LoadShouldThrowIfLoadedNullFile()
        {
            var subject = ArrangeUsingEmbeddedResources();
            subject.LoadFromDiskOveride = fileName => null;
            await subject.LoadAsync("foo.bar", false);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task SaveShouldThrowGivenNullFileName()
        {
            var subject = ArrangeUsingEmbeddedResources();
            await subject.SaveAsync(MatchingRulesTestDataGenerated.TestData1(), null, false);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task SaveShouldThrowGivenNullRulesList()
        {
            var subject = ArrangeUsingEmbeddedResources();
            await subject.SaveAsync(null, "Foo.bar", false);
            Assert.Fail();
        }

        [TestInitialize]
        public void TestSetup()
        {
            this.mockReaderWriter = new Mock<IFileReaderWriter>();
            this.mockSelector = new Mock<IReaderWriterSelector>();
            this.mockSelector.Setup(m => m.SelectReaderWriter(It.IsAny<bool>())).Returns(this.mockReaderWriter.Object);
        }

        private XamlOnDiskMatchingRuleRepositoryTestHarness ArrangeUsingEmbeddedResources()
        {
            var bucketRepo = new BucketBucketRepoAlwaysFind();
            MatchingRulesTestDataGenerated.BucketRepo = bucketRepo;

            return new XamlOnDiskMatchingRuleRepositoryTestHarness(
                new Mapper_MatchingRuleDto_MatchingRule(new BucketBucketRepoAlwaysFind()),
                new LocalDiskReaderWriterSelector(
                    new [] { new EmbeddedResourceFileReaderWriter() }));
        }

        private XamlOnDiskMatchingRuleRepositoryTestHarness ArrangeUsingMocks()
        {
            var bucketRepo = new BucketBucketRepoAlwaysFind();
            MatchingRulesTestDataGenerated.BucketRepo = bucketRepo;

            return new XamlOnDiskMatchingRuleRepositoryTestHarness(
                new Mapper_MatchingRuleDto_MatchingRule(new BucketBucketRepoAlwaysFind()),
                this.mockSelector.Object);
        }
    }
}