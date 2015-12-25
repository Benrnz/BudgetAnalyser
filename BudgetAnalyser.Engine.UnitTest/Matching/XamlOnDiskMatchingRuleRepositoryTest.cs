﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Matching.Data;
using BudgetAnalyser.Engine.UnitTest.Helper;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Matching
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
        public async Task LoadFromDemoFileShouldReturnMatchingRules()
        {
            XamlOnDiskMatchingRuleRepositoryTestHarness subject = Arrange();
            subject.LoadXamlFromDiskOveride = fileName => EmbeddedResourceHelper.ExtractText(TestDataConstants.DemoRulesFileName);
            IEnumerable<MatchingRule> results = await subject.LoadAsync("foo.bar");

            Assert.IsNotNull(results);
            Assert.IsTrue(results.Any());
        }

        [TestMethod]
        public async Task LoadShouldReturnMatchingRules()
        {
            XamlOnDiskMatchingRuleRepositoryTestHarness subject = Arrange();
            subject.LoadFromDiskOveride = fileName => MatchingRulesTestData.RawTestData1().ToList();
            IEnumerable<MatchingRule> results = await subject.LoadAsync("foo.bar");

            Assert.IsNotNull(results);
            Assert.IsTrue(results.Any());
        }

        [TestMethod]
        [ExpectedException(typeof(DataFormatException))]
        public async Task LoadShouldThrowGivenBadFileFormat()
        {
            XamlOnDiskMatchingRuleRepositoryTestHarness subject = Arrange();
            subject.LoadFromDiskOveride = fileName => { throw new Exception(); };
            await subject.LoadAsync("foo.bar");
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task LoadShouldThrowGivenNullFileName()
        {
            XamlOnDiskMatchingRuleRepositoryTestHarness subject = Arrange();
            await subject.LoadAsync(null);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task LoadShouldThrowIfFileNotFound()
        {
            XamlOnDiskMatchingRuleRepositoryTestHarness subject = Arrange();
            subject.ExistsOveride = filename => false;
            await subject.LoadAsync("Foo.bar");
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(DataFormatException))]
        public async Task LoadShouldThrowIfLoadedNullFile()
        {
            XamlOnDiskMatchingRuleRepositoryTestHarness subject = Arrange();
            subject.LoadFromDiskOveride = fileName => null;
            await subject.LoadAsync("foo.bar");
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task SaveShouldThrowGivenNullFileName()
        {
            XamlOnDiskMatchingRuleRepositoryTestHarness subject = Arrange();
            await subject.SaveAsync(MatchingRulesTestDataGenerated.TestData1(), null);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task SaveShouldThrowGivenNullRulesList()
        {
            XamlOnDiskMatchingRuleRepositoryTestHarness subject = Arrange();
            await subject.SaveAsync(null, "Foo.bar");
            Assert.Fail();
        }

        private XamlOnDiskMatchingRuleRepositoryTestHarness Arrange()
        {
            var bucketRepo = new BucketBucketRepoAlwaysFind();
            MatchingRulesTestDataGenerated.BucketRepo = bucketRepo;
            return new XamlOnDiskMatchingRuleRepositoryTestHarness(new DtoToMatchingRuleMapper(), new MatchingRuleDomainToDataMapper());
        }
    }
}