#nullable disable
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Reconciliation;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.Engine.XUnit.Helpers;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using NSubstitute;
using Rees.UnitTestUtilities;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Ledger;

// ReSharper disable once InconsistentNaming
public class ReconciliationCreationManagerTest
{
    private static readonly IEnumerable<BankBalance> TestDataBankBalances = new[] { new BankBalance(TransactionsListModelTestData.ChequeAccount, 2050M) };
    private static readonly DateOnly TestDataReconcileDate = new(2013, 09, 15);

    private readonly IBudgetBucketRepository bucketRepo;
    private IEnumerable<BankBalance> currentBankBalances;
    private readonly IReconciliationBuilder mockReconciliationBuilder;
    private readonly IReconciliationConsistency mockReconciliationConsistency;
    private readonly ITransactionRuleService mockRuleService;
    private readonly ReconciliationCreationManager subject;
    private BudgetCollection testDataBudgetCollection;
    private BudgetModel testDataBudgetModel;
    private readonly LedgerBook testDataLedgerBook;
    private ReconciliationResult testDataReconResult;
    private readonly IList<ToDoTask> testDataToDoList;
    private TransactionsListModel testDataTransactions;

    public ReconciliationCreationManagerTest()
    {
        this.mockRuleService = Substitute.For<ITransactionRuleService>();
        this.mockReconciliationBuilder = Substitute.For<IReconciliationBuilder>();
        this.mockReconciliationConsistency = Substitute.For<IReconciliationConsistency>();
        this.bucketRepo = new BudgetBucketRepoAlwaysFind();
        this.testDataBudgetCollection = BudgetModelTestData.CreateCollectionWith1And2();
        this.testDataBudgetModel = this.testDataBudgetCollection.ForDate(TestDataReconcileDate);
        this.testDataTransactions = new TransactionsListModelBuilder()
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
        this.testDataToDoList = new List<ToDoTask>();

        this.subject = new ReconciliationCreationManager(this.mockRuleService, this.mockReconciliationConsistency, this.mockReconciliationBuilder, new FakeLogger());

        this.testDataLedgerBook = LedgerBookTestData.TestData5(recons => new LedgerBookTestHarness(recons) { StorageKey = "Test Ledger Book.xaml" });

        this.mockReconciliationConsistency.EnsureConsistency(Arg.Any<LedgerBook>()).Returns(Substitute.For<IDisposable>());

        this.testDataReconResult = new ReconciliationResult { Tasks = this.testDataToDoList, Reconciliation = new LedgerEntryLine(TestDataReconcileDate, TestDataBankBalances) };
    }

    [Fact]
    public void OutputTestData1()
    {
        var book = new LedgerBookBuilder().TestData1().Build();
        book.Output(true);
    }

    [Fact]
    public void OutputTestData5()
    {
        LedgerBookTestData.TestData5().Output(true);
    }

    [Fact]
    public void Reconcile_ShouldAddLedgerEntryLineToLedgerBook()
    {
        this.mockReconciliationBuilder.CreateNewMonthlyReconciliation(TestDataReconcileDate, this.testDataBudgetModel, this.testDataTransactions, Arg.Any<BankBalance[]>())
            .Returns(this.testDataReconResult);
        var reconcileCalled = false;
        ((LedgerBookTestHarness)this.testDataLedgerBook).ReconcileOverride = _ =>
        {
            // record the fact Reconcile was called.
            reconcileCalled = true;
        };

        ActPeriodEndReconciliation();

        reconcileCalled.ShouldBeTrue();
    }

