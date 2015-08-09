using AutoMapper;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Ledger
{
    [TestClass]
    public class BankBalanceToDtoMapperTest
    {
        private BankBalanceDto Result { get; set; }

        private BankBalance TestData
        {
            get { return new BankBalance(StatementModelTestData.ChequeAccount, 44552.21M); }
        }

        [TestMethod]
        public void ShouldMapAmount()
        {
            Assert.AreEqual(44552.21M, Result.Balance);
        }

        [TestMethod]
        public void ShouldMapBankAccount()
        {
            Assert.AreEqual(StatementModelTestData.ChequeAccount.Name, Result.Account);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            Result = Mapper.Map<BankBalanceDto>(TestData);
        }
    }
}