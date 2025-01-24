using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Services;

/// <summary>
///     A service to control the reconciliation process from the UI. A reconciliation is a process of closing off a period of time and importing statement transactions into the ledger book.
///     This service primarily manages the creation of <see cref="LedgerEntryLine" /> and adding it to a <see cref="LedgerBook" />.
/// </summary>
public interface IReconciliationService
{
    /// <summary>
    ///     Gets the user reminder task list for reconciliation.
    /// </summary>
    ToDoCollection ReconciliationToDoList { get; }

    /// <summary>
    ///     An optional validation method the UI can call before invoking <see cref="PeriodEndReconciliation" /> to test for validation warnings. If validation fails a new
    ///     <see cref="ValidationWarningException" /> is thrown; otherwise the method returns.
    /// </summary>
    void BeforeReconciliationValidation(LedgerBook book, StatementModel model);

    /// <summary>
    ///     Cancels an existing balance adjustment transaction that already exists in the Ledger Entry Line.
    /// </summary>
    void CancelBalanceAdjustment(LedgerEntryLine entryLine, Guid transactionId);

    /// <summary>
    ///     Creates a new balance adjustment transaction for the given entry line.
    /// </summary>
    LedgerTransaction CreateBalanceAdjustment(LedgerEntryLine entryLine, decimal amount, string narrative, Account account);

    /// <summary>
    ///     Creates a new ledger transaction in the given Ledger. The Ledger Entry must exist in the current Ledger Book.
    /// </summary>
    LedgerTransaction CreateLedgerTransaction(LedgerBook ledgerBook, LedgerEntryLine reconciliation, LedgerEntry ledgerEntry, decimal amount, string narrative);

    /// <summary>
    ///     Creates a new LedgerEntryLine for the specified <see cref="LedgerBook" /> to begin reconciliation.
    /// </summary>
    /// <param name="ledgerBook">The ledger book to which this new reconciliation applies.</param>
    /// <param name="reconciliationDate">
    ///     The startDate for the <see cref="LedgerEntryLine" />. This is usually the previous Month's "Reconciliation-Date", as this month's reconciliation starts with this date and includes
    ///     transactions from that date. This date is different to the "Reconciliation-Date" that appears next to the resulting reconciliation which is the end date for the period.
    /// </param>
    /// <param name="budgetCollection">The collection of budgets. The Reconciliation engine classes will make a decision which budget to choose.</param>
    /// <param name="statement">The currently loaded statement. Global filter will not be used to select transactions from the statement. Selection is made based on<paramref name="reconciliationDate" />.</param>
    /// <param name="ignoreWarnings">Ignores validation warnings if true, otherwise <see cref="ValidationWarningException" />.</param>
    /// <param name="balances">The bank balances as at the <paramref name="reconciliationDate" /> to include in this new single line of the ledger book.</param>
    /// <exception cref="InvalidOperationException">Thrown when this <see cref="LedgerBook" /> is in an invalid state.</exception>
    LedgerEntryLine PeriodEndReconciliation(LedgerBook ledgerBook,
        DateTime reconciliationDate,
        BudgetCollection budgetCollection,
        StatementModel statement,
        bool ignoreWarnings,
        params BankBalance[] balances);

    /// <summary>
    ///     Removes the transaction from the specified Ledger Entry.
    /// </summary>
    void RemoveTransaction(LedgerBook ledgerBook, LedgerEntry ledgerEntry, Guid transactionId);

    /// <summary>
    ///     Transfer funds from one ledger bucket to another. This is only possible if the current ledger reconciliation is unlocked. This is usually used during reconciliation.
    /// </summary>
    /// <param name="ledgerBook">The parent ledger book.</param>
    /// <param name="reconciliation">The reconciliation line that this transfer will be created in.  A transfer can only occur between two ledgers in the same reconciliation.</param>
    /// <param name="transferDetails">The details of the requested transfer.</param>
    void TransferFunds(LedgerBook ledgerBook, LedgerEntryLine reconciliation, TransferFundsCommand transferDetails);

    /// <summary>
    ///     Unlocks the current month after it has been saved and locked.
    /// </summary>
    LedgerEntryLine UnlockCurrentPeriod(LedgerBook ledgerBook);

    /// <summary>
    ///     Updates the remarks for the given Ledger Entry Line.
    /// </summary>
    void UpdateRemarks(LedgerEntryLine entryLine, string remarks);
}
