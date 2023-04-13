using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Reconciliation;
using BudgetAnalyser.Engine.UnitTest.Helper;
using BudgetAnalyser.Engine.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Ledger;

// ReSharper disable InconsistentNaming
[TestClass]
public class LedgerBook_ReconcileTest
{
    private static readonly IEnumerable<BankBalance> NextReconcileBankBalance = new[] { new BankBalance(StatementModelTestData.ChequeAccount, 2050M) };
    private static readonly DateTime ReconcileDate = new DateTime(2013, 09, 15);

    private LedgerBook subject;
    private ReconciliationResult testDataReconResult;

    [TestMethod]
    public void AddLedger_ShouldAddToLedgersCollection_GivenTestData1()
    {
        this.subject.AddLedger(LedgerBookTestData.RatesLedger);

        Assert.IsTrue(this.subject.Ledgers.Any(l => l.BudgetBucket == LedgerBookTestData.RatesLedger.BudgetBucket));
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
    public void Reconcile_Output_GivenTestData1()
    {
        Act();
        this.subject.Output();
    }

    [TestMethod]
    public void Reconcile_ShouldInsertLastestInFront_GivenTestData1()
    {
        Act();
        Assert.AreEqual(this.testDataReconResult.Reconciliation, this.subject.Reconciliations.First());
    }


    [TestMethod]
    public void Reconcile_ShouldResultIn4Lines_GivenTestData1()
    {
        Act();
        Assert.AreEqual(4, this.subject.Reconciliations.Count());
    }

    [TestInitialize]
    public void TestInitialise()
    {
        this.subject = LedgerBookTestData.TestData1();
        this.testDataReconResult = new ReconciliationResult { Reconciliation = new LedgerEntryLine(ReconcileDate, NextReconcileBankBalance) };
    }

    private void Act()
    {
        this.subject.Reconcile(this.testDataReconResult);
    }
}
// ReSharper restore InconsistentNaming