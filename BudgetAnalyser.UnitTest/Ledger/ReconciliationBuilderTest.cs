using System;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.UnitTest.Helper;
using BudgetAnalyser.UnitTest.TestData;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Ledger
{
    [TestClass]
    public class ReconciliationBuilderTest
    {
        private BudgetModel budgetTestData;
        private DateTime reconciliationDate;
        private StatementModel statementTestData;
        private ReconciliationBuilder subject;
        private LedgerBook ledgerBookTestData;

        [TestMethod]
        public void SpentMonthlyLedgers_ShouldOnlyRemoveExcessUpToBudgetAmount_GivenBudgetAmountIsGreaterThanPreviousBalance()
        {
            // Issue 83
            ReconciliationResult result = this.subject.CreateNewMonthlyReconciliation(
                this.reconciliationDate,
                this.budgetTestData,
                this.statementTestData,
                new BankBalance(LedgerBookTestData.ChequeAccount, 5000M));

            result.Reconciliation.Output(LedgerBookHelper.LedgerOrder(this.ledgerBookTestData), true, true);

            var resultingPowerBalance = result.Reconciliation.Entries.First(e => e.LedgerBucket == LedgerBookTestData.PowerLedger).Balance;
            Assert.AreEqual(175M, resultingPowerBalance);
        }

        [TestMethod]
        public void TestDataOutput()
        {
            Console.WriteLine("Reconciliation Date will be: " + this.reconciliationDate);
            Console.WriteLine();
            this.ledgerBookTestData.Output();
            this.budgetTestData.Output();
            this.statementTestData.Output(this.reconciliationDate.AddMonths(-1));
        }


        [TestInitialize]
        public void TestInitialise()
        {
            this.reconciliationDate = new DateTime(2013, 9, 20);

            this.budgetTestData = BudgetModelTestData.CreateTestData1();

            this.statementTestData = new StatementModelBuilder()
                .TestData1()
                .AppendTransaction(
                    new Transaction
                    {
                        Account = StatementModelTestData.ChequeAccount,
                        BudgetBucket = StatementModelTestData.PowerBucket,
                        Amount = -75M,
                        Date = new DateTime(2013, 9, 17),
                        Description = "Power Bill",
                        Id = Guid.NewGuid(),
                        TransactionType = new NamedTransaction("Foo"),
                    })
                .Build();

            this.ledgerBookTestData = new LedgerBookBuilder()
                .IncludeLedger(LedgerBookTestData.PowerLedger, 125M)
                .AppendReconciliation(this.reconciliationDate.AddMonths(-1), new BankBalance(LedgerBookTestData.ChequeAccount, 1000M))
                .WithReconciliationEntries(
                    entryBuilder =>
                    {
                        entryBuilder.WithLedger(LedgerBookTestData.PowerLedger)
                            .HasNoTransactions();
                    })
                .Build();

            this.subject = new ReconciliationBuilder(new FakeLogger());
            this.subject.LedgerBook = this.ledgerBookTestData;
        }
    }
}