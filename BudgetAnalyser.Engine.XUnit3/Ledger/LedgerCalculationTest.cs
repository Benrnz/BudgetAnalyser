#nullable disable
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.Engine.XUnit.Helpers;
using BudgetAnalyser.Engine.XUnit.TestData;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Ledger;

public class LedgerCalculationTest
{
    public LedgerCalculationTest()
    {
        Subject = new LedgerCalculation(new DebugLogger());
        TestData = LedgerBookTestData.TestData1();
    }

    private LedgerCalculation Subject { get; }
    private LedgerBook TestData { get; }

    [Fact]
    public void CalculateCurrentFortnightLedgerBalances_ShouldNotCountAutomatchTransactions_GivenAutoMatchTransactions()
    {
        var ledgerLine = CreateLedgerBookTestData().Reconciliations.First();
        var result = Subject.CalculateCurrentPeriodLedgerBalances(
            ledgerLine,
            new GlobalFilterCriteria { BeginDate = new DateOnly(2015, 11, 01), EndDate = new DateOnly(2015, 11, 15) },
            CreateTransactionsTestData());

        result[LedgerBookTestData.HouseInsLedgerSavingsAccount.BudgetBucket].ShouldBe(120M);
    }

    [Fact]
    public void CalculateCurrentMonthLedgerBalances_ShouldNotCountAutomatchTransactions_GivenAutoMatchTransactions()
    {
        var ledgerLine = CreateLedgerBookTestData().Reconciliations.First();
        var result = Subject.CalculateCurrentPeriodLedgerBalances(
            ledgerLine,
            new GlobalFilterCriteria { BeginDate = new DateOnly(2015, 10, 20), EndDate = new DateOnly(2015, 11, 19) },
            CreateTransactionsTestData());

        result[LedgerBookTestData.HouseInsLedgerSavingsAccount.BudgetBucket].ShouldBe(20M);
    }

    [Fact]
    public void CalculateCurrentPeriodSurplusBalance_ShouldCountPayCCTransactionsAsSurplusTransactions()
    {
        var ledgerLine = CreateLedgerBookTestData().Reconciliations.First();
        var transactions = CreateTransactionsBuilder().AppendTransaction(
                new Transaction
                {
                    Account = TransactionsListModelTestData.ChequeAccount,
                    Amount = -100M,
                    BudgetBucket = TransactionsListModelTestData.PayCreditCard,
                    Date = new DateOnly(2015, 11, 2),
                    Reference1 = "Pay credit card debit from surplus"
                })
            .Build();
        var result = Subject.CalculateCurrentPeriodSurplusBalance(
            ledgerLine,
            new GlobalFilterCriteria { BeginDate = new DateOnly(2015, 10, 20), EndDate = new DateOnly(2015, 11, 19) },
            transactions);

        result.ShouldBe(2770M);
    }

    [Fact]
    public void CalculateCurrentPeriodSurplusBalance_ShouldNotCountPositivePayCCTransactions()
    {
        var ledgerLine = CreateLedgerBookTestData().Reconciliations.First();
        var transactions = CreateTransactionsBuilder()
            .AppendTransaction(new Transaction
            {
                Account = TransactionsListModelTestData.ChequeAccount,
                Amount = -100M,
                BudgetBucket = TransactionsListModelTestData.PayCreditCard,
                Date = new DateOnly(2015, 11, 2),
                Reference1 = "Pay credit card debit from surplus"
            })
            .AppendTransaction(new Transaction
            {
                Account = TransactionsListModelTestData.VisaAccount,
                Amount = 100M,
                BudgetBucket = TransactionsListModelTestData.PayCreditCard,
                Date = new DateOnly(2015, 11, 2),
                Reference1 = "Credit Visa account with payment"
            })
            .AppendTransaction(new Transaction
            {
                Account = TransactionsListModelTestData.ChequeAccount,
                Amount = -50M,
                BudgetBucket = TransactionsListModelTestData.SurplusBucket,
                Date = new DateOnly(2015, 11, 3),
                Reference1 = "Buy something cool with spare funds"
            })
            .AppendTransaction(new Transaction
            {
                Account = TransactionsListModelTestData.ChequeAccount,
                Amount = 30M,
                BudgetBucket = TransactionsListModelTestData.SurplusBucket,
                Date = new DateOnly(2015, 11, 3),
                Reference1 = "Refund"
            })
            .AppendTransaction(new Transaction
            {
                Account = TransactionsListModelTestData.ChequeAccount,
                Amount = -20M,
                BudgetBucket = TransactionsListModelTestData.SavingsBucket,
                Date = new DateOnly(2015, 11, 4),
                Reference1 = "Save for a rainy day"
            })
            .AppendTransaction(new Transaction
            {
                Account = TransactionsListModelTestData.ChequeAccount,
                Amount = 20M,
                BudgetBucket = TransactionsListModelTestData.SavingsBucket,
                Date = new DateOnly(2015, 11, 4),
                Reference1 = "Credits to savings shouldn't affect surplus balance"
            })
            .Build();
        var result = Subject.CalculateCurrentPeriodSurplusBalance(
            ledgerLine,
            new GlobalFilterCriteria { BeginDate = new DateOnly(2015, 10, 20), EndDate = new DateOnly(2015, 11, 19) },
            transactions);

        result.ShouldBe(2750M);
    }

