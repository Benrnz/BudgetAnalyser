#nullable disable
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Reconciliation;
using BudgetAnalyser.Engine.XUnit.Helpers;
using BudgetAnalyser.Engine.XUnit.TestData;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Ledger;

// ReSharper disable InconsistentNaming
public class LedgerBook_ReconcileTest
{
    private static readonly IEnumerable<BankBalance> NextReconcileBankBalance = new[] { new BankBalance(TransactionsListModelTestData.ChequeAccount, 2050M) };
    private static readonly DateOnly ReconcileDate = new(2013, 09, 15);

    private readonly LedgerBook subject;
    private readonly ReconciliationResult testDataReconResult;

    public LedgerBook_ReconcileTest()
    {
        this.subject = LedgerBookTestData.TestData1();
        this.testDataReconResult = new ReconciliationResult { Reconciliation = new LedgerEntryLine(ReconcileDate, NextReconcileBankBalance), Tasks = Array.Empty<ToDoTask>() };
    }

    [Fact]
    public void AddLedger_ShouldAddToLedgersCollection_GivenTestData1()
    {
        this.subject.AddLedger(LedgerBookTestData.RatesLedger);

        this.subject.Ledgers.Any(l => l.BudgetBucket == LedgerBookTestData.RatesLedger.BudgetBucket).ShouldBeTrue();
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
    public void Reconcile_Output_GivenTestData1()
    {
        Act();
        this.subject.Output();
    }

    [Fact]
    public void Reconcile_ShouldInsertLastestInFront_GivenTestData1()
    {
        Act();
        this.subject.Reconciliations.First().ShouldBe(this.testDataReconResult.Reconciliation);
    }


    [Fact]
    public void Reconcile_ShouldResultIn4Lines_GivenTestData1()
    {
        Act();
        this.subject.Reconciliations.Count().ShouldBe(4);
    }

    private void Act()
    {
        this.subject.Reconcile(this.testDataReconResult);
    }
}
// ReSharper restore InconsistentNaming
