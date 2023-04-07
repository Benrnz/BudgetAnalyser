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
                            .AppendTransactions(txnBuilder => { txnBuilder.WithCredit(100, "Foo", new DateTime(2015, 10, 20), "automatchref12"); });
                        entryBuilder.WithLedger(LedgerBookTestData.HouseInsLedgerSavingsAccount)
                            .AppendTransactions(txnBuilder => { txnBuilder.WithCredit(-100, "Foo", new DateTime(2015, 10, 20), "automatchref12"); });
                    })
                .Build();
        }

        private static StatementModel CreateStatementTestData()
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
                    })
                .Build();
        }

        [TestMethod]
        public void CalculateCurrentMonthLedgerBalances_ShouldNotCountAutomatchTransactions_GivenAutoMatchTransactions()
        {
            var result = Subject.CalculateCurrentPeriodLedgerBalances(
                CreateLedgerBookTestData(), 
                new GlobalFilterCriteria
                {
                    BeginDate = new DateTime(2015, 10, 20),
                    EndDate = new DateTime(2015, 11, 19)
                }, 
                CreateStatementTestData());

            Assert.AreEqual(90M, result[LedgerBookTestData.HouseInsLedgerSavingsAccount.BudgetBucket]);
        }

        [TestMethod]
        public void CalculateOverSpentLedgersShouldReturnEmptyGivenNoLedgerLineForGivenDate()
        {
            IEnumerable<ReportTransaction> result = Subject.CalculateOverSpentLedgers(StatementModelTestData.TestData1(), TestData, new DateTime(2014, 07, 01));
            Assert.IsTrue(result.Count() == 0);
        }

        [TestMethod]
        public void CalculateOverSpentLedgersShouldReturnEmptyGivenNoLedgersWereOverdrawn()
        {
            TestData.Output(true);
            IEnumerable<ReportTransaction> result = Subject.CalculateOverSpentLedgers(StatementModelTestData.TestData3(), TestData, new DateTime(2013, 08, 15));
            foreach (ReportTransaction txn in result)
            {
                Console.WriteLine("{0} {1} {2}", txn.Date, txn.Narrative, txn.Amount);
            }

            Assert.IsTrue(result.Count() == 0);
        }

        [TestMethod]
        public void CalculateOverSpentLedgersShouldReturnOverdrawnTransactionsGivenStatementTransactionsSpendMoreThanLedgerBalance()
        {
            TestData.Output(true);
            IEnumerable<ReportTransaction> result = Subject.CalculateOverSpentLedgers(StatementModelTestData.TestData2(), TestData, new DateTime(2013, 08, 15));
            foreach (ReportTransaction txn in result)
            {
                Console.WriteLine("{0} {1} {2}", txn.Date, txn.Narrative, txn.Amount);
            }

            Assert.AreEqual(-40.41M, result.Sum(t => t.Amount));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CalculateOverSpentLedgersShouldThrowGivenNullLedger()
        {
            Subject.CalculateOverSpentLedgers(StatementModelTestData.TestData1(), null, new DateTime(2014, 07, 01));
            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CalculateOverSpentLedgersShouldThrowGivenNullStatement()
        {
            Subject.CalculateOverSpentLedgers(null, TestData, new DateTime(2014, 07, 01));
            Assert.Fail();
        }

        [TestInitialize]
        public void TestInitialise()
        {
            Subject = new LedgerCalculation();
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
    }
}