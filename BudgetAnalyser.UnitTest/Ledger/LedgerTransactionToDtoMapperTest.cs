using System;
using AutoMapper;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Ledger
{
    [TestClass]
    public class LedgerTransactionToDtoMapperTest
    {
        private static readonly Guid TransactionId = new Guid("7F921750-4467-4EA4-81E6-3EFD466341C6");

        private LedgerTransactionDto Result { get; set; }

        private LedgerTransaction TestData
        {
            get
            {
                return new DebitLedgerTransaction(new Guid("7F921750-4467-4EA4-81E6-3EFD466341C6"))
                {
                    BankAccount = StatementModelTestData.ChequeAccount,
                    Debit = 123.99M,
                    Narrative = "Foo bar.",
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
            Assert.AreEqual(StatementModelTestData.ChequeAccount.Name, Result.AccountType);
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
            Assert.AreEqual(typeof(DebitLedgerTransaction).FullName, Result.TransactionType);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            AutoMapperConfigurationTest.AutoMapperConfiguration();

            Result = Mapper.Map<LedgerTransactionDto>(TestData);
        }
    }
}