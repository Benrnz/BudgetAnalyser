using System.Text;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Reconciliation;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Services;

[AutoRegisterWithIoC(SingleInstance = true)]
[UsedImplicitly] // Used by IoC
internal class ReconciliationService(IReconciliationCreationManager reconciliationManager) : IReconciliationService, ISupportsModelPersistence
{
    private readonly IReconciliationCreationManager reconciliationManager = reconciliationManager ?? throw new ArgumentNullException(nameof(reconciliationManager));

    /// <inheritdoc />
    public ToDoCollection ReconciliationToDoList { get; private set; } = new();

    /// <inheritdoc />
    public void BeforeReconciliationValidation(LedgerBook book, StatementModel model)
    {
        this.reconciliationManager.ValidateAgainstOrphanedAutoMatchingTransactions(book, model);
    }

    /// <inheritdoc />
    public void CancelBalanceAdjustment(LedgerEntryLine entryLine, Guid transactionId)
    {
        if (entryLine is null)
        {
            throw new ArgumentNullException(nameof(entryLine));
        }

        entryLine.CancelBalanceAdjustment(transactionId);
    }

    /// <inheritdoc />
    public LedgerTransaction CreateBalanceAdjustment(LedgerEntryLine entryLine, decimal amount, string narrative, Account account)
    {
        if (entryLine is null)
        {
            throw new ArgumentNullException(nameof(entryLine));
        }

        if (narrative is null)
        {
            throw new ArgumentNullException(nameof(narrative));
        }

        if (account is null)
        {
            throw new ArgumentNullException(nameof(account));
        }

        var adjustmentTransaction = entryLine.BalanceAdjustment(amount, narrative, account);
        adjustmentTransaction.Date = entryLine.Date;
        return adjustmentTransaction;
    }

    /// <inheritdoc />
    public LedgerTransaction CreateLedgerTransaction(LedgerBook ledgerBook, LedgerEntryLine reconciliation, LedgerEntry ledgerEntry, decimal amount, string narrative)
    {
        if (reconciliation is null)
        {
            throw new ArgumentNullException(nameof(reconciliation));
        }

        if (ledgerEntry is null)
        {
            throw new ArgumentNullException(nameof(ledgerEntry));
        }

        if (narrative is null)
        {
            throw new ArgumentNullException(nameof(narrative));
        }

        LedgerTransaction newTransaction = new CreditLedgerTransaction();
        newTransaction.WithAmount(amount).WithNarrative(narrative);
        newTransaction.Date = reconciliation.Date;

        // ledgerEntry.AddTransactionForPersistenceOnly(newTransaction);
        var replacementTxns = ledgerEntry.Transactions.ToList();
        replacementTxns.Add(newTransaction);
        ledgerEntry.SetTransactionsForReconciliation(replacementTxns);
        ledgerEntry.RecalculateClosingBalance(ledgerBook);
        return newTransaction;
    }

    /// <inheritdoc />
    public LedgerEntryLine PeriodEndReconciliation(LedgerBook ledgerBook,
        DateOnly reconciliationDate,
        BudgetCollection budgetCollection,
        StatementModel statement,
        bool ignoreWarnings,
        params BankBalance[] balances)
    {
        var reconResult = this.reconciliationManager.PeriodEndReconciliation(ledgerBook, reconciliationDate, budgetCollection, statement, ignoreWarnings, balances);
        ReconciliationToDoList.Clear();
        reconResult.Tasks.ToList().ForEach(ReconciliationToDoList.Add);
        return reconResult.Reconciliation;
    }

    /// <inheritdoc />
    public void RemoveTransaction(LedgerBook ledgerBook, LedgerEntry ledgerEntry, Guid transactionId)
    {
        if (ledgerBook is null)
        {
            throw new ArgumentNullException(nameof(ledgerBook));
        }

        if (ledgerEntry is null)
        {
            throw new ArgumentNullException(nameof(ledgerEntry));
        }

        ledgerEntry.RemoveTransaction(transactionId);

        ledgerEntry.RecalculateClosingBalance(ledgerBook);
    }

    /// <inheritdoc />
    public void TransferFunds(LedgerBook ledgerBook, LedgerEntryLine reconciliation, TransferFundsCommand transferDetails)
    {
        if (reconciliation is null)
        {
            throw new ArgumentNullException(nameof(reconciliation), "There are no reconciliations. Transfer funds can only be used on the most recent reconciliation.");
        }

        this.reconciliationManager.TransferFunds(ledgerBook, transferDetails, reconciliation);
    }

    /// <inheritdoc />
    public LedgerEntryLine? UnlockCurrentPeriod(LedgerBook ledgerBook)
    {
        return ledgerBook is null ? throw new ArgumentNullException(nameof(ledgerBook)) : ledgerBook.UnlockMostRecentLine();
    }

    /// <inheritdoc />
    public void UpdateRemarks(LedgerEntryLine entryLine, string remarks)
    {
        if (entryLine is null)
        {
            throw new ArgumentNullException(nameof(entryLine));
        }

        entryLine.UpdateRemarks(remarks);
    }

    /// <inheritdoc />
    public ApplicationDataType DataType => ApplicationDataType.Ledger;

    /// <inheritdoc />
    public int LoadSequence => 51;

    /// <inheritdoc />
    public void Close()
    {
        ReconciliationToDoList = new ToDoCollection();
    }

    /// <inheritdoc />
    public Task CreateNewAsync(ApplicationDatabase applicationDatabase)
    {
        // Nothing needs to be done here.
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task LoadAsync(ApplicationDatabase applicationDatabase)
    {
        if (applicationDatabase is null)
        {
            throw new ArgumentNullException(nameof(applicationDatabase));
        }

        // The To Do Collection persistence is managed by the ApplicationDatabaseService.
        ReconciliationToDoList = applicationDatabase.LedgerReconciliationToDoCollection;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SaveAsync(ApplicationDatabase applicationDatabase)
    {
        // Nothing needs to be done here.
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void SavePreview()
    {
    }

    /// <inheritdoc />
    public bool ValidateModel(StringBuilder messages)
    {
        return true;
    }
}
