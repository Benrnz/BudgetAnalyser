using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.Engine.XUnit.Helpers;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Widgets;

public class BudgetBucketMonitorWidgetTest
{
    private readonly BudgetBucketRepoAlwaysFind bucketRepo;
    private readonly IBudgetCurrencyContext budgetTestData;
    private readonly GlobalFilterCriteria criteriaTestData;
    private readonly LedgerBook ledgerBookTestData;
    private readonly LedgerCalculation ledgerCalculation;
    private readonly BudgetBucketMonitorWidget subject;
    private readonly TransactionsListModel transactionsTestData;

    public BudgetBucketMonitorWidgetTest()
    {
        this.subject = new BudgetBucketMonitorWidget { BucketCode = TransactionsListModelTestData.PhoneBucket.Code };
        this.bucketRepo = new BudgetBucketRepoAlwaysFind();
        this.criteriaTestData = new GlobalFilterCriteria { BeginDate = new DateOnly(2015, 10, 20), EndDate = new DateOnly(2015, 11, 19) };
        this.transactionsTestData = CreateTransactionsTestData();

        var budgetModel = BudgetModelTestData.CreateTestData5();
        this.budgetTestData = new BudgetCurrencyContext(new BudgetCollection(budgetModel), budgetModel);
        this.ledgerBookTestData = CreateLedgerBookTestData();
        this.ledgerCalculation = new LedgerCalculation(new FakeLogger());
    }

    [Fact]
    public void OutputTestData()
    {
        this.ledgerBookTestData.Output(true);
        this.budgetTestData.Output();
        this.transactionsTestData.Output(DateOnly.MinValue);
    }

    [Fact]
    public void Update_ShouldExcludeAutoMatchedTransactionsInCalculation()
    {
        this.subject.Update(this.budgetTestData, this.transactionsTestData, this.criteriaTestData, this.bucketRepo, this.ledgerBookTestData, this.ledgerCalculation, new FakeLogger());
        this.subject.Value.ShouldBe(130.00);
    }

    private static LedgerBook CreateLedgerBookTestData()
    {
        return new LedgerBookBuilder { StorageKey = "BudgetBucketMonitorWidgetTest.xml", Modified = new DateTime(2015, 11, 23), Name = "Smith Budget 2015" }
            .IncludeLedger(LedgerBookTestData.PhoneLedger, 50M)
            .IncludeLedger(LedgerBookTestData.HouseInsLedgerSavingsAccount, 100M)
            .AppendReconciliation(
                new DateOnly(2015, 10, 20),
                new BankBalance(LedgerBookTestData.ChequeAccount, 2000M),
                new BankBalance(LedgerBookTestData.SavingsAccount, 1000M))
            .WithReconciliationEntries(entryBuilder =>
            {
                entryBuilder.WithLedger(LedgerBookTestData.PhoneLedger)
                    .AppendTransactions(txnBuilder => { txnBuilder.WithCredit(100, "Foo", new DateOnly(2015, 10, 20), "automatchref12"); });
                entryBuilder.WithLedger(LedgerBookTestData.HouseInsLedgerSavingsAccount)
                    .AppendTransactions(txnBuilder => { txnBuilder.WithCredit(-100, "Foo", new DateOnly(2015, 10, 20), "automatchref12"); });
            })
            .Build();
    }

    private static TransactionsListModel CreateTransactionsTestData()
    {
        return new TransactionsListModelBuilder()
            .AppendTransaction(
                new Transaction
                {
                    Account = TransactionsListModelTestData.SavingsAccount,
                    Amount = -100M,
                    BudgetBucket = TransactionsListModelTestData.InsHomeBucket,
                    Date = new DateOnly(2015, 11, 19),
                    Reference1 = "automatchref12"
                })
            .AppendTransaction(
                new Transaction
                {
                    Account = TransactionsListModelTestData.ChequeAccount,
                    Amount = 100M,
                    BudgetBucket = TransactionsListModelTestData.PhoneBucket,
                    Date = new DateOnly(2015, 11, 19),
                    Reference1 = "automatchref12"
                })
            .AppendTransaction(
                new Transaction
                {
                    Account = TransactionsListModelTestData.SavingsAccount,
                    Amount = -10M,
                    BudgetBucket = TransactionsListModelTestData.InsHomeBucket,
                    Date = new DateOnly(2015, 11, 1),
                    Reference1 = "Foo"
                })
            .AppendTransaction(
                new Transaction
                {
                    Account = TransactionsListModelTestData.ChequeAccount,
                    Amount = -20M,
                    BudgetBucket = TransactionsListModelTestData.PhoneBucket,
                    Date = new DateOnly(2015, 11, 1),
                    Reference1 = "Foo"
                })
            .Build();
    }
}