    [Fact]
    public void CalculateCurrentPeriodSurplusBalance_ShouldNotCountSavingsTransactionsAsSurplusTransactions()
    {
        // There used to be a special bucket "SavingsCommitmentBucket" that was a special bucket type. When a debit was coded against this bucket it was thought of as debiting Surplus.
        // Now this is simply treated as a SavedUpForExpenseBucket like any other ledger. Debits will remove funds from this LedgerBucket and credit another bucket somewhere else.
        var ledgerLine = CreateLedgerBookTestData().Reconciliations.First();
        var transactions = CreateTransactionsBuilder().AppendTransaction(
                new Transaction
                {
                    Account = TransactionsListModelTestData.ChequeAccount,
                    Amount = -100M,
                    BudgetBucket = TransactionsListModelTestData.SavingsBucket,
                    Date = new DateOnly(2015, 11, 2),
                    Reference1 = "Yee har"
                })
            .Build();
        var result = Subject.CalculateCurrentPeriodSurplusBalance(
            ledgerLine,
            new GlobalFilterCriteria { BeginDate = new DateOnly(2015, 10, 20), EndDate = new DateOnly(2015, 11, 19) },
            transactions);

        result.ShouldBe(2870M);
    }

    [Fact]
    public void CalculateCurrentPeriodSurplusBalance_UsingFortnightlyData_ShouldReturn3777()
    {
        var transactions = TransactionsListModelTestData.TestData1();
        var filter = new GlobalFilterCriteria { BeginDate = new DateOnly(2013, 8, 15), EndDate = new DateOnly(2013, 8, 29) };
        var ledgerLine = LedgerBookTestData.TestData6().Reconciliations.OrderByDescending(l => l.Date).First();

        var result = Subject.CalculateCurrentPeriodSurplusBalance(ledgerLine, filter, transactions);

        result.ShouldBe(3777.56M);
    }

    [Fact]
    public void CalculateOverSpentLedgersShouldReturnEmptyGivenNoLedgerLineForGivenDate()
    {
        var beginDate = new DateOnly(2014, 07, 01);
        var endDate = beginDate.AddMonths(1).AddDays(-1);
        var ledgerLine = TestData.Reconciliations.First();
        var result = Subject.CalculateOverSpentLedgers(TransactionsListModelTestData.TestData1(), ledgerLine, beginDate, endDate);
        result.Any().ShouldBeFalse();
    }

    [Fact]
    public void CalculateOverSpentLedgersShouldReturnEmptyGivenNoLedgersWereOverdrawn()
    {
        TestData.Output(true);
        var beginDate = new DateOnly(2013, 08, 15);
        var endDate = beginDate.AddMonths(1).AddDays(-1);
        var ledgerLine = TestData.Reconciliations.First();
        var result = Subject.CalculateOverSpentLedgers(TransactionsListModelTestData.TestData3(), ledgerLine, beginDate, endDate);
        foreach (var txn in result)
        {
            Console.WriteLine("{0} {1} {2}", txn.Date, txn.Narrative, txn.Amount);
        }

        result.Count().ShouldBe(0);
    }

