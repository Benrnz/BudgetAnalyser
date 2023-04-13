using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Ledger;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Ledger;

[TestClass]
public class LedgerEntryTest
{
    private LedgerEntry subject;
    
    [TestInitialize]
    public void TestInitialise()
    {
        this.subject = new LedgerEntry(true) { Balance = 120 };
        this.subject.SetTransactionsForReconciliation(new List<LedgerTransaction>
        {
            new BudgetCreditLedgerTransaction { Amount = 200 },
            new CreditLedgerTransaction { Amount = -50 },
            new CreditLedgerTransaction { Amount = -30 }
        });
    }
    
    [TestMethod]
    public void AddTransactionShouldEffectEntryBalance()
    {
        var newTransaction = new CreditLedgerTransaction { Amount = -100 };
        this.subject.AddTransactionForPersistenceOnly(newTransaction);
            
        Assert.AreEqual(20M, this.subject.Balance);
    }

    [TestMethod]
    public void AddTransactionShouldEffectEntryNetAmount()
    {
        var newTransaction = new CreditLedgerTransaction { Amount = -100 };
        this.subject.AddTransactionForPersistenceOnly(newTransaction);
        
        Assert.AreEqual(20M, this.subject.NetAmount);
    }

    [TestMethod]
    public void RemoveTransactionShouldNotEffectEntryBalance()
    {
        this.subject.RemoveTransaction(this.subject.Transactions.First(t => t is CreditLedgerTransaction).Id);
        
        // The balance cannot be simply set inside the Ledger Line, it must be recalc'ed at the ledger book level.
        Assert.AreEqual(120M, this.subject.Balance);
    }
    
    [TestMethod]
    public void RemoveTransactionShouldEffectEntryNetAmount()
    {
        var txn = this.subject.Transactions.First(t => t is CreditLedgerTransaction);
        this.subject.RemoveTransaction(txn.Id);
        
        Assert.AreEqual(170M, this.subject.NetAmount);
    }
}