using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Reports;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Reports
{
    [TestClass]
    public class SpendingGraphAnalyserTest
    {
        public SpendingGraphAnalyserTest()
        {
            this.SurplusTestBucket = new SurplusBucket();
        }

        private BudgetBucket SurplusTestBucket { get; set; }


        //[TestMethod]
        //public void TestMethod1()
        //{
        //    BurnDownChartAnalyserResult subject = CreateSubject();
        //    BudgetModel budgetModel;
        //    StatementModel statementModel = CreateStatementModel(out budgetModel);
        //    var criteria = new GlobalFilterCriteria { BeginDate = new DateTime(2013, 5, 1), EndDate = new DateTime(2013, 5, 31) };
        //    statementModel.Filter(criteria);

        //    subject.Analyse(statementModel, budgetModel, new[] { SurplusTestBucket }, criteria);
        //    foreach (var line in subject.ActualSpending)
        //    {
        //        Debug.WriteLine("    " + line.Key + " " + line.Value);
        //    }
        //}

        //private StatementModel CreateStatementModel(out BudgetModel budgetModel)
        //{
        //    // TODO this is shite.
        //    var bucketRepository = new InMemoryBudgetBucketRepository();
        //    var budgets = new BudgetModelImporter(bucketRepository).LoadBudgetData(@"C:\Development\xxx\BudgetAnalyserProject\Trunk\TestData\BudgetModel.xml");
        //    budgetModel = budgets.CurrentActiveBudget;
        //    var mock = new Mock<IUserMessageBox>();
        //    var statementImporter = new AnzAccountStatementImporterV1(mock.Object, bucketRepository);
        //    return statementImporter.ImportFromFile(@"C:\Development\xxx\BudgetAnalyserProject\Trunk\TestData\8Months.csv", budgetModel, new ChequeAccount("Cheque"));
        //}

        //private BurnDownChartAnalyserResult CreateSubject()
        //{
        //    var analyser = new BurnDownChartAnalyserResult();
        //    return analyser;
        //}
    }
}