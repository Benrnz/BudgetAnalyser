using System;
using System.Linq;
using System.Reflection;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Budget
{
    [TestClass]
    public class BudgetModelToDataBudgetModelMapperTest
    {
        private BudgetModel TestData { get; set; }

        [TestMethod]
        public void EffectiveFromShouldBeMapped()
        {
            DataBudgetModel result = ArrangeAndAct();
            Assert.AreEqual(TestData.EffectiveFrom, result.EffectiveFrom);
        }

        [TestMethod]
        public void ExpensesBucketsShouldBeMapped()
        {
            DataBudgetModel result = ArrangeAndAct();
            Assert.AreEqual(TestData.Expenses.Sum(i => (long)i.Bucket.GetHashCode()), result.Expenses.Sum(i => (long)i.Bucket.GetHashCode()));
        }

        [TestMethod]
        public void ExpensesCountShouldBeMapped()
        {
            DataBudgetModel result = ArrangeAndAct();
            Assert.AreEqual(TestData.Expenses.Count(), result.Expenses.Count);
        }

        [TestMethod]
        public void ExpensesSumShouldBeMapped()
        {
            DataBudgetModel result = ArrangeAndAct();
            Assert.AreEqual(TestData.Expenses.Sum(i => i.Amount), result.Expenses.Sum(i => i.Amount));
        }

        [TestMethod]
        public void IncomesBucketsShouldBeMapped()
        {
            DataBudgetModel result = ArrangeAndAct();
            Assert.AreEqual(TestData.Incomes.Sum(i => (long)i.Bucket.GetHashCode()), result.Incomes.Sum(i => (long)i.Bucket.GetHashCode()));
        }

        [TestMethod]
        public void IncomesCountShouldBeMapped()
        {
            DataBudgetModel result = ArrangeAndAct();
            Assert.AreEqual(TestData.Incomes.Count(), result.Incomes.Count);
        }

        [TestMethod]
        public void IncomesSumShouldBeMapped()
        {
            DataBudgetModel result = ArrangeAndAct();
            Assert.AreEqual(TestData.Incomes.Sum(i => i.Amount), result.Incomes.Sum(i => i.Amount));
        }

        [TestMethod]
        public void LastModifiedCommentShouldBeMapped()
        {
            DataBudgetModel result = ArrangeAndAct();
            Assert.AreEqual(TestData.LastModifiedComment, result.LastModifiedComment);
        }

        [TestMethod]
        public void LastModifiedDateShouldBeMapped()
        {
            DataBudgetModel result = ArrangeAndAct();
            Assert.AreEqual(TestData.LastModified, result.LastModified);
        }

        [TestMethod]
        public void NameShouldBeMapped()
        {
            DataBudgetModel result = ArrangeAndAct();
            Assert.AreEqual(TestData.Name, result.Name);
        }

        [TestMethod]
        [Description("A test designed to break when new propperties are added to the DataBudgetModel. This is a trigger to update the mappers.")]
        public void NumberOfDataBudgetModelPropertiesShouldBe5()
        {
            int dataProperties = typeof(DataBudgetModel).CountProperties();
            Assert.AreEqual(6, dataProperties);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            TestData = BudgetModelTestData.CreateTestData1();
        }

        private DataBudgetModel ArrangeAndAct()
        {
            var mapper = new BudgetModelToDataBudgetModelMapper();
            return mapper.Map(TestData);
        }
    }
}