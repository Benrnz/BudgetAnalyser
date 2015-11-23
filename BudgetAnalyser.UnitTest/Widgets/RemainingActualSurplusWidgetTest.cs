using System;
using System.Linq;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.UnitTest.Helper;
using BudgetAnalyser.UnitTest.TestData;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Widgets
{
    [TestClass]
    public class RemainingActualSurplusWidgetTest
    {
        private RemainingActualSurplusWidget subject;
        private StatementModel statementTestData;
        private GlobalFilterCriteria criteriaTestData;
        private LedgerBook ledgerBookTestData;
        private LedgerCalculation ledgerCalculation;

        [TestInitialize]
        public void TestInitialise()
        {
            this.subject = new RemainingActualSurplusWidget();
            this.criteriaTestData = new GlobalFilterCriteria
            {
                BeginDate = new DateTime(2015, 10, 20),
                EndDate = new DateTime(2015, 11, 19),
            };
            this.ledgerCalculation = new LedgerCalculation();

            StatementModelTestDataGenerated.AccountTypeRepo = new InMemoryAccountTypeRepository();
            StatementModelTestDataGenerated.BudgetBucketRepo = new BucketBucketRepoAlwaysFind();
            this.statementTestData = StatementModelTestDataGenerated.TestDataGenerated();

            this.ledgerBookTestData = new LedgerBookBuilder
            {
                StorageKey = "RemainingActualSurplusWidgetTest.xml",
                Modified = new DateTime(2015, 11, 23),
                Name = "Smith Budget 2015"
            }
                .WithLedger(StatementModelTestData.PhoneBucket, 130M)
                .WithLedger(StatementModelTestData.CarMtcBucket, 90M)
                .WithLedger(StatementModelTestData.PowerBucket)
                .WithReconciliation(
                    new DateTime(2015, 10, 20),
                    new BankBalance(LedgerBookTestData.ChequeAccount, 4502.75M))
                .WithEntries(
                    reconBuilder =>
                    {
                        reconBuilder.ForLedger(LedgerBookTestData.PhoneLedger);
                        reconBuilder.ForLedger(LedgerBookTestData.CarMtcLedger);
                        reconBuilder.ForLedger(LedgerBookTestData.PowerLedger);
                        reconBuilder.WithTransactions(
                            txnBuilder =>
                            {
                                txnBuilder.WithCredit(3000M, "Oct Savings", new DateTime(2015, 10, 20), "automatchref12");
                            });
                    })
                .Build();
        }

        [TestMethod]
        public void OutputTestData()
        {
            this.ledgerBookTestData.Output(true);

            foreach (var transaction in this.statementTestData.AllTransactions)
            {
                Console.WriteLine($"{transaction.Date:d}  {transaction.BudgetBucket.Code}  {transaction.Amount:F2}");
            }

            Console.WriteLine($"{this.statementTestData.AllTransactions.Count()} Transactions");
        }

        [TestMethod]
        public void Update_ShouldExcludeAutoMatchedTransactionsInCalculation()
        {
            this.subject.Update(this.statementTestData, this.criteriaTestData, this.ledgerBookTestData, this.ledgerCalculation);

            Assert.AreEqual(-2433.34, this.subject.Value);
        }
    }
}
