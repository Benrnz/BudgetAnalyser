using System;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Budget
{
    [TestClass]
    public class BudgetModelToDataBudgetModelMapperTest
    {
        private BudgetModel TestData { get; set; }

        [TestInitialize]
        public void TestInitialise()
        {
            this.TestData = BudgetModelTestData.CreateTestData1();
        }

        [TestMethod]
        [Description("A test designed to break when new propperties are added to the DataBudgetModel. This is a trigger to update the mappers.")]
        public void NumberOfDataBudgetModelPropertiesShouldBe5()
        {
            var dataProperties = CountProperties(typeof (DataBudgetModel));
            Assert.AreEqual(6, dataProperties);
        }

        [TestMethod]
        public void EffectiveFromShouldBeMapped()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(this.TestData.EffectiveFrom, result.EffectiveFrom);
        }

        [TestMethod]
        public void LastModifiedDateShouldBeMapped()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(this.TestData.LastModified, result.LastModified);
        }

        [TestMethod]
        public void NameShouldBeMapped()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(this.TestData.Name, result.Name);
        }

        [TestMethod]
        public void LastModifiedCommentShouldBeMapped()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(this.TestData.LastModifiedComment, result.LastModifiedComment);
        }

        [TestMethod]
        public void ExpensesCountShouldBeMapped()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(this.TestData.Expenses.Count(), result.Expenses.Count);
        }

        [TestMethod]
        public void IncomesCountShouldBeMapped()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(this.TestData.Incomes.Count(), result.Incomes.Count);
        }

        [TestMethod]
        public void IncomesSumShouldBeMapped()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(this.TestData.Incomes.Sum(i => i.Amount), result.Incomes.Sum(i => i.Amount));
        }

        [TestMethod]
        public void ExpensesSumShouldBeMapped()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(this.TestData.Expenses.Sum(i => i.Amount), result.Expenses.Sum(i => i.Amount));
        }

        [TestMethod]
        public void ExpensesBucketsShouldBeMapped()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(this.TestData.Expenses.Sum(i => (long)i.Bucket.GetHashCode()), result.Expenses.Sum(i => (long)i.Bucket.GetHashCode()));
        }

        [TestMethod]
        public void IncomesBucketsShouldBeMapped()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(this.TestData.Incomes.Sum(i => (long)i.Bucket.GetHashCode()), result.Incomes.Sum(i => (long)i.Bucket.GetHashCode()));
        }

        private DataBudgetModel ArrangeAndAct()
        {
            var mapper = new BudgetModelToDataBudgetModelMapper();
            return mapper.Map(this.TestData);
        }

        private int CountProperties(Type type)
        {
            var properties = type.GetProperties();
            properties.ToList().ForEach(p => Console.WriteLine(p.Name));
            return properties.Length;
        }
    }
}
