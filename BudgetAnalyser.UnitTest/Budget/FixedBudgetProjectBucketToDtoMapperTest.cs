using System;
using AutoMapper;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rees.UnitTestUtilities;

namespace BudgetAnalyser.UnitTest.Budget
{
    [TestClass]
    public class FixedBudgetProjectBucketToDtoMapperTest
    {
        private const string FixedProjectCode = "Foo";
        private readonly DateTime testDataCreatedDate = new DateTime(2014, 09, 20);
        private BudgetBucketDto result;
        private BudgetBucket testData;

        [TestMethod]
        public void ShouldMapBucketType()
        {
            Assert.AreEqual(BucketDtoType.FixedBudgetProject, this.result.Type);
        }

        [TestMethod]
        public void ShouldMapCode()
        {
            Assert.AreEqual(FixedBudgetProjectBucket.CreateCode(FixedProjectCode), this.result.Code);
        }

        [TestMethod]
        public void ShouldMapCreatedDate()
        {
            Assert.AreEqual(this.testDataCreatedDate, ((FixedBudgetBucketDto)this.result).Created);
        }

        [TestMethod]
        public void ShouldMapDescription()
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(this.result.Description));
        }

        [TestMethod]
        public void ShouldMapFixedBudgetAmount()
        {
            Assert.AreEqual(1000, ((FixedBudgetBucketDto)this.result).FixedBudgetAmount);
        }

        [TestMethod]
        public void ShouldMapType()
        {
            Assert.IsInstanceOfType(this.result, typeof(FixedBudgetBucketDto));
        }

        [TestInitialize]
        public void TestInitialise()
        {
            this.testData = new FixedBudgetProjectBucket(FixedProjectCode, "Foo bar dum-de-dum", 1000);
            PrivateAccessor.SetProperty(this.testData, "Created", this.testDataCreatedDate);

            // When the buckets are mapped they are mapped using their base class, not their concrete type FixedBudgetProjectBucket.
            this.result = Mapper.Map<BudgetBucketDto>(this.testData);
        }
    }
}