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

        private LedgerTransaction Result { get; set; }

        private LedgerTransactionDto TestData
        {
            get
            {
                return new LedgerTransactionDto
                {
                    Id = TransactionId,
                    Debit = 123.99M,
                    Narrative = "Foo bar.",
                    AccountType = StatementModelTestData.ChequeAccount.Name,
                    TransactionType = typeof(DebitLedgerTransaction).FullName,
                };
            }
        }

        [TestMethod]
        public void ShouldMapAmount()
        {
            Assert.AreEqual(123.99M, Result.Debit);
        }

        [TestMethod]
        public void ShouldMapBankAccount()
        {
            Assert.AreEqual(StatementModelTestData.ChequeAccount.Name, Result.BankAccount.Name);
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
            Assert.IsInstanceOfType(Result, typeof(DebitLedgerTransaction));
        }

        [TestInitialize]
        public void TestInitialise()
        {
            

            Result = Mapper.Map<LedgerTransaction>(TestData);
        }
    }
}