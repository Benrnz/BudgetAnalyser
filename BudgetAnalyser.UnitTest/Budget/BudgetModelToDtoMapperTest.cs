using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Budget
{
    [TestClass]
    public class BudgetModelToDtoMapperTest
    {
        private BudgetModel TestData { get; set; }

        [TestMethod]
        public void EffectiveFromShouldBeMapped()
        {
            BudgetModelDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.EffectiveFrom, result.EffectiveFrom);
        }

        [TestMethod]
        public void ExpensesBucketsShouldBeMapped()
        {
            BudgetModelDto result = ArrangeAndAct();
            Assert.IsTrue(result.Expenses.All(dto => !string.IsNullOrWhiteSpace(dto.BudgetBucketCode)));
        }

        [TestMethod]
        public void ExpensesCountShouldBeMapped()
        {
            BudgetModelDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.Expenses.Count(), result.Expenses.Count);
        }

        [TestMethod]
        public void ExpensesSumShouldBeMapped()
        {
            BudgetModelDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.Expenses.Sum(i => i.Amount), result.Expenses.Sum(i => i.Amount));
        }

        [TestMethod]
        public void IncomesBucketsShouldBeMapped()
        {
            BudgetModelDto result = ArrangeAndAct();
            Assert.IsTrue(result.Incomes.All(dto => !string.IsNullOrWhiteSpace(dto.BudgetBucketCode)));
        }

        [TestMethod]
        public void IncomesCountShouldBeMapped()
        {
            BudgetModelDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.Incomes.Count(), result.Incomes.Count);
        }

        [TestMethod]
        public void IncomesSumShouldBeMapped()
        {
            BudgetModelDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.Incomes.Sum(i => i.Amount), result.Incomes.Sum(i => i.Amount));
        }

        [TestMethod]
        public void LastModifiedCommentShouldBeMapped()
        {
            BudgetModelDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.LastModifiedComment, result.LastModifiedComment);
        }

        [TestMethod]
        public void LastModifiedDateShouldBeMapped()
        {
            BudgetModelDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.LastModified, result.LastModified);
        }

        [TestMethod]
        public void NameShouldBeMapped()
        {
            BudgetModelDto result = ArrangeAndAct();
            Assert.AreEqual(TestData.Name, result.Name);
        }

        [TestMethod]
        [Description("A test designed to break when new propperties are added to the BudgetModelDto. This is a trigger to update the mappers.")]
        public void NumberOfDataBudgetModelPropertiesShouldBe5()
        {
            int dataProperties = typeof(BudgetModelDto).CountProperties();
            Assert.AreEqual(6, dataProperties);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            AutoMapperConfigurationTest.AutoMapperConfiguration();

            TestData = BudgetModelTestData.CreateTestData1();
        }

        private BudgetModelDto ArrangeAndAct()
        {
            var mapper = new BudgetModelToDtoMapper();
            return mapper.Map(TestData);
        }
    }
}