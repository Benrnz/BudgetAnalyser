using System.Diagnostics;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Reconciliation;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.UnitTest.Helper;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;

namespace BudgetAnalyser.Engine.UnitTest.Ledger;

[TestClass]
// ReSharper disable once InconsistentNaming
public class ReconciliationBuilderTest
{
    private static readonly IEnumerable<BankBalance> TestDataBankBalances = new[] { new BankBalance(TransactionsListModelTestData.ChequeAccount, 2050M) };
    private static readonly DateOnly TestDataReconcileDate = new(2013, 09, 15);

    private IEnumerable<BankBalance> currentBankBalances;
    private ReconciliationBuilder subject;
    private IBudgetCurrencyContext testDataBudgetContext;
    private TransactionsListModel testDataTransactionsList;

    [TestMethod]
    public void AddLedger_ShouldIncludeNewLedgerInNextReconcile_GivenTestData1()
    {
        this.subject.LedgerBook.AddLedger(LedgerBookTestData.RatesLedger);
        var result = ActPeriodEndReconciliation();

        Assert.IsTrue(result.Reconciliation.Entries.Any(e => e.LedgerBucket == LedgerBookTestData.RatesLedger));
    }

    [TestMethod]
    public void OutputTestData1()
    {
        var book = new LedgerBookBuilder().TestData1().Build();
        book.Output(true);
    }

    [TestMethod]
    public void OutputTestData5()
    {
        LedgerBookTestData.TestData5().Output(true);
    }

    [TestMethod]
    [Description("Ensures that the reconciliation process finds ledger transactions from the previous month that required funds to be transfered and matches these to " +
                 "transactions with auto-matching Id")]
    public void Reconcile_ShouldAutoMatchTransactionsAndLinkIdToTransaction_GivenTestData5()
    {
        // The auto-matched credit ledger transaction from last month should be linked to the transaction.
        this.testDataTransactionsList = TransactionsListModelTestData.TestData5();
        var transactions = this.testDataTransactionsList.AllTransactions.Where(t => t.Reference1 == "agkT9kC").ToList();
        Debug.Assert(transactions.Count() == 2);

        ActPeriodEndReconciliationOnTestData5(this.testDataTransactionsList);
        var previousMonthLine =
            this.subject.LedgerBook.Reconciliations.Single(line => line.Date == new DateOnly(2013, 08, 15)).Entries.Single(e => e.LedgerBucket.BudgetBucket == TransactionsListModelTestData.InsHomeBucket);
        var previousLedgerTxn = previousMonthLine.Transactions.OfType<BudgetCreditLedgerTransaction>().Single();

        // Assert last month's ledger transaction has been linked to the credit 16/8/13
        Assert.AreEqual(transactions.Single(t => t.Amount > 0).Id, previousLedgerTxn.Id);
    }

    [TestMethod]
    [Description("Ensures the reconciliation process matches transactions that should be automatched and then ignored - not imported into the Ledger Transaction listing." +
                 "If this is not working there will be more than 3 ledger transactions, because there are two transactions for INSHOME that should be matched and ignored.")]
    public void Reconcile_ShouldAutoMatchTransactionsAndResultIn3InsHomeTransactions_GivenTestData5()
    {
        // Two transactions should be removed as they are auto-matched to the previous month.
        var result = ActPeriodEndReconciliationOnTestData5();

        Assert.AreEqual(3, result.Reconciliation.Entries.Single(e => e.LedgerBucket.BudgetBucket == TransactionsListModelTestData.InsHomeBucket).Transactions.Count());
        // Assert last month's ledger transaction has been linked to the credit 16/8/13
    }

    [TestMethod]
    public void Reconcile_ShouldAutoMatchTransactionsAndResultInInsHomeBalance300_GivenTestData5()
    {
        var result = ActPeriodEndReconciliationOnTestData5();
        Assert.AreEqual(300M, result.Reconciliation.Entries.Single(e => e.LedgerBucket.BudgetBucket == TransactionsListModelTestData.InsHomeBucket).Balance);
    }

