using System;
using AutoMapper;
using BudgetAnalyser.Engine.Account;
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

        public LedgerTransactionToDtoMapperTest()
        {
            TestData = new DebitLedgerTransaction(new Guid("7F921750-4467-4EA4-81E6-3EFD466341C6"))
            {
                Debit = 123.99M,
                Narrative = "Foo bar.",
            };
        }

        private LedgerTransactionDto Result { get; set; }

        private LedgerTransaction TestData { get; set; }

        [TestMethod]
        public void ShouldMapAmount()
        {
            Assert.AreEqual(123.99M, Result.Debit);
        }

        [TestMethod]
        public void ShouldMapBankAccount()
        {
            TestData = new BankBalanceAdjustmentTransaction(TransactionId)
            {
                BankAccount = new ChequeAccount("CHEQUE"),
                Debit = -101,
                Narrative = "TEsting 123",
            };
            TestInitialise(); // Re-initialise to use different test data.

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

        [TestMethod]
        public void ShouldNotMapBankAccountForCreditLedgerTransaction()
        {
            Assert.IsNull(Result.AccountType);
        }

        [TestMethod]
        public void ShouldNotMapBankAccountForDebitLedgerTransaction()
        {
            Assert.IsNull(Result.AccountType);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            Result = Mapper.Map<LedgerTransactionDto>(TestData);
        }
    }
}