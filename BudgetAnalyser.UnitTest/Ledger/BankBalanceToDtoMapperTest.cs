using AutoMapper;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Ledger
{
    [TestClass]
    public class DtoToBankBalanceMapperTest
    {
        private BankBalance Result { get; set; }

        private BankBalanceDto TestData
        {
            get
            {
                return new BankBalanceDto
                {
                    Account = StatementModelTestData.ChequeAccount.Name,
                    Balance = 44552.44M,
                };
            }
        }

        [TestMethod]
        public void ShouldMapAmount()
        {
            Assert.AreEqual(44552.44M, Result.Balance);
        }

        [TestMethod]
        public void ShouldMapBankAccount()
        {
            Assert.AreEqual(StatementModelTestData.ChequeAccount.Name, Result.Account.Name);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            

            Result = Mapper.Map<BankBalance>(TestData);
        }
    }
}