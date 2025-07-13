using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Reconciliation;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.UnitTest.Helper;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Moq;
using Rees.UnitTestUtilities;

namespace BudgetAnalyser.Engine.UnitTest.Ledger;

[TestClass]
// ReSharper disable once InconsistentNaming
public class ReconciliationCreationManagerTest
{
    private static readonly IEnumerable<BankBalance> TestDataBankBalances = new[] { new BankBalance(StatementModelTestData.ChequeAccount, 2050M) };
    private static readonly DateOnly TestDataReconcileDate = new(2013, 09, 15);

    private IBudgetBucketRepository bucketRepo;
    private IEnumerable<BankBalance> currentBankBalances;
    private Mock<IReconciliationBuilder> mockReconciliationBuilder;
    private Mock<IReconciliationConsistency> mockReconciliationConsistency;
    private Mock<ITransactionRuleService> mockRuleService;
    private ReconciliationCreationManager subject;
    private BudgetCollection testDataBudgetCollection;
    private BudgetModel testDataBudgetModel;
    private LedgerBook testDataLedgerBook;
    private ReconciliationResult testDataReconResult;
    private IList<ToDoTask> testDataToDoList;
    private TransactionSetModel testDataTransactionSet;

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
    public void Reconcile_ShouldAddLedgerEntryLineToLedgerBook()
    {
        this.mockReconciliationBuilder.Setup(m => m.CreateNewMonthlyReconciliation(TestDataReconcileDate, this.testDataBudgetModel, this.testDataTransactionSet, It.IsAny<BankBalance[]>()))
            .Returns(this.testDataReconResult);
        var reconcileCalled = false;
        ((LedgerBookTestHarness)this.testDataLedgerBook).ReconcileOverride = _ =>
        {
            // record the fact Reconcile was called.
            reconcileCalled = true;
        };

        ActPeriodEndReconciliation();

        Assert.IsTrue(reconcileCalled);
    }

