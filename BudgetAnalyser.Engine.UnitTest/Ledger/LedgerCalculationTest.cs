using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Reports;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.UnitTest.Helper;
using BudgetAnalyser.Engine.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Ledger
{
    [TestClass]
    public class LedgerCalculationTest
    {
        private LedgerCalculation Subject { get; set; }
        private LedgerBook TestData { get; set; }

        private static LedgerBook CreateLedgerBookTestData()
        {
            return new LedgerBookBuilder
                {
                    StorageKey = "BudgetBucketMonitorWidgetTest.xml",
                    Modified = new DateTime(2015, 11, 23),
                    Name = "Smith Budget 2015"
                }
                .IncludeLedger(LedgerBookTestData.PhoneLedger, 50M)
                .IncludeLedger(LedgerBookTestData.HouseInsLedgerSavingsAccount, 200M)
                .AppendReconciliation(
                                      new DateTime(2015, 10, 20),
                                      new BankBalance(LedgerBookTestData.ChequeAccount, 2000M),
                                      new BankBalance(LedgerBookTestData.SavingsAccount, 1000M))
                .WithReconciliationEntries(
                                           entryBuilder =>
                                           {
                                               entryBuilder.WithLedger(LedgerBookTestData.PhoneLedger)
                                                   .AppendTransactions(txnBuilder => { txnBuilder.WithCredit(100, "Soo", new DateTime(2015, 10, 20), "automatchref12"); })
                                                   .AppendTransactions(txnBuilder => { txnBuilder.WithCredit(-60, "Soo1", new DateTime(2015, 10, 20)); });
                                               entryBuilder.WithLedger(LedgerBookTestData.HouseInsLedgerSavingsAccount)
                                                   .AppendTransactions(txnBuilder => { txnBuilder.WithCredit(-100, "Foo", new DateTime(2015, 10, 20), "automatchref12"); })
                                                   .AppendTransactions(txnBuilder => { txnBuilder.WithCredit(-70, "Foo1", new DateTime(2015, 10, 20)); });
                                           })
                .Build();
        }

        private static StatementModel CreateStatementTestData()
        {
            return CreateStatementBuilder().Build();
        }

        private static StatementModelBuilder CreateStatementBuilder()
        {
            return new StatementModelBuilder()
                .AppendTransaction(
                                   new Transaction
                                   {
                                       Account = StatementModelTestData.SavingsAccount,
                                       Amount = -100M,
                                       BudgetBucket = StatementModelTestData.InsHomeBucket,
                                       Date = new DateTime(2015, 11, 19),
                                       Reference1 = "automatchref12",
                                   })
                .AppendTransaction(
                                   new Transaction
                                   {
                                       Account = StatementModelTestData.ChequeAccount,
                                       Amount = 100M,
                                       BudgetBucket = StatementModelTestData.PhoneBucket,
                                       Date = new DateTime(2015, 11, 19),
                                       Reference1 = "automatchref12",
                                   })
                .AppendTransaction(
                                   new Transaction
                                   {
                                       Account = StatementModelTestData.SavingsAccount,
                                       Amount = -10M,
                                       BudgetBucket = StatementModelTestData.InsHomeBucket,
                                       Date = new DateTime(2015, 11, 1),
                                       Reference1 = "Foo"
                                   })
                .AppendTransaction(
                                   new Transaction
                                   {
                                       Account = StatementModelTestData.ChequeAccount,
                                       Amount = -20M,
                                       BudgetBucket = StatementModelTestData.PhoneBucket,
                                       Date = new DateTime(2015, 11, 1),
                                       Reference1 = "Foo"
                                   });
        }

        [TestMethod]
        public void CalculateCurrentPeriodSurplusBalance_ShouldCountSavingsTransactionsAsSurplusTransactions()
        {
            var ledgerLine = CreateLedgerBookTestData().Reconciliations.First();
            var statement = CreateStatementBuilder().AppendTransaction(
                                                                       new Transaction
                                                                       {
                                                                           Account = StatementModelTestData.ChequeAccount,
                                                                           Amount = -100M,
                                                                           BudgetBucket = StatementModelTestData.SavingsBucket,
                                                                           Date = new DateTime(2015, 11, 2),
                                                                           Reference1 = "Yee har"
                                                                       })
                .Build();
            var result = Subject.CalculateCurrentPeriodSurplusBalance(
                                                                      ledgerLine,
                                                                      new GlobalFilterCriteria
                                                                      {
                                                                          BeginDate = new DateTime(2015, 10, 20),
                                                                          EndDate = new DateTime(2015, 11, 19)
                                                                      },
                                                                      statement);

            Assert.AreEqual(2770M, result);
        }

        [TestMethod]
        public void CalculateCurrentPeriodSurplusBalance_ShouldCountPayCCTransactionsAsSurplusTransactions()
        {
            var ledgerLine = CreateLedgerBookTestData().Reconciliations.First();
            var statement = CreateStatementBuilder().AppendTransaction(
                                                                       new Transaction
                                                                       {
                                                                           Account = StatementModelTestData.ChequeAccount,
                                                                           Amount = -100M,
                                                                           BudgetBucket = StatementModelTestData.PayCreditCard,
                                                                           Date = new DateTime(2015, 11, 2),
                                                                           Reference1 = "Pay credit card debit from surplus"
                                                                       })
                .Build();
            var result = Subject.CalculateCurrentPeriodSurplusBalance(
                                                                      ledgerLine,
                                                                      new GlobalFilterCriteria
                                                                      {
                                                                          BeginDate = new DateTime(2015, 10, 20),
                                                                          EndDate = new DateTime(2015, 11, 19)
                                                                      },
                                                                      statement);

            Assert.AreEqual(2770M, result);
        }

        [TestMethod]
        public void CalculateCurrentPeriodSurplusBalance_ShouldNotCountPositivePayCCOrSavingsTransactions()
        {
            var ledgerLine = CreateLedgerBookTestData().Reconciliations.First();
            var statement = CreateStatementBuilder().AppendTransaction(new Transaction
                {
                    Account = StatementModelTestData.ChequeAccount,
                    Amount = -100M,
                    BudgetBucket = StatementModelTestData.PayCreditCard,
                    Date = new DateTime(2015, 11, 2),
                    Reference1 = "Pay credit card debit from surplus"
                })
                .AppendTransaction(new Transaction
                {
                    Account = StatementModelTestData.VisaAccount,
                    Amount = 100M,
                    BudgetBucket = StatementModelTestData.PayCreditCard,
                    Date = new DateTime(2015, 11, 2),
                    Reference1 = "Credit Visa account with payment"
                })
                .AppendTransaction(new Transaction
                {
                    Account = StatementModelTestData.ChequeAccount,
                    Amount = -50M,
                    BudgetBucket = StatementModelTestData.SurplusBucket,
                    Date = new DateTime(2015, 11, 3),
                    Reference1 = "Buy something cool with spare funds"
                })
                .AppendTransaction(new Transaction
                {
                    Account = StatementModelTestData.ChequeAccount,
                    Amount = 30M,
                    BudgetBucket = StatementModelTestData.SurplusBucket,
                    Date = new DateTime(2015, 11, 3),
                    Reference1 = "Refund"
                })
                .AppendTransaction(new Transaction
                {
                    Account = StatementModelTestData.ChequeAccount,
                    Amount = -20M,
                    BudgetBucket = StatementModelTestData.SavingsBucket,
                    Date = new DateTime(2015, 11, 4),
                    Reference1 = "Save for a rainy day"
                })
                .AppendTransaction(new Transaction
                {
                    Account = StatementModelTestData.ChequeAccount,
                    Amount = 20M,
                    BudgetBucket = StatementModelTestData.SavingsBucket,
                    Date = new DateTime(2015, 11, 4),
                    Reference1 = "Credits to savings shouldn't affect surplus balance"
                })
                .Build();
            var result = Subject.CalculateCurrentPeriodSurplusBalance(
                  ledgerLine, 
                  new GlobalFilterCriteria
                  {
                      BeginDate = new DateTime(2015, 10, 20),
                      EndDate = new DateTime(2015, 11, 19)
                  },
                  statement);

            Assert.AreEqual(2730M, result);
        }

        [TestMethod]
        public void CalculateCurrentMonthLedgerBalances_ShouldNotCountAutomatchTransactions_GivenAutoMatchTransactions()
        {
            var ledgerLine = CreateLedgerBookTestData().Reconciliations.First();
            var result = Subject.CalculateCurrentPeriodLedgerBalances(
                                                                      ledgerLine,
                                                                      new GlobalFilterCriteria
                                                                      {
                                                                          BeginDate = new DateTime(2015, 10, 20),
                                                                          EndDate = new DateTime(2015, 11, 19)
                                                                      },
                                                                      CreateStatementTestData());

            Assert.AreEqual(20M, result[LedgerBookTestData.HouseInsLedgerSavingsAccount.BudgetBucket]);
        }

        [TestMethod]
        public void CalculateCurrentFortnightLedgerBalances_ShouldNotCountAutomatchTransactions_GivenAutoMatchTransactions()
        {
            var ledgerLine = CreateLedgerBookTestData().Reconciliations.First();
            var result = Subject.CalculateCurrentPeriodLedgerBalances(
                                                                      ledgerLine,
                                                                      new GlobalFilterCriteria
                                                                      {
                                                                          BeginDate = new DateTime(2015, 11, 01),
                                                                          EndDate = new DateTime(2015, 11, 15)
                                                                      },
                                                                      CreateStatementTestData());

            Assert.AreEqual(120M, result[LedgerBookTestData.HouseInsLedgerSavingsAccount.BudgetBucket]);
        }

        [TestMethod]
        public void CalculateOverSpentLedgersShouldReturnEmptyGivenNoLedgerLineForGivenDate()
        {
            var beginDate = new DateTime(2014, 07, 01);
            var endDate = beginDate.AddMonths(1).AddDays(-1);
            var ledgerLine = TestData.Reconciliations.First();
            IEnumerable<ReportTransaction> result = Subject.CalculateOverSpentLedgers(StatementModelTestData.TestData1(), ledgerLine, beginDate, endDate);
            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public void CalculateOverSpentLedgersShouldReturnEmptyGivenNoLedgersWereOverdrawn()
        {
            TestData.Output(true);
            var beginDate = new DateTime(2013, 08, 15);
            var endDate = beginDate.AddMonths(1).AddDays(-1);
            var ledgerLine = TestData.Reconciliations.First();
            IEnumerable<ReportTransaction> result = Subject.CalculateOverSpentLedgers(StatementModelTestData.TestData3(), ledgerLine, beginDate, endDate);
            foreach (var txn in result)
            {
                Console.WriteLine("{0} {1} {2}", txn.Date, txn.Narrative, txn.Amount);
            }

            Assert.IsTrue(result.Count() == 0);
        }

        [TestMethod]
        public void CalculateOverSpentLedgersShouldReturnOverdrawnTransactionsGivenStatementTransactionsSpendMoreThanLedgerBalance()
        {
            TestData.Output(true);
            var beginDate = new DateTime(2013, 08, 15);
            var endDate = beginDate.AddMonths(1).AddDays(-1);
            var ledgerLine = TestData.Reconciliations.First();
            IEnumerable<ReportTransaction> result = Subject.CalculateOverSpentLedgers(StatementModelTestData.TestData2(), ledgerLine, beginDate, endDate);
            foreach (var txn in result)
            {
                Console.WriteLine("{0} {1} {2}", txn.Date, txn.Narrative, txn.Amount);
            }

            Assert.AreEqual(-40.41M, result.Sum(t => t.Amount));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CalculateOverSpentLedgersShouldThrowGivenNullLedger()
        {
            var beginDate = new DateTime(2014, 07, 01);
            var endDate = beginDate.AddMonths(1).AddDays(-1);
            Subject.CalculateOverSpentLedgers(StatementModelTestData.TestData1(), null, beginDate, endDate);
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CalculateOverSpentLedgersShouldThrowGivenNullStatement()
        {
            var beginDate = new DateTime(2014, 07, 01);
            var endDate = beginDate.AddMonths(1).AddDays(-1);
            var ledgerLine = TestData.Reconciliations.First();

            Subject.CalculateOverSpentLedgers(null, ledgerLine, beginDate, endDate);
            Assert.Fail();
        }

        [TestInitialize]
        public void TestInitialise()
        {
            Subject = new LedgerCalculation(new DebugLogger());
            TestData = LedgerBookTestData.TestData1();
        }

        [TestMethod]
        public void UsingTestData1_LocateApplicableLedgerBalance_ShouldReturn64()
        {
            var filter = new GlobalFilterCriteria { BeginDate = new DateTime(2013, 04, 15), EndDate = new DateTime(2013, 05, 15) };
            decimal result = Subject.LocateApplicableLedgerBalance(TestData, filter, StatementModelTestData.PhoneBucket.Code);
            Assert.AreEqual(0M, result);
        }

        [TestMethod]
        public void UsingTestData1WithAugust15_LocateApplicableLedgerBalance_ShouldReturn64()
        {
            var filter = new GlobalFilterCriteria { BeginDate = new DateTime(2013, 07, 15), EndDate = new DateTime(2013, 08, 15) };

            decimal result = Subject.LocateApplicableLedgerBalance(TestData, filter, StatementModelTestData.PhoneBucket.Code);
            TestData.Output();
            Assert.AreEqual(64.71M, result);
        }

        [TestMethod]
        public void CalculateCurrentPeriodSurplusBalance_UsingFortnightlyData_ShouldReturn3777()
        {
            var statementModel = StatementModelTestData.TestData1();
            var filter = new GlobalFilterCriteria()
            {
                BeginDate = new DateTime(2013, 8, 15),
                EndDate = new DateTime(2013, 8, 29)
            };
            var ledgerLine = LedgerBookTestData.TestData6().Reconciliations.OrderByDescending(l => l.Date).First();

            var result = Subject.CalculateCurrentPeriodSurplusBalance(ledgerLine, filter, statementModel);

            Assert.AreEqual(3777.56M, result);
        }
    }
}