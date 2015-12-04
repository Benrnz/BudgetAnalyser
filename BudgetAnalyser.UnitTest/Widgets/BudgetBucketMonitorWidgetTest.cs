using System;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
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
    public class BudgetBucketMonitorWidgetTest
    {
        private IBudgetCurrencyContext budgetTestData;
        private GlobalFilterCriteria criteriaTestData;
        private LedgerBook ledgerBookTestData;
        private StatementModel statementTestData;
        private BudgetBucketMonitorWidget subject;
        private LedgerCalculation ledgerCalculation;
        private BucketBucketRepoAlwaysFind bucketRepo;

        [TestMethod]
        public void OutputTestData()
        {
            this.ledgerBookTestData.Output(true);
            this.budgetTestData.Output();
            this.statementTestData.Output(DateTime.MinValue);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            this.subject = new BudgetBucketMonitorWidget();
            this.subject.BucketCode = StatementModelTestData.PhoneBucket.Code;

            this.bucketRepo = new BucketBucketRepoAlwaysFind();
            this.criteriaTestData = new GlobalFilterCriteria
            {
                BeginDate = new DateTime(2015, 10, 20),
                EndDate = new DateTime(2015, 11, 19)
            };

            CreateStatementTestData();

            BudgetModel budgetModel = BudgetModelTestData.CreateTestData5();
            this.budgetTestData = new BudgetCurrencyContext(new BudgetCollection(budgetModel), budgetModel);

            CreateLedgerBookTestData();

            this.ledgerCalculation = new LedgerCalculation(new FakeLogger());
        }

        private void CreateLedgerBookTestData()
        {
            this.ledgerBookTestData = new LedgerBookBuilder
            {
                StorageKey = "BudgetBucketMonitorWidgetTest.xml",
                Modified = new DateTime(2015, 11, 23),
                Name = "Smith Budget 2015"
            }
                .IncludeLedger(LedgerBookTestData.PhoneLedger, 50M)
                .IncludeLedger(LedgerBookTestData.HouseInsLedgerSavingsAccount, 100M)
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

        private void CreateStatementTestData()
        {
            this.statementTestData = new StatementModelBuilder()
                .AppendTransaction(
                    new Transaction
                    {
                        Account = StatementModelTestData.SavingsAccount,
                        Amount = -100M,
                        BudgetBucket = StatementModelTestData.InsHomeBucket,
                        Date = new DateTime(2015, 10, 19),
                        Reference1 = "automatchref12",
                    })
                .AppendTransaction(
                    new Transaction
                    {
                        Account = StatementModelTestData.ChequeAccount,
                        Amount = 100M,
                        BudgetBucket = StatementModelTestData.PhoneBucket,
                        Date = new DateTime(2015, 10, 19),
                        Reference1 = "automatchref12",
                    })
                .AppendTransaction(
                    new Transaction
                    {
                        Account = StatementModelTestData.SavingsAccount,
                        Amount = -10M,
                        BudgetBucket = StatementModelTestData.InsHomeBucket,
                        Date = new DateTime(2015, 10, 1),
                        Reference1 = "Foo"
                    })
                .AppendTransaction(
                    new Transaction
                    {
                        Account = StatementModelTestData.ChequeAccount,
                        Amount = -20M,
                        BudgetBucket = StatementModelTestData.PhoneBucket,
                        Date = new DateTime(2015, 10, 1),
                        Reference1 = "Foo"
                    })
                .Build();
        }

        [TestMethod]
        [Description("A transfer has taken place from InsHome in Savings, to Phone in Cheque for $100. This should be excluded from running balance of both buckets.")]
        public void Update_ShouldExcludeAutoMatchedTransactionsInCalculation()
        {
            this.subject.Update(this.budgetTestData, this.statementTestData, this.criteriaTestData, this.bucketRepo, this.ledgerBookTestData, this.ledgerCalculation);

            // Starting Phone Balance is Budget Amount: 150.00
            // Total Phone Statement transactions are: -20.00
            // Resulting Balance = 130.00
            Assert.AreEqual(130.00, this.subject.Value);
        }
    }
}