    [TestMethod]
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
                    BucketCode = StatementModelTestData.CarMtcBucket.Code,
                    DestinationAccount = LedgerBookTestData.SavingsAccount,
                    SourceAccount = LedgerBookTestData.ChequeAccount
                });
            this.testDataReconResult = new ReconciliationResult { Reconciliation = recon.Reconciliation, Tasks = this.testDataToDoList };
        };

        // Expect a call to the Rule service to create the single use rule for the transfer.
        this.mockRuleService.Setup(m => m.CreateNewSingleUseRule(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<decimal?>(), true))
            .Returns(new SingleUseMatchingRule(this.bucketRepo));

        this.mockReconciliationBuilder.Setup(m => m.CreateNewMonthlyReconciliation(TestDataReconcileDate, this.testDataBudgetModel, this.testDataTransactionSet, It.IsAny<BankBalance[]>()))
            .Returns(this.testDataReconResult);

        ActPeriodEndReconciliation();

        // Ensure the rule service was called with the appropriate parameters.
        this.mockRuleService.VerifyAll();
    }

    [TestMethod]
    public void Reconcile_ShouldNotThrow_GivenTestData1AndUnclassifiedTransactionsOutsideReconPeriod()
    {
        this.mockReconciliationBuilder.Setup(m => m.CreateNewMonthlyReconciliation(TestDataReconcileDate, this.testDataBudgetModel, this.testDataTransactionSet, It.IsAny<BankBalance[]>()))
            .Returns(this.testDataReconResult);
        var aTransaction = this.testDataTransactionSet.AllTransactions.First();
        PrivateAccessor.SetField(aTransaction, "budgetBucket", null!);

        ActPeriodEndReconciliation(TestDataReconcileDate);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Reconcile_ShouldThrow_GivenInvalidLedgerBook()
    {
        var myTestDate = new DateOnly(2012, 2, 20);

        // Make sure there is a valid budget to isolate the invalid ledger book test
        var budget = this.testDataBudgetCollection.OrderBy(b => b.EffectiveFrom).First();
        budget.EffectiveFrom = myTestDate.AddDays(-1);

        ActPeriodEndReconciliation(myTestDate);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Reconcile_ShouldThrow_GivenNoEffectiveBudget()
    {
        var myTestDate = new DateOnly(2012, 2, 20);
        ActPeriodEndReconciliation(myTestDate);
    }

    [TestMethod]
    public void Reconcile_ShouldThrow_GivenTestData1AndNoStatementModelTransactions()
    {
        this.testDataTransactionSet = new TransactionSetModel(new FakeLogger()) { StorageKey = "C:\\Foo.xml" };
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

        Assert.Fail();
    }

    [TestMethod]
    public void Reconcile_ShouldThrow_GivenTestData1AndUnclassifiedTransactions()
    {
        this.testDataTransactionSet = new StatementModelBuilder()
            .TestData1()
            .AppendTransaction(new Transaction { Account = StatementModelTestData.ChequeAccount, Amount = 12.45M, Date = TestDataReconcileDate.AddDays(-1), Description = "Foo bar" })
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

        Assert.Fail();
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Reconcile_ShouldThrow_GivenTestData1WithDateEqualToExistingLine()
    {
        ActPeriodEndReconciliation(new DateOnly(2013, 08, 15));
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Reconcile_ShouldThrow_GivenTestData1WithDateLessThanExistingLine()
    {
        ActPeriodEndReconciliation(new DateOnly(2013, 07, 15));
    }

    [TestMethod]
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

        Assert.Fail();
    }

    [TestMethod]
    [Description("When there is more than one problem, the first exception should not prevent the user from seeing the other different exception.")]
    public void Reconcile_ShouldThrowValidationWarning_GivenTwoOrMoreWarningsHaveAlreadyBeenThrown()
    {
        // First the statement has a transaction that is not classified with a bucket.
        this.testDataTransactionSet = new StatementModelBuilder()
            .TestData1()
            .AppendTransaction(new Transaction { Account = StatementModelTestData.ChequeAccount, Amount = 12.45M, Date = TestDataReconcileDate.AddDays(-1), Description = "Foo bar" })
            .Build();
        try
        {
            ActPeriodEndReconciliation();
        }
        catch (ValidationWarningException ex)
        {
            if (ex.Source != "3")
            {
                Assert.Fail();
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

        Assert.Fail();
    }

    [TestMethod]
    public void Reconcile_ShouldThrowWhenAutoMatchingTransactionAreMissingFromStatement_GivenTestData5()
    {
        try
        {
            ActPeriodEndReconciliationOnTestData5(StatementModelTestData.TestData4());
        }
        catch (ValidationWarningException ex)
        {
            if (ex.Source == "1")
            {
                return;
            }
        }

        Assert.Fail();
    }

    [TestInitialize]
    public void TestInitialise()
    {
        this.mockRuleService = new Mock<ITransactionRuleService>(MockBehavior.Strict);
        this.mockReconciliationBuilder = new Mock<IReconciliationBuilder>();
        this.mockReconciliationConsistency = new Mock<IReconciliationConsistency>();
        this.bucketRepo = new BucketBucketRepoAlwaysFind();
        this.testDataBudgetCollection = BudgetModelTestData.CreateCollectionWith1And2();
        this.testDataBudgetModel = this.testDataBudgetCollection.ForDate(TestDataReconcileDate);
        this.testDataTransactionSet = new StatementModelBuilder()
            .TestData5()
            .AppendTransaction(new Transaction
            {
                Account = StatementModelTestData.ChequeAccount,
                Amount = -23.56M,
                BudgetBucket = StatementModelTestData.RegoBucket,
                Date = TestDataReconcileDate.AddDays(-1),
                TransactionType = new NamedTransaction("Foo"),
                Description = "Last transaction"
            })
            .Build();
        this.testDataToDoList = new List<ToDoTask>();

        this.subject = new ReconciliationCreationManager(this.mockRuleService.Object, this.mockReconciliationConsistency.Object, this.mockReconciliationBuilder.Object, new FakeLogger());

        this.testDataLedgerBook = LedgerBookTestData.TestData5(recons => new LedgerBookTestHarness(recons) { StorageKey = "Test Ledger Book.xaml" });

        this.mockReconciliationConsistency.Setup(m => m.EnsureConsistency(It.IsAny<LedgerBook>())).Returns(new Mock<IDisposable>().Object);

        this.testDataReconResult = new ReconciliationResult { Tasks = this.testDataToDoList, Reconciliation = new LedgerEntryLine(TestDataReconcileDate, TestDataBankBalances) };
    }

    private ReconciliationResult ActPeriodEndReconciliation(DateOnly? reconciliationDate = null, IEnumerable<BankBalance> bankBalances = null, bool ignoreWarnings = false)
    {
        this.currentBankBalances = bankBalances ?? TestDataBankBalances;

        Console.WriteLine("********************** BEFORE RUNNING RECONCILIATION *******************************");
        this.testDataLedgerBook.Output(true);

        var result = this.subject.PeriodEndReconciliation(this.testDataLedgerBook,
            reconciliationDate ?? TestDataReconcileDate,
            this.testDataBudgetCollection,
            this.testDataTransactionSet,
            ignoreWarnings,
            this.currentBankBalances.ToArray());
        Console.WriteLine("********************** AFTER RUNNING RECONCILIATION *******************************");
        result.Reconciliation.Output(LedgerBookHelper.LedgerOrder(this.testDataLedgerBook), true, true);

        return result;
    }

    private void ActPeriodEndReconciliationOnTestData5(TransactionSetModel transactionSetModelTestData = null, bool ignoreWarnings = false)
    {
        this.testDataBudgetCollection = BudgetModelTestData.CreateCollectionWith5();
        this.testDataBudgetModel = this.testDataBudgetCollection.ForDate(TestDataReconcileDate);
        this.testDataTransactionSet = transactionSetModelTestData ?? StatementModelTestData.TestData5();

        Console.WriteLine("********************** BEFORE RUNNING RECONCILIATION *******************************");
        this.testDataTransactionSet.Output(TestDataReconcileDate.AddMonths(-1));
        this.testDataLedgerBook.Output(true);

        var result = ActPeriodEndReconciliation(bankBalances: new[] { new BankBalance(StatementModelTestData.ChequeAccount, 1850.5M), new BankBalance(StatementModelTestData.SavingsAccount, 1200M) },
            ignoreWarnings: ignoreWarnings);

        Console.WriteLine();
        Console.WriteLine("********************** AFTER RUNNING RECONCILIATION *******************************");
        result.Reconciliation.Output(LedgerBookHelper.LedgerOrder(this.testDataLedgerBook), true, true);
    }
}