    [Fact]
    public void Reconcile_ShouldCreateSingleUseMatchingRulesForTransferToDos()
    {
        // Artificially create a transfer to do task when the reconciliation method is invoked on the LedgerBook.
        // Remember: the subject here is the ReconciliationCreationManager not the LedgerBook.
        ((LedgerBookTestHarness)this.testDataLedgerBook).ReconcileOverride = recon =>
        {
            this.testDataToDoList.Add(
                new TransferTask
                {
                    SystemGenerated = true,
                    Description = string.Empty,
                    Reference = "sjghsh",
                    Amount = 12.22M,
                    BucketCode = TransactionsListModelTestData.CarMtcBucket.Code,
                    DestinationAccount = LedgerBookTestData.SavingsAccount,
                    SourceAccount = LedgerBookTestData.ChequeAccount
                });
            this.testDataReconResult = new ReconciliationResult { Reconciliation = recon.Reconciliation, Tasks = this.testDataToDoList };
        };

        // Expect a call to the Rule service to create the single use rule for the transfer.
        this.mockRuleService.CreateNewSingleUseRule(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<string>(), Arg.Any<decimal?>(), true)
            .Returns(new SingleUseMatchingRule(this.bucketRepo));

        this.mockReconciliationBuilder.CreateNewMonthlyReconciliation(TestDataReconcileDate, this.testDataBudgetModel, this.testDataTransactions, Arg.Any<BankBalance[]>())
            .Returns(this.testDataReconResult);

        ActPeriodEndReconciliation();

        // Ensure the rule service was called with the appropriate parameters.
        this.mockRuleService.Received().CreateNewSingleUseRule(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<string>(), Arg.Any<decimal?>(), true);
    }

    [Fact]
    public void Reconcile_ShouldNotThrow_GivenTestData1AndUnclassifiedTransactionsOutsideReconPeriod()
    {
        this.mockReconciliationBuilder.CreateNewMonthlyReconciliation(TestDataReconcileDate, this.testDataBudgetModel, this.testDataTransactions, Arg.Any<BankBalance[]>())
            .Returns(this.testDataReconResult);
        var aTransaction = this.testDataTransactions.AllTransactions.First();
        PrivateAccessor.SetField(aTransaction, "<BudgetBucket>k__BackingField", null!);

        ActPeriodEndReconciliation(TestDataReconcileDate);
    }

    [Fact]
    public void Reconcile_ShouldThrow_GivenInvalidLedgerBook()
    {
        var myTestDate = new DateOnly(2012, 2, 20);

        // Make sure there is a valid budget to isolate the invalid ledger book test
        var budget = this.testDataBudgetCollection.OrderBy(b => b.EffectiveFrom).First();
        budget.EffectiveFrom = myTestDate.AddDays(-1);

        Should.Throw<InvalidOperationException>(() => ActPeriodEndReconciliation(myTestDate));
    }

    [Fact]
    public void Reconcile_ShouldThrow_GivenNoEffectiveBudget()
    {
        var myTestDate = new DateOnly(2012, 2, 20);
        Should.Throw<ArgumentNullException>(() => ActPeriodEndReconciliation(myTestDate));
    }

    [Fact]
    public void Reconcile_ShouldThrow_GivenTestData1AndNoTransactions()
    {
        this.testDataTransactions = new TransactionsListModel(new FakeLogger()) { StorageKey = "C:\\Foo.xml" };
        try
        {
            ActPeriodEndReconciliation(new DateOnly(2013, 10, 15));
        }
        catch (ValidationWarningException ex)
        {
            if (ex.Source == "5")
            {
                return;
            }
        }

        true.ShouldBeFalse("Expected exception was not thrown.");
    }

    [Fact]
    public void Reconcile_ShouldThrow_GivenTestData1AndUnclassifiedTransactions()
    {
        this.testDataTransactions = new TransactionsListModelBuilder()
            .TestData1()
            .AppendTransaction(new Transaction { Account = TransactionsListModelTestData.ChequeAccount, Amount = 12.45M, Date = TestDataReconcileDate.AddDays(-1), Description = "Foo bar" })
            .Build();
        try
        {
            ActPeriodEndReconciliation();
        }
        catch (ValidationWarningException ex)
        {
            if (ex.Source == "3")
            {
                return;
            }
        }

        true.ShouldBeFalse("Expected exception was not thrown.");
    }

    [Fact]
    public void Reconcile_ShouldThrow_GivenTestData1WithDateEqualToExistingLine()
    {
        Should.Throw<InvalidOperationException>(() => ActPeriodEndReconciliation(new DateOnly(2013, 08, 15)));
    }

    [Fact]
    public void Reconcile_ShouldThrow_GivenTestData1WithDateLessThanExistingLine()
    {
        Should.Throw<InvalidOperationException>(() => ActPeriodEndReconciliation(new DateOnly(2013, 07, 15)));
    }

