using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rees.TestUtilities;

namespace BudgetAnalyser.UnitTest.Ledger
{
    [TestClass]
    public class DtoToLedgerEntryMapperTest
    {
        private LedgerEntry Control { get; set; }
        private LedgerEntry Result { get; set; }

        private LedgerEntryDto TestData
        {
            get
            {
                /*
            <LedgerEntryDto Balance="52.32" BucketCode="POWER">
              <LedgerEntryDto.Transactions>
                <scg:List x:TypeArguments="LedgerTransactionDto" Capacity="4">
                  <LedgerTransactionDto Account="{x:Null}" Credit="140" Debit="0" Id="601d77e5-63d5-479c-a0e5-d56a18c975f1" Narrative="Budgeted amount" TransactionType="BudgetAnalyser.Engine.Ledger.BudgetCreditLedgerTransaction" />
                  <LedgerTransactionDto Account="{x:Null}" Credit="0" Debit="98.56" Id="450f9b46-010a-4508-afc5-d46042c80d02" Narrative="Power bill" TransactionType="BudgetAnalyser.Engine.Ledger.CreditLedgerTransaction" />
                </scg:List>
              </LedgerEntryDto.Transactions>
            </LedgerEntryDto>                 */
                return new LedgerEntryDto
                {
                    Balance = 52.32M,
                    BucketCode = TestDataConstants.PowerBucketCode,
                    Transactions = new List<LedgerTransactionDto>
                    {
                        new LedgerTransactionDto
                        {
                            Account = StatementModelTestData.ChequeAccount.Name,
                            Amount = 140M,
                            Id = Guid.NewGuid(),
                            Narrative = "Foo...",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName
                        },
                        new LedgerTransactionDto
                        {
                            Account = StatementModelTestData.ChequeAccount.Name,
                            Amount = -98.56M,
                            Id = Guid.NewGuid(),
                            Narrative = "Bar...",
                            TransactionType = typeof(CreditLedgerTransaction).FullName
                        }
                    }
                };
            }
        }

        [TestMethod]
        public void ShouldMapBalance()
        {
            Assert.AreEqual(Control.Balance, Result.Balance);
        }

        [TestMethod]
        public void ShouldMapBucketCode()
        {
            Assert.AreEqual(TestDataConstants.PowerBucketCode, Result.LedgerBucket.BudgetBucket.Code);
        }

        [TestMethod]
        public void ShouldMapCorrectNumberOfTransactions()
        {
            Assert.AreEqual(2, Result.Transactions.Count());
        }

        [TestMethod]
        public void ShouldMapNetAmount()
        {
            Assert.AreEqual(Control.NetAmount, Result.NetAmount);
        }

        [TestMethod]
        public void ShouldSetIsNewToFalse()
        {
            Assert.IsFalse((bool)PrivateAccessor.GetField(Result, "isNew"));
        }

        [TestInitialize]
        public void TestInitialise()
        {
            Result = Mapper.Map<LedgerEntry>(TestData);

            LedgerBook book = LedgerBookTestData.TestData2();
            Control = book.Reconciliations.First(l => l.Date == new DateTime(2013, 08, 15)).Entries.First(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
        }
    }
}