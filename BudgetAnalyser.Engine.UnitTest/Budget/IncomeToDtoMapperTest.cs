﻿using System.Linq;

using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.Engine.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Budget
{
    [TestClass]
    public class IncomeToDtoMapperTest
    {
        private IncomeDto Result { get; set; }
        private Income TestData => BudgetModelTestData.CreateTestData1().Incomes.First(e => e.Bucket.Code == TestDataConstants.IncomeBucketCode);

        [TestMethod]
        public void ShouldMapAmount()
        {
            Assert.AreEqual(1500M, Result.Amount);
        }

        [TestMethod]
        public void ShouldMapCode()
        {
            Assert.AreEqual(TestDataConstants.IncomeBucketCode, Result.BudgetBucketCode);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            Result = Mapper.Map<IncomeDto>(TestData);
        }
    }
}