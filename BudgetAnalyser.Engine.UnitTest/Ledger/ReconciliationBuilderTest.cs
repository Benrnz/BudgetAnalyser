using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Reconciliation;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.UnitTest.Helper;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rees.UnitTestUtilities;

namespace BudgetAnalyser.Engine.UnitTest.Ledger;

[TestClass]
// ReSharper disable once InconsistentNaming
public class ReconciliationBuilderTest
{
    private static readonly IEnumerable<BankBalance> TestDataBankBalances = new[] { new BankBalance(StatementModelTestData.ChequeAccount, 2050M) };
    private static readonly DateTime TestDataReconcileDate = new(2013, 09, 15);

    private IEnumerable<BankBalance> currentBankBalances;
    private ReconciliationBuilder subject;
    private IBudgetCurrencyContext testDataBudgetContext;
    private StatementModel testDataStatement;

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
                 "statement transactions with auto-matching Id")]
    public void Reconcile_ShouldAutoMatchTransactionsAndLinkIdToStatementTransaction_GivenTestData5()
    {
        // The auto-matched credit ledger transaction from last month should be linked to the statement transaction.
        this.testDataStatement = StatementModelTestData.TestData5();
        var statementTransactions = this.testDataStatement.AllTransactions.Where(t => t.Reference1 == "agkT9kC").ToList();
        Debug.Assert(statementTransactions.Count() == 2);

        ActPeriodEndReconciliationOnTestData5(this.testDataStatement);
        var previousMonthLine =
            this.subject.LedgerBook.Reconciliations.Single(line => line.Date == new DateTime(2013, 08, 15)).Entries.Single(e => e.LedgerBucket.BudgetBucket == StatementModelTestData.InsHomeBucket);
        var previousLedgerTxn = previousMonthLine.Transactions.OfType<BudgetCreditLedgerTransaction>().Single();

        // Assert last month's ledger transaction has been linked to the credit 16/8/13
        Assert.AreEqual(statementTransactions.Single(t => t.Amount > 0).Id, previousLedgerTxn.Id);
    }

    [TestMethod]
    [Description("Ensures the reconciliation process matches transactions that should be automatched and then ignored - not imported into the Ledger Transaction listing." +
                 "If this is not working there will be more than 3 ledger transactions, because there are two statement transactions for INSHOME that should be matched and ignored.")]
    public void Reconcile_ShouldAutoMatchTransactionsAndResultIn3InsHomeTransactions_GivenTestData5()
    {
        // Two transactions should be removed as they are auto-matched to the previous month.
        var result = ActPeriodEndReconciliationOnTestData5();

        Assert.AreEqual(3, result.Reconciliation.Entries.Single(e => e.LedgerBucket.BudgetBucket == StatementModelTestData.InsHomeBucket).Transactions.Count());
        // Assert last month's ledger transaction has been linked to the credit 16/8/13
    }

    [TestMethod]
    public void Reconcile_ShouldAutoMatchTransactionsAndResultInInsHomeBalance300_GivenTestData5()
    {
        var result = ActPeriodEndReconciliationOnTestData5();
        Assert.AreEqual(300M, result.Reconciliation.Entries.Single(e => e.LedgerBucket.BudgetBucket == StatementModelTestData.InsHomeBucket).Balance);
    }

    [TestMethod]
    public void Reconcile_ShouldAutoMatchTransactionsAndUpdateLedgerAutoMatchRefSoItIsNotAutoMatchedAgain_GivenTestData5()
    {
        // Two transactions should be removed as they are auto-matched to the previous month.
        ActPeriodEndReconciliationOnTestData5();
        var previousMonthLine = this.subject.LedgerBook.Reconciliations.Single(line => line.Date == TestDataReconcileDate.AddMonths(-1))
            .Entries.Single(e => e.LedgerBucket.BudgetBucket == StatementModelTestData.InsHomeBucket);
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
                          .IncludeLedger(new SavedUpForLedger { BudgetBucket = StatementModelTestData.CarMtcBucket, StoredInAccount = LedgerBookTestData.SavingsAccount })
                          .IncludeLedger(new SavedUpForLedger { BudgetBucket = StatementModelTestData.HairBucket, StoredInAccount = LedgerBookTestData.SavingsAccount })
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
        this.testDataStatement = StatementModelTestData.TestData5();
        var insHomePayment = this.testDataStatement.AllTransactions.Single(t => t.BudgetBucket == StatementModelTestData.InsHomeBucket && t.Amount == -1000M);
        insHomePayment.Account = StatementModelTestData.ChequeAccount;

        var result = ActPeriodEndReconciliationOnTestData5(this.testDataStatement);
        OutputToDoList(result.Tasks);

        // Given the test data 5 set, there should only be one transfer task.
        Assert.AreEqual(2, result.Tasks.OfType<TransferTask>().Count());
    }

    [TestMethod]
    public void Reconcile_ShouldResultIn1678_GivenTestData1()
    {
        TestIntialise(1);
        var result = ActPeriodEndReconciliation(bankBalances: new[] { new BankBalance(StatementModelTestData.ChequeAccount, 1850.5M) });
        Assert.AreEqual(1555.50M, result.Reconciliation.CalculatedSurplus);
    }

    [TestMethod]
    public void Reconcile_WithPaymentFromWrongAccountShouldUpdateLedgerBalance_GivenTestData5()
    {
        var budgetCollection = BudgetModelTestData.CreateCollectionWith5();
        this.testDataBudgetContext = new BudgetCurrencyContext(budgetCollection, budgetCollection.CurrentActiveBudget);
        var testTransaction = this.testDataStatement.AllTransactions.Last();
        testTransaction.BudgetBucket = LedgerBookTestData.HouseInsLedgerSavingsAccount.BudgetBucket;
        testTransaction.Account = StatementModelTestData.ChequeAccount;
        testTransaction.Amount = -1250;
        this.testDataStatement.Output(DateTime.MinValue);

        var reconResult = ActPeriodEndReconciliation(bankBalances: new[]
        {
            new BankBalance(StatementModelTestData.ChequeAccount, 1850.5M), new BankBalance(StatementModelTestData.SavingsAccount, 1000M)
        });

        Assert.AreEqual(300M, reconResult.Reconciliation.Entries.Single(e => e.LedgerBucket == LedgerBookTestData.HouseInsLedgerSavingsAccount).Balance);
    }

    [TestMethod]
    [Description("This test overdraws the Hair ledger and tests to make sure the reconciliation process compensates and leaves it with a balance equal to the monthly budget amount.")]
    public void Reconcile_WithStatementSavedUpForHairLedgerShouldHaveBalance55_GivenTestData1()
    {
        TestIntialise(1);
        var additionalTransactions = this.testDataStatement.AllTransactions.ToList();

        additionalTransactions.Add(new Transaction
        {
            Account = additionalTransactions.First().Account,
            Amount = -264M,
            BudgetBucket = additionalTransactions.First(t => t.BudgetBucket.Code == TestDataConstants.HairBucketCode).BudgetBucket,
            Date = new DateTime(2013, 09, 13)
        });
        this.testDataStatement.LoadTransactions(additionalTransactions);

        var result = ActPeriodEndReconciliation();

        Assert.AreEqual(55M, result.Reconciliation.Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode).Balance);
        Assert.IsTrue(result.Reconciliation.Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode).NetAmount < 0);
    }

    [TestMethod]
    public void Reconcile_WithStatementShouldHave2HairTransactions_GivenTestData1()
    {
        var result = ActPeriodEndReconciliation();
        Assert.AreEqual(2, result.Reconciliation.Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode).Transactions.Count());
    }

    [TestMethod]
    public void Reconcile_WithStatementShouldHave3PowerTransactions_GivenTestData1()
    {
        TestIntialise(1);
        var result = ActPeriodEndReconciliation();
        Assert.AreEqual(3, result.Reconciliation.Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PowerBucketCode).Transactions.Count());
    }

    [TestMethod]
    public void Reconcile_WithStatementShouldHaveSurplus1613_GivenTestData1()
    {
        TestIntialise(1);
        var result = ActPeriodEndReconciliation(bankBalances: new[] { new BankBalance(StatementModelTestData.ChequeAccount, 1850.5M) });
        Assert.AreEqual(1555.50M, result.Reconciliation.CalculatedSurplus);
    }

    [TestMethod]
    public void Reconcile_WithStatementSpentMonthlyLedgerShouldSupplementShortfall_GivenTestData1()
    {
        TestIntialise(1);
        var result = ActPeriodEndReconciliation();
        Assert.AreEqual(0M, result.Reconciliation.Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PhoneBucketCode).Balance);
    }

    [TestMethod]
    public void Reconcile_WithStatementWithBalanceAdjustment599ShouldHaveSurplus1014_GivenTestData1()
    {
        TestIntialise(1);
        var result = ActPeriodEndReconciliation(bankBalances: new[] { new BankBalance(StatementModelTestData.ChequeAccount, 1850.5M) });
        result.Reconciliation.BalanceAdjustment(-599M, "Visa pmt not yet in statement", new ChequeAccount("Chq"));
        Assert.AreEqual(956.50M, result.Reconciliation.CalculatedSurplus);
    }

    [TestInitialize]
    public void TestIntialise()
    {
        TestIntialise(5);
    }

    private ReconciliationResult ActPeriodEndReconciliation(DateTime? reconciliationDate = null, IEnumerable<BankBalance> bankBalances = null, bool ignoreWarnings = false)
    {
        this.currentBankBalances = bankBalances ?? TestDataBankBalances;

        Debug.WriteLine("********************** BEFORE RUNNING RECONCILIATION *******************************");
        this.subject.LedgerBook.Output(true);

        var result = this.subject.CreateNewMonthlyReconciliation(reconciliationDate ?? TestDataReconcileDate,
                                                                 this.testDataBudgetContext.Model,
                                                                 this.testDataStatement,
                                                                 this.currentBankBalances.ToArray());

        Debug.WriteLine("********************** AFTER RUNNING RECONCILIATION *******************************");
        result.Reconciliation.Output(LedgerBookHelper.LedgerOrder(this.subject.LedgerBook), true, true);

        return result;
    }

    private ReconciliationResult ActPeriodEndReconciliationOnTestData5(StatementModel statementModelTestData = null, bool ignoreWarnings = false)
    {
        var budgetCollection = BudgetModelTestData.CreateCollectionWith5();
        this.testDataBudgetContext = new BudgetCurrencyContext(budgetCollection, budgetCollection.CurrentActiveBudget);
        this.testDataStatement = statementModelTestData ?? StatementModelTestData.TestData5();

        var result = ActPeriodEndReconciliation(bankBalances: new[]
                                                {
                                                    new BankBalance(StatementModelTestData.ChequeAccount, 1850.5M),
                                                    new BankBalance(StatementModelTestData.SavingsAccount, 1200M)
                                                },
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
                this.testDataStatement = new StatementModelBuilder()
                    .TestData1()
                    .Build();
                this.subject.LedgerBook = ledgerBook ?? LedgerBookTestData.TestData1();
                break;
            case 5:
                this.testDataBudgetContext = new BudgetCurrencyContext(budgetCollection, budgetCollection.CurrentActiveBudget);
                this.testDataStatement = new StatementModelBuilder()
                    .TestData5()
                    .AppendTransaction(new Transaction
                    {
                        Account = StatementModelTestData.ChequeAccount,
                        Amount = -23.56M,
                        BudgetBucket = StatementModelTestData.RegoBucket,
                        Date = TestDataReconcileDate.Date.AddDays(-1),
                        TransactionType = new NamedTransaction("Foo"),
                        Description = "Last transaction"
                    })
                    .Build();
                this.subject.LedgerBook = ledgerBook ?? LedgerBookTestData.TestData5(() => new LedgerBookTestHarness());
                break;
        }
    }
}
