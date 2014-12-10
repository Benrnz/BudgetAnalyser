using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Matching.Data;
using BudgetAnalyser.UnitTest.TestData;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Matching
{
    [TestClass]
    public class XamlOnDiskMatchingRuleRepositoryTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowWhenGivenNullMapper()
        {
            new XamlOnDiskMatchingRuleRepository(null, new MatchingRuleDomainToDataMapper());
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowWhenGivenNullMapper2()
        {
            new XamlOnDiskMatchingRuleRepository(new DtoToMatchingRuleMapper(), null);
            Assert.Fail();
        }

        [TestMethod]
        public void LoadShouldReturnMatchingRules()
        {
            XamlOnDiskMatchingRuleRepositoryTestHarness subject = Arrange();
            subject.LoadFromDiskOveride = fileName => MatchingRulesTestData.RawTestData1().ToList();
            IEnumerable<MatchingRule> results = subject.LoadRules("foo.bar");

            Assert.IsNotNull(results);
            Assert.IsTrue(results.Any());
        }

        [TestMethod]
        [ExpectedException(typeof(DataFormatException))]
        public void LoadShouldThrowGivenBadFileFormat()
        {
            XamlOnDiskMatchingRuleRepositoryTestHarness subject = Arrange();
            subject.LoadFromDiskOveride = fileName => { throw new Exception(); };
            subject.LoadRules("foo.bar");
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LoadShouldThrowGivenNullFileName()
        {
            XamlOnDiskMatchingRuleRepositoryTestHarness subject = Arrange();
            subject.LoadRules(null);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void LoadShouldThrowIfFileNotFound()
        {
            XamlOnDiskMatchingRuleRepositoryTestHarness subject = Arrange();
            subject.ExistsOveride = filename => false;
            subject.LoadRules("Foo.bar");
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(DataFormatException))]
        public void LoadShouldThrowIfLoadedNullFile()
        {
            XamlOnDiskMatchingRuleRepositoryTestHarness subject = Arrange();
            subject.LoadFromDiskOveride = fileName => null;
            subject.LoadRules("foo.bar");
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SaveShouldThrowGivenNullRulesList()
        {
            var subject = Arrange();
            subject.SaveRules(null, "Foo.bar");
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SaveShouldThrowGivenNullFileName()
        {
            var subject = Arrange();
            subject.SaveRules(MatchingRulesTestDataGenerated.TestData1(), null);
            Assert.Fail();
        }

        [TestMethod]
        public void SaveShouldRaiseApplicationEvent()
        {
            var subject = Arrange();
            bool eventRaised = false;
            subject.ApplicationEvent += (s, e) => eventRaised = true;
            subject.SaveRules(MatchingRulesTestDataGenerated.TestData1(), "foo.bar");

            Assert.IsTrue(eventRaised);
        }

        private XamlOnDiskMatchingRuleRepositoryTestHarness Arrange()
        {
            var bucketRepo = new BucketBucketRepoAlwaysFind();
            MatchingRulesTestDataGenerated.BucketRepo = bucketRepo;
            return new XamlOnDiskMatchingRuleRepositoryTestHarness(new DtoToMatchingRuleMapper(), new MatchingRuleDomainToDataMapper());
        }
    }
}