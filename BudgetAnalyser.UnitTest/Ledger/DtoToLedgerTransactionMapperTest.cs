using System;
using AutoMapper;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Ledger
{
    [TestClass]
    public class DtoToLedgerTransactionMapperTest
    {
        private static readonly Guid TransactionId = new Guid("7F921750-4467-4EA4-81E6-3EFD466341C6");

        public DtoToLedgerTransactionMapperTest()
        {
            TestData = new LedgerTransactionDto
            {
                Id = TransactionId,
                Amount = -123.99M,
                Narrative = "Foo bar.",
                TransactionType = typeof(CreditLedgerTransaction).FullName,
            };
        }

        private LedgerTransaction Result { get; set; }

        private LedgerTransactionDto TestData { get; set; }

        [TestMethod]
        public void ShouldMapAccountTypeForBalanceAdjustmentTransaction()
        {
            TestData = new LedgerTransactionDto
                {
                    Id = TransactionId,
                    Amount = -123.99M,
                    Narrative = "Foo bar.",
                    Account = StatementModelTestData.ChequeAccount.Name,
                    TransactionType = typeof(BankBalanceAdjustmentTransaction).FullName,
                };
            Result = Mapper.Map<BankBalanceAdjustmentTransaction>(TestData);

            Assert.AreEqual(StatementModelTestData.ChequeAccount.Name, ((BankBalanceAdjustmentTransaction)Result).BankAccount.Name);
        }

        [TestMethod]
        public void ShouldMapAmount()
        {
            Assert.AreEqual(-123.99M, Result.Amount);
        }

        [TestMethod]
        public void ShouldMapId()
        {
            Assert.AreEqual(TransactionId, Result.Id);
        }

        [TestMethod]
        public void ShouldMapNarrative()
        {
            Assert.AreEqual("Foo bar.", Result.Narrative);
        }

        [TestMethod]
        public void ShouldMapTransactionType()
        {
            Assert.IsInstanceOfType(Result, typeof(CreditLedgerTransaction));
        }

        [TestInitialize]
        public void TestInitialise()
        {
            Result = Mapper.Map<LedgerTransaction>(TestData);
        }
    }
}