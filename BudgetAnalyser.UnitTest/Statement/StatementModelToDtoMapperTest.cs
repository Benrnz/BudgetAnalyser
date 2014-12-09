using System;
using System.Linq;
using AutoMapper;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.Statement.Data;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Statement
{
    [TestClass]
    public class StatementModelToDtoMapperTest
    {
        private TransactionSetDto Result { get; set; }

        private StatementModel TestData
        {
            get { return StatementModelTestData.TestData2(); }
        }

        [TestMethod]
        public void ShouldMapFileName()
        {
            Assert.AreEqual(TestData.StorageKey, Result.FileName);
        }

        [TestMethod]
        public void ShouldMapLastImport()
        {
            Assert.AreEqual(TestData.LastImport, Result.LastImport);
        }

        [TestMethod]
        public void ShouldMapAllTransactions()
        {
            Assert.AreEqual(TestData.AllTransactions.Count(), Result.Transactions.Count());
        }

        [TestMethod]
        public void ShouldMapAllTransactionsAndHaveSameSum()
        {
            Assert.AreEqual(TestData.AllTransactions.Sum(t => t.Amount), Result.Transactions.Sum(t => t.Amount));
            Assert.AreEqual(TestData.AllTransactions.Sum(t => t.Date.Ticks), Result.Transactions.Sum(t => t.Date.Ticks));
        }

        [TestMethod]
        public void ShouldMapAllTransactionsEvenWhenFiltered()
        {
            var testData = TestData;
            testData.Filter(new GlobalFilterCriteria { BeginDate = new DateTime(2013, 07, 20), EndDate = new DateTime(2013, 08, 19) });
            Act(testData);

            Assert.AreEqual(TestData.AllTransactions.Count(), Result.Transactions.Count);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            
            Act(TestData);
        }

        private void Act(StatementModel testData)
        {
            Result = Mapper.Map<TransactionSetDto>(testData);
        }
    }
}