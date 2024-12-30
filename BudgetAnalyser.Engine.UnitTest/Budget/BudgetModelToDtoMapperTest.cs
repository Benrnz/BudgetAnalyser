using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Budget
{
    [TestClass]
    public class BudgetModelToDtoMapperTest
    {
        private BudgetModel TestData { get; set; }

        [TestMethod]
        public void EffectiveFromShouldBeMapped()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(TestData.EffectiveFrom, result.EffectiveFrom);
        }

        [TestMethod]
        public void ExpensesBucketsShouldBeMapped()
        {
            var result = ArrangeAndAct();
            Assert.IsTrue(result.Expenses.All(dto => !string.IsNullOrWhiteSpace(dto.BudgetBucketCode)));
        }

        [TestMethod]
        public void ExpensesCountShouldBeMapped()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(TestData.Expenses.Count(), result.Expenses.Count);
        }

        [TestMethod]
        public void ExpensesSumShouldBeMapped()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(TestData.Expenses.Sum(i => i.Amount), result.Expenses.Sum(i => i.Amount));
        }

        [TestMethod]
        public void IncomesBucketsShouldBeMapped()
        {
            var result = ArrangeAndAct();
            Assert.IsTrue(result.Incomes.All(dto => !string.IsNullOrWhiteSpace(dto.BudgetBucketCode)));
        }

        [TestMethod]
        public void IncomesCountShouldBeMapped()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(TestData.Incomes.Count(), result.Incomes.Count);
        }

        [TestMethod]
        public void IncomesSumShouldBeMapped()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(TestData.Incomes.Sum(i => i.Amount), result.Incomes.Sum(i => i.Amount));
        }

        [TestMethod]
        public void LastModifiedCommentShouldBeMapped()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(TestData.LastModifiedComment, result.LastModifiedComment);
        }

        [TestMethod]
        public void LastModifiedDateShouldBeMapped()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(TestData.LastModified, result.LastModified);
        }

        [TestMethod]
        public void NameShouldBeMapped()
        {
            var result = ArrangeAndAct();
            Assert.AreEqual(TestData.Name, result.Name);
        }

        [TestMethod]
        [Description("A test designed to break when new properties are added to the BudgetModelDto. This is a trigger to update the mappers.")]
        public void NumberOfDataBudgetModelPropertiesShouldBe7()
        {
            var dataProperties = typeof(BudgetModelDto).CountProperties();
            Assert.AreEqual(7, dataProperties);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            TestData = BudgetModelTestData.CreateTestData1();
        }

        private BudgetModelDto ArrangeAndAct()
        {
            var mapper = new MapperBudgetModelDtoBudgetModel(new BucketBucketRepoAlwaysFind());
            return mapper.ToDto(TestData);
        }
    }
}
