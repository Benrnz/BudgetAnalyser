using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Budget
{
    [TestClass]
    public class BudgetCollectionToDtoMapperTest2
    {
        private BudgetCollectionDto Result { get; set; }
        private BudgetCollection TestData { get; set; }
        private IEnumerable<BudgetBucket> TestDataBuckets { get; set; }

        [TestMethod]
        public void OutputBudgetCollectionDtoResult()
        {
            decimal expensesTotal = 0;
            Console.WriteLine("TestData (BudgetCollection)");
            foreach (var budget in TestData)
            {
                Console.WriteLine("Budget: '{0}' Effective From: {1}", budget.Name, budget.EffectiveFrom);
                foreach (var expense in budget.Expenses)
                {
                    Console.WriteLine("    Expense: {0} {1}", expense.Bucket.Code, expense.Amount);
                    expensesTotal += expense.Amount;
                }
            }
            Console.WriteLine("Expenses Total: " + expensesTotal);
            Console.WriteLine();

            expensesTotal = 0;
            Console.WriteLine("Result (BudgetCollectionDto)");
            foreach (var budget in Result.Budgets)
            {
                Console.WriteLine("Budget: '{0}' Effective From: {1}", budget.Name, budget.EffectiveFrom);
                foreach (var expense in budget.Expenses)
                {
                    Console.WriteLine("    Expense: {0} {1}", expense.BudgetBucketCode, expense.Amount);
                    expensesTotal += expense.Amount;
                }
            }

            Console.WriteLine("Expenses Total: " + expensesTotal);
        }

        [TestMethod]
        public void ShouldMapBudgetsCorrectly()
        {
            Assert.AreEqual(TestData.Sum(b => b.Expenses.Sum(e => e.Amount)), Result.Budgets.Sum(b => b.Expenses.Sum(e => e.Amount)));
        }

        [TestMethod]
        public void ShouldMapCorrectNumberOfBuckets()
        {
            Console.WriteLine("TestDataBuckets.Count = " + TestDataBuckets.Count());
            Console.WriteLine("Result.Buckets.Count = " + Result.Buckets.Count());

            Assert.IsTrue(Result.Buckets.Any());
            foreach (var bucket in TestDataBuckets)
            {
                Assert.IsTrue(Result.Buckets.Any(b => b.Code == bucket.Code));
            }
        }

        [TestMethod]
        public void ShouldMapFileName()
        {
            Assert.AreEqual(TestData.StorageKey, Result.StorageKey);
        }

        [TestMethod]
        public void ShouldMapSameNumberOfBudgets()
        {
            Assert.AreEqual(TestData.Count, Result.Budgets.Count);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            var bucketRepo = new BucketBucketRepoAlwaysFind();
            TestData = BudgetModelTestData.CreateCollectionWith1And2();

            // ExtractXaml All Buckets from The Test Data.
            TestDataBuckets = TestData.SelectMany(b => b.Expenses.Cast<BudgetItem>())
                .Union(TestData.SelectMany(b => b.Incomes))
                .Select(x => x.Bucket)
                .Distinct();

            // Preload the buckets into the bucket repo used by the Mapper.
            foreach (var bucket in TestDataBuckets)
            {
                bucketRepo.GetByCode(bucket.Code);
            }

            var subject = new MapperBudgetCollectionDtoBudgetCollection(
                bucketRepo,
                new MapperBudgetBucketDtoBudgetBucket(new BudgetBucketFactory()),
                new MapperBudgetModelDtoBudgetModel(bucketRepo));
            Result = subject.ToDto(TestData);
        }
    }
}