    [TestMethod]
    public void Reconcile_ShouldAutoMatchTransactionsAndUpdateLedgerAutoMatchRefSoItIsNotAutoMatchedAgain_GivenTestData5()
    {
        // Two transactions should be removed as they are auto-matched to the previous month.
        ActPeriodEndReconciliationOnTestData5();
        var previousMonthLine = this.subject.LedgerBook.Reconciliations.Single(line => line.Date == TestDataReconcileDate.AddMonths(-1))
            .Entries.Single(e => e.LedgerBucket.BudgetBucket == TransactionsListModelTestData.InsHomeBucket);
        var previousLedgerTxn = previousMonthLine.Transactions.OfType<BudgetCreditLedgerTransaction>().Single();

        Console.WriteLine(previousLedgerTxn.AutoMatchingReference);
        Assert.AreNotEqual("agkT9kC", previousLedgerTxn.AutoMatchingReference);
    }

    [TestMethod]
    public void Reconcile_ShouldCreateBalanceAdjustmentOf150_GivenSavingsMonthlyBudgetAmountsSumTo150()
    {
        // 95 Car Mtc Monthly budget
        // 55 Hair cut monthly budget
        // ===
        // 150 Balance Adjustment expected in Savings
        // Power 175 goes in Chq
        TestIntialise(1, new LedgerBookBuilder()
            .IncludeLedger(new SavedUpForLedger { BudgetBucket = TransactionsListModelTestData.CarMtcBucket, StoredInAccount = LedgerBookTestData.SavingsAccount })
            .IncludeLedger(new SavedUpForLedger { BudgetBucket = TransactionsListModelTestData.HairBucket, StoredInAccount = LedgerBookTestData.SavingsAccount })
            .IncludeLedger(LedgerBookTestData.PowerLedger)
            .Build());

        var result = ActPeriodEndReconciliation();

        Assert.AreEqual(150M, result.Reconciliation.BankBalanceAdjustments.Single(b => b.BankAccount == LedgerBookTestData.SavingsAccount).Amount);
        Assert.AreEqual(-150M, result.Reconciliation.BankBalanceAdjustments.Single(b => b.BankAccount == LedgerBookTestData.ChequeAccount).Amount);
    }

    [TestMethod]
    public void Reconcile_ShouldCreateToDoEntries_GivenTestData5()
    {
        var result = ActPeriodEndReconciliationOnTestData5();
        OutputToDoList(result.Tasks);
        Assert.AreEqual(1, result.Tasks.OfType<TransferTask>().Count(t => t.Reference.IsSomething() && t.BucketCode.IsSomething()));
    }

    [TestMethod]
    public void Reconcile_ShouldNotCreateTasksForUserTransfersOfBudgetedAmounts_GivenTestData5()
    {
        var result = ActPeriodEndReconciliationOnTestData5();
        OutputToDoList(result.Tasks);

        // Given the test data 5 set, there should only be one transfer task.
        Assert.AreEqual(1, result.Tasks.OfType<TransferTask>().Count());
    }

    [TestMethod]
    [Description("This test is effectively testing two things: First that budgeted amount doesn't show up as a payment when there is a payment going out." +
                 "Second, that a payment transfer task is created successfully.")]
    public void Reconcile_ShouldNotCreateTasksForUserTransfersOfBudgetedAmounts_GivenTestData5AndPaymentFromDifferentAccount()
    {
        // Modify a InsHome payment transaction, originally coming out of the Savings account where the ledger is stored, to the Cheque account.
        this.testDataTransactionsList = TransactionsListModelTestData.TestData5();
        var insHomePayment = this.testDataTransactionsList.AllTransactions.Single(t => t.BudgetBucket == TransactionsListModelTestData.InsHomeBucket && t.Amount == -1000M);
        insHomePayment.Account = TransactionsListModelTestData.ChequeAccount;

        var result = ActPeriodEndReconciliationOnTestData5(this.testDataTransactionsList);
        OutputToDoList(result.Tasks);

        // Given the test data 5 set, there should only be one transfer task.
        Assert.AreEqual(2, result.Tasks.OfType<TransferTask>().Count());
    }

    [TestMethod]
    public void Reconcile_ShouldResultIn1678_GivenTestData1()
    {
        TestIntialise(1);
        var result = ActPeriodEndReconciliation(bankBalances: new[] { new BankBalance(TransactionsListModelTestData.ChequeAccount, 1850.5M) });
        Assert.AreEqual(1555.50M, result.Reconciliation.CalculatedSurplus);
    }

