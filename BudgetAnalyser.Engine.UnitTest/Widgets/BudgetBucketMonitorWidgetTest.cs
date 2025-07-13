using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.UnitTest.Helper;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using BudgetAnalyser.Engine.Widgets;

namespace BudgetAnalyser.Engine.UnitTest.Widgets;

[TestClass]
public class BudgetBucketMonitorWidgetTest
{
    private BucketBucketRepoAlwaysFind bucketRepo;
    private IBudgetCurrencyContext budgetTestData;
    private GlobalFilterCriteria criteriaTestData;
    private LedgerBook ledgerBookTestData;
    private LedgerCalculation ledgerCalculation;
    private BudgetBucketMonitorWidget subject;
    private TransactionSetModel transactionSetTestData;

    [TestMethod]
    public void OutputTestData()
    {
        this.ledgerBookTestData.Output(true);
        this.budgetTestData.Output();
        this.transactionSetTestData.Output(DateOnly.MinValue);
    }

    [TestInitialize]
    public void TestInitialise()
    {
        this.subject = new BudgetBucketMonitorWidget { BucketCode = StatementModelTestData.PhoneBucket.Code };

        this.bucketRepo = new BucketBucketRepoAlwaysFind();
        this.criteriaTestData = new GlobalFilterCriteria { BeginDate = new DateOnly(2015, 10, 20), EndDate = new DateOnly(2015, 11, 19) };

        CreateStatementTestData();

        var budgetModel = BudgetModelTestData.CreateTestData5();
        this.budgetTestData = new BudgetCurrencyContext(new BudgetCollection(budgetModel), budgetModel);

        CreateLedgerBookTestData();

        this.ledgerCalculation = new LedgerCalculation(new FakeLogger());
    }

    [TestMethod]
    [Description("A transfer has taken place from InsHome in Savings, to Phone in Cheque for $100. This should be excluded from running balance of both buckets.")]
    public void Update_ShouldExcludeAutoMatchedTransactionsInCalculation()
    {
        this.subject.Update(this.budgetTestData, this.transactionSetTestData, this.criteriaTestData, this.bucketRepo, this.ledgerBookTestData, this.ledgerCalculation, new FakeLogger());

        // Starting Phone Balance is Budget Amount: 150.00
        // Total Phone Statement transactions are: -20.00
        // Resulting Balance = 130.00
        Assert.AreEqual(130.00, this.subject.Value);
    }

    private void CreateLedgerBookTestData()
    {
        this.ledgerBookTestData = new LedgerBookBuilder { StorageKey = "BudgetBucketMonitorWidgetTest.xml", Modified = new DateTime(2015, 11, 23), Name = "Smith Budget 2015" }
            .IncludeLedger(LedgerBookTestData.PhoneLedger, 50M)
            .IncludeLedger(LedgerBookTestData.HouseInsLedgerSavingsAccount, 100M)
            .AppendReconciliation(
                new DateOnly(2015, 10, 20),
                new BankBalance(LedgerBookTestData.ChequeAccount, 2000M),
                new BankBalance(LedgerBookTestData.SavingsAccount, 1000M))
            .WithReconciliationEntries(
                entryBuilder =>
                {
                    entryBuilder.WithLedger(LedgerBookTestData.PhoneLedger)
                        .AppendTransactions(txnBuilder => { txnBuilder.WithCredit(100, "Foo", new DateOnly(2015, 10, 20), "automatchref12"); });
                    entryBuilder.WithLedger(LedgerBookTestData.HouseInsLedgerSavingsAccount)
                        .AppendTransactions(txnBuilder => { txnBuilder.WithCredit(-100, "Foo", new DateOnly(2015, 10, 20), "automatchref12"); });
                })
            .Build();
    }

    private void CreateStatementTestData()
    {
        this.transactionSetTestData = new StatementModelBuilder()
            .AppendTransaction(
                new Transaction
                {
                    Account = StatementModelTestData.SavingsAccount,
                    Amount = -100M,
                    BudgetBucket = StatementModelTestData.InsHomeBucket,
                    Date = new DateOnly(2015, 11, 19),
                    Reference1 = "automatchref12"
                })
            .AppendTransaction(
                new Transaction
                {
                    Account = StatementModelTestData.ChequeAccount,
                    Amount = 100M,
                    BudgetBucket = StatementModelTestData.PhoneBucket,
                    Date = new DateOnly(2015, 11, 19),
                    Reference1 = "automatchref12"
                })
            .AppendTransaction(
                new Transaction
                {
                    Account = StatementModelTestData.SavingsAccount,
                    Amount = -10M,
                    BudgetBucket = StatementModelTestData.InsHomeBucket,
                    Date = new DateOnly(2015, 11, 1),
                    Reference1 = "Foo"
                })
            .AppendTransaction(
                new Transaction
                {
                    Account = StatementModelTestData.ChequeAccount,
                    Amount = -20M,
                    BudgetBucket = StatementModelTestData.PhoneBucket,
                    Date = new DateOnly(2015, 11, 1),
                    Reference1 = "Foo"
                })
            .Build();
    }
}
