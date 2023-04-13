using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Ledger.Reconciliation;

/// <inheritdoc cref="IReconciliationCreationManager" />
[AutoRegisterWithIoC]
internal class ReconciliationCreationManager : IReconciliationCreationManager
{
    private readonly IReconciliationBuilder builder;
    private readonly ILogger logger;
    private readonly IReconciliationConsistency reconciliationConsistency;
    private readonly ITransactionRuleService transactionRuleService;
    private ICollection<string> validationMessages = new Collection<string>();

    public ReconciliationCreationManager([NotNull] ITransactionRuleService transactionRuleService,
                                         [NotNull] IReconciliationConsistency reconciliationConsistency,
                                         [NotNull] IReconciliationBuilder builder,
                                         [NotNull] ILogger logger)
    {
        this.transactionRuleService = transactionRuleService ?? throw new ArgumentNullException(nameof(transactionRuleService));
        this.reconciliationConsistency = reconciliationConsistency ?? throw new ArgumentNullException(nameof(reconciliationConsistency));
        this.builder = builder ?? throw new ArgumentNullException(nameof(builder));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc cref="IReconciliationCreationManager.PeriodEndReconciliation" />
    public ReconciliationResult PeriodEndReconciliation(
        LedgerBook ledgerBook,
        DateTime closingDateExclusive,
        IBudgetCurrencyContext budgetContext,
        StatementModel statement,
        bool ignoreWarnings,
        params BankBalance[] currentBankBalances)
    {
        if (ledgerBook == null)
        {
            throw new ArgumentNullException(nameof(ledgerBook));
        }

        if (currentBankBalances == null)
        {
            throw new ArgumentNullException(nameof(currentBankBalances));
        }

        if (budgetContext == null)
        {
            throw new ArgumentNullException(nameof(budgetContext));
        }

        if (statement == null)
        {
            throw new ArgumentNullException(nameof(statement));
        }

        if (!budgetContext.BudgetActive)
        {
            throw new ArgumentException("Reconciling against an inactive budget is invalid.");
        }

        var stopWatch = Stopwatch.StartNew();
        this.logger.LogInfo(l => l.Format("Starting Ledger Book reconciliation {0}", DateTime.Now));

        if (!ignoreWarnings)
        {
            this.validationMessages = new Collection<string>();
        }

        try
        {
            PreReconciliationValidation(ledgerBook, closingDateExclusive, statement, budgetContext.Model.BudgetCycle);
        }
        catch (ValidationWarningException ex)
        {
            if (ShouldValidationExceptionBeRethrown(ignoreWarnings, ex))
            {
                throw;
            }
        }

        ReconciliationResult recon;
        try
        {
            using (this.reconciliationConsistency.EnsureConsistency(ledgerBook))
            {
                this.builder.LedgerBook = ledgerBook;
                // BUG budgetContext.Model may not be the right budget to use fot the closingDateExclusive. Use BudgetCollection instead or pass it in.
                // TODO Should really call budgetContext.BudgetActive to make sure before starting reconciliation?
                // TODO The reconciliation date is probably the wrong date to use to find the applicable budget. Recon date is the day after the close off date of the period.
                recon = this.builder.CreateNewMonthlyReconciliation(closingDateExclusive, budgetContext.Model, statement, currentBankBalances);
                ledgerBook.Reconcile(recon);
            }

            if (recon == null)
            {
                throw new NullReferenceException("Unexpected error: Reconciliation failed to create.");
            }
        }
        finally
        {
            this.builder.LedgerBook = null;
        }

        // Create new single use matching rules - if needed to ensure transfers are assigned a bucket easily without user intervention.
        foreach (var task in recon.Tasks)
        {
            this.logger.LogInfo(l => l.Format("TASK: {0} SystemGenerated:{1}", task.Description, task.SystemGenerated));
            if (task is TransferTask { SystemGenerated: true } transferTask && transferTask.Reference.IsSomething())
            {
                this.logger.LogInfo(l =>
                                        l.Format("TRANSFER-TASK detected- creating new single use rule. SystemGenerated:{1} Reference:{2}", task.Description, task.SystemGenerated,
                                                 transferTask.Reference));
                this.transactionRuleService.CreateNewSingleUseRule(transferTask.BucketCode, null, new[] { transferTask.Reference }, null, null, true);
            }
        }

        stopWatch.Stop();
        this.logger.LogInfo(l => l.Format("Finished Ledger Book reconciliation {0}. It took {1:F0}ms", DateTime.Now, stopWatch.ElapsedMilliseconds));
        this.validationMessages = null;
        return recon;
    }

    /// <summary>
    ///     Performs a funds transfer for the given ledger entry line.
    /// </summary>
    public void TransferFunds(LedgerBook ledgerBook, TransferFundsCommand transferDetails, LedgerEntryLine ledgerEntryLine)
    {
        if (transferDetails == null)
        {
            throw new ArgumentNullException(nameof(transferDetails));
        }

        if (ledgerEntryLine == null)
        {
            throw new ArgumentNullException(nameof(ledgerEntryLine));
        }

        if (!transferDetails.IsValid())
        {
            throw new InvalidOperationException("Code Error: The transfer command is in an invalid state, this should be resolved in the UI.");
        }

        PerformBankTransfer(ledgerBook, transferDetails, ledgerEntryLine);
    }

    /// <summary>
    ///     Examines the ledger book's most recent reconciliation looking for transactions waiting to be matched to
    ///     transactions imported in the current month.
    ///     If any transactions are found, the statement is then examined to see if the transactions appear, if they do not a
    ///     new <see cref="ValidationWarningException" />
    ///     is thrown; otherwise the method returns.
    /// </summary>
    public void ValidateAgainstOrphanedAutoMatchingTransactions(LedgerBook ledgerBook, StatementModel statement)
    {
        if (ledgerBook == null)
        {
            throw new ArgumentNullException(nameof(ledgerBook));
        }

        if (statement == null)
        {
            throw new ArgumentNullException(nameof(statement));
        }

        var lastLine = ledgerBook.Reconciliations.FirstOrDefault();
        if (lastLine == null)
        {
            return;
        }

        List<LedgerTransaction> unmatchedTxns = lastLine.Entries
            .SelectMany(e => e.Transactions)
            .Where(
                   t =>
                       !string.IsNullOrWhiteSpace(t.AutoMatchingReference) &&
                       !t.AutoMatchingReference.StartsWith(ReconciliationBuilder.MatchedPrefix,
                                                           StringComparison.Ordinal))
            .ToList();

        if (unmatchedTxns.None())
        {
            return;
        }

        List<Transaction> statementSubSet = statement.AllTransactions.Where(t => t.Date >= lastLine.Date).ToList();
        foreach (var ledgerTransaction in unmatchedTxns)
        {
            IEnumerable<Transaction> statementTxns = ReconciliationBuilder.TransactionsToAutoMatch(statementSubSet,
                                                                                                   ledgerTransaction.AutoMatchingReference);
            if (statementTxns.None())
            {
                this.logger.LogWarning(
                                       l =>
                                           l.Format(
                                                    "There appears to be some transactions from last month that should be auto-matched to a statement transactions, but no matching statement transactions were found. {0}",
                                                    ledgerTransaction));
                throw new ValidationWarningException(
                                                     string.Format(
                                                                   CultureInfo.CurrentCulture,
                                                                   "There appears to be some transactions from last month that should be auto-matched to a statement transactions, but no matching statement transactions were found.\nHave you forgotten to do a transfer?\nTransaction ID:{0} Ref:{1} Amount:{2:C}",
                                                                   ledgerTransaction.Id,
                                                                   ledgerTransaction.AutoMatchingReference,
                                                                   ledgerTransaction.Amount))
                {
                    Source = "1"
                };
            }
        }
    }

    private void PerformBankTransfer(LedgerBook ledgerBook, TransferFundsCommand transferDetails, LedgerEntryLine ledgerEntryLine)
    {
        var sourceTransaction = new CreditLedgerTransaction
        {
            Amount = -transferDetails.TransferAmount,
            AutoMatchingReference = transferDetails.AutoMatchingReference,
            Date = ledgerEntryLine.Date,
            Narrative = transferDetails.Narrative
        };

        var destinationTransaction = new CreditLedgerTransaction
        {
            Amount = transferDetails.TransferAmount,
            AutoMatchingReference = transferDetails.AutoMatchingReference,
            Date = ledgerEntryLine.Date,
            Narrative = transferDetails.Narrative
        };

        if (transferDetails.BankTransferRequired)
        {
            ledgerEntryLine.BalanceAdjustment(-transferDetails.TransferAmount, transferDetails.Narrative,
                                              transferDetails.FromLedger.StoredInAccount);
            ledgerEntryLine.BalanceAdjustment(transferDetails.TransferAmount, transferDetails.Narrative,
                                              transferDetails.ToLedger.StoredInAccount);
            this.transactionRuleService.CreateNewSingleUseRule(
                                                               transferDetails.FromLedger.BudgetBucket.Code,
                                                               null,
                                                               new[] { transferDetails.AutoMatchingReference },
                                                               null,
                                                               -transferDetails.TransferAmount,
                                                               true);
            this.transactionRuleService.CreateNewSingleUseRule(
                                                               transferDetails.ToLedger.BudgetBucket.Code,
                                                               null,
                                                               new[] { transferDetails.AutoMatchingReference },
                                                               null,
                                                               transferDetails.TransferAmount,
                                                               true);
        }


        // No need for a source transaction for surplus ledger.
        if (!(transferDetails.FromLedger.BudgetBucket is SurplusBucket))
        {
            var ledgerEntry = ledgerEntryLine.Entries.Single(e => e.LedgerBucket == transferDetails.FromLedger);
            List<LedgerTransaction> replacementTxns = ledgerEntry.Transactions.ToList();
            replacementTxns.Add(sourceTransaction);
            ledgerEntry.SetTransactionsForReconciliation(replacementTxns);
            ledgerEntry.RecalculateClosingBalance(ledgerBook);
        }

        // No need for a destination transaction for surplus ledger.
        if (!(transferDetails.ToLedger.BudgetBucket is SurplusBucket))
        {
            var ledgerEntry = ledgerEntryLine.Entries.Single(e => e.LedgerBucket == transferDetails.ToLedger);
            List<LedgerTransaction> replacementTxns = ledgerEntry.Transactions.ToList();
            replacementTxns.Add(destinationTransaction);
            ledgerEntry.SetTransactionsForReconciliation(replacementTxns);
            ledgerEntry.RecalculateClosingBalance(ledgerBook);
        }
    }

    private void PreReconciliationValidation(LedgerBook ledgerBook, DateTime reconciliationDate, StatementModel statement, BudgetCycle periodType)
    {
        var messages = new StringBuilder();
        if (!ledgerBook.Validate(messages))
        {
            throw new InvalidOperationException("Ledger book is currently in an invalid state. Cannot add new entries.\n" + messages);
        }

        if (statement == null)
        {
            return;
        }

        var startDate = ReconciliationBuilder.CalculateBeginDateForReconciliationPeriod(ledgerBook, reconciliationDate, periodType);

        ValidateDates(ledgerBook, startDate, reconciliationDate, statement, periodType);

        ValidateAgainstUncategorisedTransactions(startDate, reconciliationDate, statement);

        ValidateAgainstOrphanedAutoMatchingTransactions(ledgerBook, statement);

        ValidateAgainstMissingTransactions(reconciliationDate, statement);
    }

    private bool ShouldValidationExceptionBeRethrown(bool ignoreWarnings, ValidationWarningException ex)
    {
        if (ignoreWarnings)
        {
            if (this.validationMessages.Contains(ex.Source))
            {
                return false;
            }

            return true;
        }

        this.validationMessages.Add(ex.Source);
        return true;
    }

    private static void ValidateAgainstMissingTransactions(DateTime reconciliationDate, StatementModel statement)
    {
        var lastTransactionDate = statement.Transactions.Where(t => t.Date < reconciliationDate).Max(t => t.Date);
        var difference = reconciliationDate.Subtract(lastTransactionDate);
        if (difference.TotalHours > 24)
        {
            throw new ValidationWarningException("There are no statement transactions in the last day or two, are you sure you have imported all this month's transactions?")
            {
                Source = "2"
            };
        }
    }

    private void ValidateAgainstUncategorisedTransactions(DateTime startDate, DateTime reconciliationDate, StatementModel statement)
    {
        if (statement.AllTransactions
            .Where(t => t.Date >= startDate && t.Date < reconciliationDate)
            .Any(
                 t =>
                     t.BudgetBucket == null ||
                     (t.BudgetBucket != null && string.IsNullOrWhiteSpace(t.BudgetBucket.Code))))
        {
            IEnumerable<Transaction> uncategorised =
                statement.AllTransactions.Where(
                                                t =>
                                                    t.BudgetBucket == null ||
                                                    (t.BudgetBucket != null && string.IsNullOrWhiteSpace(t.BudgetBucket.Code)));
            var count = 0;
            this.logger.LogWarning(
                                   _ =>
                                       "LedgerBook.PreReconciliationValidation: There appears to be transactions in the statement that are not categorised into a budget bucket.");
            foreach (var transaction in uncategorised)
            {
                count++;
                var transactionCopy = transaction;
                this.logger.LogWarning(
                                       _ =>
                                           "LedgerBook.PreReconciliationValidation: Transaction: " + transactionCopy.Id +
                                           transactionCopy.BudgetBucket);
                if (count > 5)
                {
                    this.logger.LogWarning(
                                           _ => "LedgerBook.PreReconciliationValidation: There are more than 5 transactions.");
                }
            }

            throw new ValidationWarningException("There appears to be transactions in the statement that are not categorised into a budget bucket.")
            {
                Source = "3"
            };
        }
    }

    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private static void ValidateDates(LedgerBook ledgerBook, DateTime startDate, DateTime reconciliationDate, StatementModel statement, BudgetCycle periodType)
    {
        var previousEntry = ledgerBook.Reconciliations.FirstOrDefault();
        if (previousEntry != null)
        {
            if (reconciliationDate <= previousEntry.Date)
            {
                throw new InvalidOperationException("The reconciliation date entered is before the previous ledger entry.");
            }

            switch (periodType)
            {
                case BudgetCycle.Monthly:
                    if (previousEntry.Date.AddDays(7 * 4) > reconciliationDate)
                    {
                        throw new InvalidOperationException("The reconciliation date entered is not at least 4 weeks after the previous reconciliation. ");
                    }

                    if (previousEntry.Date.Day != reconciliationDate.Day)
                    {
                        throw new ValidationWarningException($"The reconciliation date chosen, {reconciliationDate}, isn't the same day of the month as the previous entry {previousEntry.Date}. Not required, but ideally reconciliations should be evenly spaced.")
                        {
                            Source = "4"
                        };
                    }

                    break;
                
                case BudgetCycle.Fortnightly:
                    if (reconciliationDate.Subtract(previousEntry.Date).Days != 14)
                    {
                        throw new ValidationWarningException("The reconciliation date entered is not 2 weeks after the previous reconciliation. ") { Source = "6" };
                    }
                    break;
            }
        }

        if (!statement.AllTransactions.Any(t => t.Date >= startDate))
        {
            throw new ValidationWarningException("There doesn't appear to be any transactions in the statement for the month up to " + reconciliationDate.ToString("d"))
            {
                Source = "5"
            };
        }
    }
}