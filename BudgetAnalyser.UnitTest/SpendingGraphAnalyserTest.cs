using System;
using System.Diagnostics;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Statement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser.UnitTest
{
    [TestClass]
    public class SpendingGraphAnalyserTest
    {
        public SpendingGraphAnalyserTest()
        {
            SurplusTestBucket = new SurplusBucket(SurplusBucket.SurplusCode, "foo");
        }

        private BudgetBucket SurplusTestBucket { get; set; }


        //[TestMethod]
        //public void TestMethod1()
        //{
        //    SpendingGraphAnalyser subject = CreateSubject();
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
        //    var budgets = new BudgetModelImporter(bucketRepository).LoadBudgetData(@"C:\Development\Brees_Unfuddle\BudgetAnalyserProject\Trunk\TestData\BudgetModel.xml");
        //    budgetModel = budgets.CurrentActiveBudget;
        //    var mock = new Mock<IUserMessageBox>();
        //    var statementImporter = new AnzAccountStatementImporterV1(mock.Object, bucketRepository);
        //    // TODO so is this.
        //    return statementImporter.ImportFromFile(@"C:\Development\Brees_Unfuddle\BudgetAnalyserProject\Trunk\TestData\8Months.csv", budgetModel, new ChequeAccount("Cheque"));
        //}

        private SpendingGraphAnalyser CreateSubject()
        {
            var analyser = new SpendingGraphAnalyser();
            return analyser;
        }
    }
}