    [TestMethod]
    public void Reconcile_WithPaymentFromWrongAccountShouldUpdateLedgerBalance_GivenTestData5()
    {
        var budgetCollection = BudgetModelTestData.CreateCollectionWith5();
        this.testDataBudgetContext = new BudgetCurrencyContext(budgetCollection, budgetCollection.CurrentActiveBudget);
        var testTransaction = this.testDataTransactionsList.AllTransactions.Last();
        testTransaction.BudgetBucket = LedgerBookTestData.HouseInsLedgerSavingsAccount.BudgetBucket;
        testTransaction.Account = TransactionsListModelTestData.ChequeAccount;
        testTransaction.Amount = -1250;
        this.testDataTransactionsList.Output(DateOnly.MinValue);

        var reconResult = ActPeriodEndReconciliation(bankBalances: new[]
        {
            new BankBalance(TransactionsListModelTestData.ChequeAccount, 1850.5M), new BankBalance(TransactionsListModelTestData.SavingsAccount, 1000M)
        });

        Assert.AreEqual(300M, reconResult.Reconciliation.Entries.Single(e => e.LedgerBucket == LedgerBookTestData.HouseInsLedgerSavingsAccount).Balance);
    }

    [TestMethod]
    [Description("This test overdraws the Hair ledger and tests to make sure the reconciliation process compensates and leaves it with a balance equal to the monthly budget amount.")]
    public void Reconcile_WithTransactionsSavedUpForHairLedgerShouldHaveBalance55_GivenTestData1()
    {
        TestIntialise(1);
        var additionalTransactions = this.testDataTransactionsList.AllTransactions.ToList();

        additionalTransactions.Add(new Transaction
        {
            Account = additionalTransactions.First().Account,
            Amount = -264M,
            BudgetBucket = additionalTransactions.First(t => t.BudgetBucket.Code == TestDataConstants.HairBucketCode).BudgetBucket,
            Date = new DateOnly(2013, 09, 13)
        });
        this.testDataTransactionsList.LoadTransactions(additionalTransactions);

        var result = ActPeriodEndReconciliation();

        Assert.AreEqual(55M, result.Reconciliation.Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode).Balance);
        Assert.IsTrue(result.Reconciliation.Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode).NetAmount < 0);
    }

    [TestMethod]
    public void Reconcile_WithTransactionsShouldHave2HairTransactions_GivenTestData1()
    {
        var result = ActPeriodEndReconciliation();
        Assert.AreEqual(2, result.Reconciliation.Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode).Transactions.Count());
    }

    [TestMethod]
    public void Reconcile_WithTransactionsShouldHave3PowerTransactions_GivenTestData1()
    {
        TestIntialise(1);
        var result = ActPeriodEndReconciliation();
        Assert.AreEqual(3, result.Reconciliation.Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PowerBucketCode).Transactions.Count());
    }

    [TestMethod]
    public void Reconcile_WithTransactionsShouldHaveSurplus1613_GivenTestData1()
    {
        TestIntialise(1);
        var result = ActPeriodEndReconciliation(bankBalances: new[] { new BankBalance(TransactionsListModelTestData.ChequeAccount, 1850.5M) });
        Assert.AreEqual(1555.50M, result.Reconciliation.CalculatedSurplus);
    }

    [TestMethod]
    public void Reconcile_WithTransactionsSpentMonthlyLedgerShouldSupplementShortfall_GivenTestData1()
    {
        TestIntialise(1);
        var result = ActPeriodEndReconciliation();
        Assert.AreEqual(0M, result.Reconciliation.Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PhoneBucketCode).Balance);
    }

    [TestMethod]
    public void Reconcile_WithTransactionsWithBalanceAdjustment599ShouldHaveSurplus1014_GivenTestData1()
    {
        TestIntialise(1);
        var result = ActPeriodEndReconciliation(bankBalances: new[] { new BankBalance(TransactionsListModelTestData.ChequeAccount, 1850.5M) });
        result.Reconciliation.BalanceAdjustment(-599M, "Visa pmt not yet in transactions", new ChequeAccount("Chq"));
        Assert.AreEqual(956.50M, result.Reconciliation.CalculatedSurplus);
    }

    [TestInitialize]
    public void TestIntialise()
    {
        TestIntialise(5);
    }

    private ReconciliationResult ActPeriodEndReconciliation(DateOnly? reconciliationDate = null, IEnumerable<BankBalance> bankBalances = null, bool ignoreWarnings = false)
    {
        this.currentBankBalances = bankBalances ?? TestDataBankBalances;

        Debug.WriteLine("********************** BEFORE RUNNING RECONCILIATION *******************************");
        this.subject.LedgerBook.Output(true);

        var result = this.subject.CreateNewMonthlyReconciliation(reconciliationDate ?? TestDataReconcileDate,
            this.testDataBudgetContext.Model,
            this.testDataTransactionsList,
            this.currentBankBalances.ToArray());

        Debug.WriteLine("********************** AFTER RUNNING RECONCILIATION *******************************");
        result.Reconciliation.Output(LedgerBookHelper.LedgerOrder(this.subject.LedgerBook), true, true);

        return result;
    }

    private ReconciliationResult ActPeriodEndReconciliationOnTestData5(TransactionsListModel transactionsListModelTestData = null, bool ignoreWarnings = false)
    {
        var budgetCollection = BudgetModelTestData.CreateCollectionWith5();
        this.testDataBudgetContext = new BudgetCurrencyContext(budgetCollection, budgetCollection.CurrentActiveBudget);
        this.testDataTransactionsList = transactionsListModelTestData ?? TransactionsListModelTestData.TestData5();

        var result = ActPeriodEndReconciliation(bankBalances: new[] { new BankBalance(TransactionsListModelTestData.ChequeAccount, 1850.5M), new BankBalance(TransactionsListModelTestData.SavingsAccount, 1200M) },
            ignoreWarnings: ignoreWarnings);

        return result;
    }

    private void OutputToDoList(IEnumerable<ToDoTask> tasks)
    {
        Console.WriteLine("==================== TODO LIST ===========================");
        Console.WriteLine("Type       Generated  Reference  Amount     Description");
        foreach (var task in tasks)
        {
            Console.WriteLine(
                "{0} {1} {2} {3} {4}",
                task.GetType().Name.PadRight(10).Truncate(10),
                task.SystemGenerated.ToString().PadRight(10),
                (task as TransferTask)?.Reference?.PadRight(10).Truncate(10) ?? "          ",
                (task as TransferTask)?.Amount.ToString("C").PadRight(10).Truncate(10) ?? "          ",
                task.Description);
        }
    }

    private void TestIntialise(int testDataId, LedgerBook ledgerBook = null)
    {
        this.subject = new ReconciliationBuilder(new FakeLogger());
        var budgetCollection = BudgetModelTestData.CreateCollectionWith1And2();

        switch (testDataId)
        {
            case 1:
                this.testDataBudgetContext = new BudgetCurrencyContext(budgetCollection, budgetCollection.ForDate(TestDataReconcileDate)); // Should be BudgetModelTestData.CreateTestData1()
                this.testDataTransactionsList = new TransactionsListModelBuilder()
                    .TestData1()
                    .Build();
                this.subject.LedgerBook = ledgerBook ?? LedgerBookTestData.TestData1();
                break;
            case 5:
                this.testDataBudgetContext = new BudgetCurrencyContext(budgetCollection, budgetCollection.CurrentActiveBudget);
                this.testDataTransactionsList = new TransactionsListModelBuilder()
                    .TestData5()
                    .AppendTransaction(new Transaction
                    {
                        Account = TransactionsListModelTestData.ChequeAccount,
                        Amount = -23.56M,
                        BudgetBucket = TransactionsListModelTestData.RegoBucket,
                        Date = TestDataReconcileDate.AddDays(-1),
                        TransactionType = new NamedTransaction("Foo"),
                        Description = "Last transaction"
                    })
                    .Build();
                this.subject.LedgerBook = ledgerBook ?? LedgerBookTestData.TestData5(recons => new LedgerBookTestHarness(recons) { StorageKey = "Test Ledger Book.xaml" });
                break;
        }
    }
}