    [Fact]
    public void CalculateOverSpentLedgersShouldReturnOverdrawnTransactionsGivenTransactionsSpendMoreThanLedgerBalance()
    {
        TestData.Output(true);
        var beginDate = new DateOnly(2013, 08, 15);
        var endDate = beginDate.AddMonths(1).AddDays(-1);
        var ledgerLine = TestData.Reconciliations.First();
        var result = Subject.CalculateOverSpentLedgers(TransactionsListModelTestData.TestData2(), ledgerLine, beginDate, endDate);
        foreach (var txn in result)
        {
            Console.WriteLine("{0} {1} {2}", txn.Date, txn.Narrative, txn.Amount);
        }

        result.Sum(t => t.Amount).ShouldBe(-40.41M);
    }

    [Fact]
    public void CalculateOverSpentLedgersShouldThrowGivenNullLedger()
    {
        var beginDate = new DateOnly(2014, 07, 01);
        var endDate = beginDate.AddMonths(1).AddDays(-1);
        Should.Throw<ArgumentNullException>(() => Subject.CalculateOverSpentLedgers(TransactionsListModelTestData.TestData1(), null!, beginDate, endDate));
    }

    [Fact]
    public void CalculateOverSpentLedgersShouldThrowGivenNullTransactions()
    {
        var beginDate = new DateOnly(2014, 07, 01);
        var endDate = beginDate.AddMonths(1).AddDays(-1);
        var ledgerLine = TestData.Reconciliations.First();

        Should.Throw<ArgumentNullException>(() => Subject.CalculateOverSpentLedgers(null!, ledgerLine, beginDate, endDate));
    }

    [Fact]
    public void UsingTestData1_LocateApplicableLedgerBalance_ShouldReturn64()
    {
        var filter = new GlobalFilterCriteria { BeginDate = new DateOnly(2013, 04, 15), EndDate = new DateOnly(2013, 05, 15) };
        var result = Subject.LocateApplicableLedgerBalance(TestData, filter, TransactionsListModelTestData.PhoneBucket.Code);
        result.ShouldBe(0M);
    }

    [Fact]
    public void UsingTestData1WithAugust15_LocateApplicableLedgerBalance_ShouldReturn64()
    {
        var filter = new GlobalFilterCriteria { BeginDate = new DateOnly(2013, 07, 15), EndDate = new DateOnly(2013, 08, 15) };

        var result = Subject.LocateApplicableLedgerBalance(TestData, filter, TransactionsListModelTestData.PhoneBucket.Code);
        TestData.Output();
        result.ShouldBe(64.71M);
    }

    private static LedgerBook CreateLedgerBookTestData()
    {
        return new LedgerBookBuilder { StorageKey = "BudgetBucketMonitorWidgetTest.xml", Modified = new DateTime(2015, 11, 23), Name = "Smith Budget 2015" }
            .IncludeLedger(LedgerBookTestData.PhoneLedger, 50M)
            .IncludeLedger(LedgerBookTestData.HouseInsLedgerSavingsAccount, 200M)
            .AppendReconciliation(
                new DateOnly(2015, 10, 20),
                new BankBalance(LedgerBookTestData.ChequeAccount, 2000M),
                new BankBalance(LedgerBookTestData.SavingsAccount, 1000M))
            .WithReconciliationEntries(entryBuilder =>
            {
                entryBuilder.WithLedger(LedgerBookTestData.PhoneLedger)
                    .AppendTransactions(txnBuilder => { txnBuilder.WithCredit(100, "Soo", new DateOnly(2015, 10, 20), "automatchref12"); })
                    .AppendTransactions(txnBuilder => { txnBuilder.WithCredit(-60, "Soo1", new DateOnly(2015, 10, 20)); });
                entryBuilder.WithLedger(LedgerBookTestData.HouseInsLedgerSavingsAccount)
                    .AppendTransactions(txnBuilder => { txnBuilder.WithCredit(-100, "Foo", new DateOnly(2015, 10, 20), "automatchref12"); })
                    .AppendTransactions(txnBuilder => { txnBuilder.WithCredit(-70, "Foo1", new DateOnly(2015, 10, 20)); });
            })
            .Build();
    }

    private static TransactionsListModelBuilder CreateTransactionsBuilder()
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
                });
    }

    private static TransactionsListModel CreateTransactionsTestData()
    {
        return CreateTransactionsBuilder().Build();
    }
}
