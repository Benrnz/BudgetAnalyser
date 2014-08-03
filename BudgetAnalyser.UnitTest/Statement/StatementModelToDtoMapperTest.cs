using System.Linq;
using AutoMapper;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.Statement.Data;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Statement
{
    [TestClass]
    public class StatementModelToDtoMapperTest
    {
        private TransactionSetDto  Result { get; set; }

        private StatementModel TestData
        {
            get { return StatementModelTestData.TestData2(); }
        }

        [TestMethod]
        public void ShouldMapFileName()
        {
            Assert.AreEqual(TestData.FileName, Result.FileName);
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

        [TestInitialize]
        public void TestInitialise()
        {
            AutoMapperConfigurationTest.AutoMapperConfiguration();

            Result = Mapper.Map<TransactionSetDto>(TestData);
        }
    }
}