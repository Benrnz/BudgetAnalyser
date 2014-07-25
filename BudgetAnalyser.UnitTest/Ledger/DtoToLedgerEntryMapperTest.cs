using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Ledger
{
    [TestClass]
    public class DtoToLedgerEntryMapperTest
    {
        private LedgerEntry Result { get; set; }

        private LedgerEntryDto TestData
        {
            get
            {
                return new LedgerEntryDto
                {
                    Balance = 2398.39M,
                    BucketCode = TestDataConstants.InsuranceHomeBucketCode,
                    Transactions = new List<LedgerTransactionDto>
                    {
                        new LedgerTransactionDto
                        {
                            AccountType = StatementModelTestData.ChequeAccount.Name,
                            Credit = 222.33M,
                            Id = Guid.NewGuid(),
                            Narrative = "Foo...",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        },
                    },
                };
            }
        }

        [TestMethod]
        public void ShouldMapBalance()
        {
            Assert.AreEqual(2620.72M, Result.Balance);
        }

        [TestMethod]
        public void ShouldMapNetAmount()
        {
            Assert.AreEqual(222.33M, Result.NetAmount);
        }

        [TestMethod]
        public void ShouldMapBucketCode()
        {
            Assert.AreEqual(TestDataConstants.InsuranceHomeBucketCode, Result.LedgerColumn.BudgetBucket.Code);
        }

        [TestMethod]
        public void ShouldMapCorrectNumberOfTransactions()
        {
            Assert.AreEqual(1, Result.Transactions.Count());
        }

        [TestInitialize]
        public void TestInitialise()
        {
            AutoMapperConfigurationTest.AutoMapperConfiguration();

            Result = Mapper.Map<LedgerEntry>(TestData);
        }
    }
}