using BudgetAnalyser.Budget;
using BudgetAnalyser.Engine.Reports;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Wpf.UnitTest
{
    [TestClass]
    public class BucketSpendingControllerTest
    {
        [TestMethod]
        public void TestChartData()
        {
            //var subject = this.CreateSubject();
            //BudgetModel budgetModel;
            //var criteria = new GlobalFilterCriteria { BeginDate = new DateTime(2013, 5, 1), EndDate = new DateTime(2013, 5, 31) };
            //var statementModel = CreateStatementModel(out budgetModel);
            //statementModel.Filter(criteria);
            //var bucket = new SpentMonthlyExpense("FOOD", "Food");
            //subject.Load(statementModel, budgetModel, bucket, criteria);
            //foreach (var line in subject.ActualSpending)
            //{
            //    Debug.WriteLine(line.Key + " - " + line.Value);
            //}
            Assert.Inconclusive();
        }

        [TestMethod]
        public void TestChartDataAggregationPerformance()
        {
            //var subject = this.CreateSubject();
            //BudgetModel budgetModel;
            //var statementModel = CreateStatementModel(out budgetModel);
            //var bucket = new SpentMonthlyExpense("FOOD", "Food");
            //subject.Load(statementModel, budgetModel, bucket, new GlobalFilterCriteria { BeginDate = new DateTime(2013, 1, 1), EndDate = new DateTime(2013, 6, 1) });
            Assert.Inconclusive();
        }

        private BucketSpendingController CreateSubject()
        {
            var controller = new BucketSpendingController(new SpendingGraphAnalyser());
            return controller;
        }
    }
}