    [Fact]
    public void Reconcile_ShouldThrowValidationWarning_GivenStatmentIsMissingTransactionsForLastDay()
    {
        try
        {
            ActPeriodEndReconciliationOnTestData5();
        }
        catch (ValidationWarningException ex)
        {
            if (ex.Source == "2")
            {
                return;
            }
        }

        true.ShouldBeFalse("Expected exception was not thrown.");
    }

    [Fact]
    public void Reconcile_ShouldThrowValidationWarning_GivenTwoOrMoreWarningsHaveAlreadyBeenThrown()
    {
        // First the transaction that is not classified with a bucket.
        this.testDataTransactions = new TransactionsListModelBuilder()
            .TestData1()
            .AppendTransaction(new Transaction { Account = TransactionsListModelTestData.ChequeAccount, Amount = 12.45M, Date = TestDataReconcileDate.AddDays(-1), Description = "Foo bar" })
            .Build();
        try
        {
            ActPeriodEndReconciliation();
        }
        catch (ValidationWarningException ex)
        {
            if (ex.Source != "3")
            {
                true.ShouldBeFalse("Expected exception was not thrown.");
            }
        }

        // Second time thru, we choose to ignore validation warnings messages we've seen before.
        try
        {
            ActPeriodEndReconciliationOnTestData5(ignoreWarnings: true);
        }
        catch (ValidationWarningException ex)
        {
            if (ex.Source == "2")
            {
                return;
            }
        }

        true.ShouldBeFalse("Expected exception was not thrown.");
    }

    [Fact]
    public void Reconcile_ShouldThrowWhenAutoMatchingTransactionAreMissingFromTransactions_GivenTestData5()
    {
        try
        {
            ActPeriodEndReconciliationOnTestData5(TransactionsListModelTestData.TestData4());
        }
        catch (ValidationWarningException ex)
        {
            if (ex.Source == "1")
            {
                return;
            }
        }

        true.ShouldBeFalse("Expected exception was not thrown.");
    }

    private ReconciliationResult ActPeriodEndReconciliation(DateOnly? reconciliationDate = null, IEnumerable<BankBalance> bankBalances = null, bool ignoreWarnings = false)
    {
        this.currentBankBalances = bankBalances ?? TestDataBankBalances;

        Console.WriteLine("********************** BEFORE RUNNING RECONCILIATION *******************************");
        this.testDataLedgerBook.Output(true);

        var result = this.subject.PeriodEndReconciliation(this.testDataLedgerBook,
            reconciliationDate ?? TestDataReconcileDate,
            this.testDataBudgetCollection,
            this.testDataTransactions,
            ignoreWarnings,
            this.currentBankBalances.ToArray());
        Console.WriteLine("********************** AFTER RUNNING RECONCILIATION *******************************");
        result.Reconciliation.Output(LedgerBookHelper.LedgerOrder(this.testDataLedgerBook), true, true);

        return result;
    }

    private void ActPeriodEndReconciliationOnTestData5(TransactionsListModel transactionsListModelTestData = null, bool ignoreWarnings = false)
    {
        this.testDataBudgetCollection = BudgetModelTestData.CreateCollectionWith5();
        this.testDataBudgetModel = this.testDataBudgetCollection.ForDate(TestDataReconcileDate);
        this.testDataTransactions = transactionsListModelTestData ?? TransactionsListModelTestData.TestData5();

        Console.WriteLine("********************** BEFORE RUNNING RECONCILIATION *******************************");
        this.testDataTransactions.Output(TestDataReconcileDate.AddMonths(-1));
        this.testDataLedgerBook.Output(true);

        var result = ActPeriodEndReconciliation(
            bankBalances: new[] { new BankBalance(TransactionsListModelTestData.ChequeAccount, 1850.5M), new BankBalance(TransactionsListModelTestData.SavingsAccount, 1200M) },
            ignoreWarnings: ignoreWarnings);

        Console.WriteLine();
        Console.WriteLine("********************** AFTER RUNNING RECONCILIATION *******************************");
        result.Reconciliation.Output(LedgerBookHelper.LedgerOrder(this.testDataLedgerBook), true, true);
    }
}
