﻿using System.Globalization;
using System.Text;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Ledger;

/// <summary>
///     A single entry on a <see cref="BudgetBucket" /> for a date (which comes from the <see cref="LedgerEntryLine" />). This instance can contain one or more <see cref="LedgerTransaction" />s
///     defining all movements for this <see cref="Budget.BudgetBucket" /> for this date. Possible transactions include budgeted 'saved up for expenses' credited into this
///     <see cref="BudgetBucket" /> and all statement transactions that are debitted to this budget bucket ledger.
/// </summary>
public class LedgerEntry
{
    /// <summary>
    ///     A variable to keep track if this is a newly created entry for a new reconciliation as opposed to creation from loading from file.  An entry can be 'unlocked', this allows editing of
    ///     this entry after it has been saved and reloaded. Only the most recent entries for a <see cref="LedgerEntryLine" /> can be unlocked. This variable is intentionally not persisted.
    /// </summary>
    private bool isNew;

    private List<LedgerTransaction> transactions;

    /// <summary>
    ///     Used only by persistence.
    /// </summary>
    internal LedgerEntry()
    {
        this.transactions = new List<LedgerTransaction>();
    }

    /// <summary>
    ///     Used when adding a new entry for a new reconciliation.
    /// </summary>
    internal LedgerEntry(bool isNew) : this()
    {
        this.isNew = isNew;
    }

    /// <summary>
    ///     Constructor for persistence purposes only. Do not use in application code.
    /// </summary>
    internal LedgerEntry(IEnumerable<LedgerTransaction> transactions) : this(false)
    {
        if (transactions is null)
        {
            throw new ArgumentNullException(nameof(transactions));
        }

        foreach (var newTransaction in transactions)
        {
            this.transactions.Add(newTransaction);
            var newBalance = Balance + newTransaction.Amount;
            Balance = newBalance > 0 ? newBalance : 0;
        }
    }

    /// <summary>
    ///     The balance of the ledger as at the date after the transactions are applied in the parent <see cref="LedgerEntryLine" />.
    /// </summary>
    public decimal Balance { get; internal set; }

    /// <summary>
    ///     The Ledger Column instance that tracks which <see cref="BudgetBucket" /> is being tracked by this Ledger. This will also designate which Bank Account the ledger funds are stored.
    ///     Note that this may be different to the master mapping in <see cref="LedgerBook.Ledgers" />. This is because this instance shows which account stored the funds at the date in the
    ///     parent <see cref="LedgerEntryLine" />.
    /// </summary>
    public required LedgerBucket LedgerBucket { get; init; }

    /// <summary>
    ///     The total net affect of all transactions in this entry.  Debits will be negative.
    /// </summary>
    public decimal NetAmount => this.transactions.Sum(t => t.Amount);

    /// <summary>
    ///     Gets the transactions collection for this entry.
    /// </summary>
    public IEnumerable<LedgerTransaction> Transactions => this.transactions;

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>
    ///     A string that represents the current object.
    /// </returns>
    public override string ToString()
    {
        return string.Format(CultureInfo.CurrentCulture, "Ledger Entry - {0}", LedgerBucket);
    }

    internal void Lock()
    {
        this.isNew = false;
    }

    internal void RecalculateClosingBalance(LedgerBook ledgerBook)
    {
        // Recalc balance based on opening balance and transactions.
        var previousLine = ledgerBook.Reconciliations.Skip(1).FirstOrDefault();
        var openingBalance = LedgerEntryLine.FindPreviousEntryClosingBalance(previousLine, LedgerBucket);
        RecalculateClosingBalance(openingBalance);
    }

    internal void RecalculateClosingBalance(decimal openingBalance)
    {
        // Recalc balance based on opening balance and transactions.
        Balance = openingBalance + Transactions.Sum(t => t.Amount);
    }

    internal void RemoveTransaction(Guid transactionId)
    {
        if (!this.isNew)
        {
            throw new InvalidOperationException("Cannot adjust existing ledger lines, only newly added lines can be adjusted.");
        }

        var txn = this.transactions.FirstOrDefault(t => t.Id == transactionId);
        if (txn is not null)
        {
            this.transactions.Remove(txn);
            // Can't just simply adjust Balance here by subtract the txn amount from the balance. Must recalc based on opening balance and txns.
            // To do that the balance needs to be recalced at the Ledger Line Level.
        }
    }

    /// <summary>
    ///     Called by <see cref="LedgerBook.Reconcile" />. Sets up this new Entry with transactions.
    /// </summary>
    /// <param name="newTransactions">The list of new transactions for this entry. This includes the period budgeted amount.</param>
    internal void SetTransactionsForReconciliation(List<LedgerTransaction> newTransactions)
    {
        this.transactions = newTransactions.OrderBy(t => t.Date).ToList();
    }

    internal void Unlock()
    {
        this.isNew = true;
    }

    internal bool Validate(StringBuilder validationMessages, decimal openingBalance)
    {
        var result = true;
        if (openingBalance + Transactions.Sum(t => t.Amount) != Balance)
        {
            validationMessages.AppendFormat(CultureInfo.CurrentCulture, "Ledger Entry '{0}' transactions do not add up to the calculated balance!", this);
            result = false;
        }

        return result;
    }
}
