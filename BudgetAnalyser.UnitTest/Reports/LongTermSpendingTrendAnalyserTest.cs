using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.UnitTest.TestData;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser.UnitTest.Reports
{
    [TestClass]
    public class LongTermSpendingTrendAnalyserTest
    {

        // Used on demand for performance profiling
        //[TestMethod]
        //public void PerformanceTest()
        //{
        //    var accountTypeRepo = new InMemoryAccountTypeRepository();
        //    var budgetBucketRepo = new InMemoryBudgetBucketRepository();
        //    var budgetRepo = new XamlOnDiskBudgetRepository(
        //        budgetBucketRepo,
        //        new BudgetCollectionToBudgetCollectionDtoMapper(new BudgetModelToBudgetModelDtoMapper()),
        //        new BudgetCollectionDtoToBudgetCollectionMapper(new BudgetModelDtoToBudgetModelMapper()));
        //    budgetRepo.Load(@"C:\BudgetfileLocation.xml");
        //    var importer = new CsvOnDiskStatementModelRepositoryV1(
        //        accountTypeRepo,
        //        MessageBoxFake,
        //        budgetBucketRepo,
        //        new BankImportUtilities(new FakeLogger()),
        //        new FakeLogger());
        //    var statementModel = importer.Load(@"C:\StatementFilelocation.csv");

        //    var subject = Arrange(budgetBucketRepo);
        //    var stopwatch = Stopwatch.StartNew();
        //    subject.Analyse(
        //        statementModel,
        //        new GlobalFilterCriteria
        //        {
        //            BeginDate = new DateTime(2013, 12, 20),
        //            EndDate = new DateTime(2014, 6, 19),
        //        });
        //    stopwatch.Stop();

        //    Console.WriteLine("{0:N} ms", stopwatch.ElapsedMilliseconds);
        //    Output(subject);
        //}

        [TestMethod]
        public void OutputTest()
        {
            var subject = Arrange();

            subject.Analyse(StatementModelTestData.TestData2(), new GlobalFilterCriteria());

            Output(subject);
        }

        private static void Output(LongTermSpendingTrendAnalyser subject)
        {
            Console.WriteLine("{0} - {1} lines in the graph.", subject.Graph.GraphName, subject.Graph.Series.Count());
            foreach (var series in subject.Graph.Series)
            {
                Console.WriteLine("[{0}] {1}", series.SeriesName, series.Description);
                foreach (var data in series.Plots)
                {
                    Console.WriteLine("    {0}  {1:C}", data.Month, data.Amount);
                }
            }
        }

        [TestInitialize]
        public void TestInitialise()
        {
            MessageBoxFake = new Mock<IUserMessageBox>().Object;
        }

        private IUserMessageBox MessageBoxFake { get; set; }

        private LongTermSpendingTrendAnalyser Arrange(IBudgetBucketRepository bucketRepo = null)
        {
            if (bucketRepo == null)
            {
                var bucketRepositoryMock = new Mock<IBudgetBucketRepository>();
                var buckets = new List<BudgetBucket>
                {
                    StatementModelTestData.CarMtcBucket,
                    StatementModelTestData.HairBucket,
                    StatementModelTestData.PhoneBucket,
                    StatementModelTestData.PowerBucket,
                    StatementModelTestData.RegoBucket,
                    StatementModelTestData.IncomeBucket,
                };
                bucketRepositoryMock.Setup(r => r.Buckets).Returns(buckets);
                bucketRepo = bucketRepositoryMock.Object;
            }

            return new LongTermSpendingTrendAnalyser(bucketRepo);
        }
    }
}