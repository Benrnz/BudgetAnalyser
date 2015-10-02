using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Ledger
{
    [AutoRegisterWithIoC]
    internal class ReconciliationManager : IReconciliationManager
    {
        private readonly ILogger logger;
        private readonly IReconciliationConsistency reconciliationConsistency;
        private readonly ITransactionRuleService transactionRuleService;

        public ReconciliationManager([NotNull] ITransactionRuleService transactionRuleService, [NotNull] IReconciliationConsistency reconciliationConsistency, [NotNull] ILogger logger)
        {
            if (transactionRuleService == null)
            {
                throw new ArgumentNullException(nameof(transactionRuleService));
            }

            if (reconciliationConsistency == null)
            {
                throw new ArgumentNullException(nameof(reconciliationConsistency));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.transactionRuleService = transactionRuleService;
            this.reconciliationConsistency = reconciliationConsistency;
            this.logger = logger;
        }

        public ReconciliationResult MonthEndReconciliation(
            LedgerBook ledgerBook,
            DateTime reconciliationDate,
            IEnumerable<BankBalance> currentBankBalances,
            IBudgetCurrencyContext budgetContext,
            StatementModel statement,
            bool ignoreWarnings = false)
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

            Stopwatch stopWatch = Stopwatch.StartNew();
            this.logger.LogInfo(l => l.Format("Starting Ledger Book reconciliation {0}", DateTime.Now));

            try
            {
                PreReconciliationValidation(ledgerBook, reconciliationDate, statement);
            }
            catch (ValidationWarningException)
            {
                if (!ignoreWarnings)
                {
                    throw;
                }
            }

            ReconciliationResult recon;
            using (this.reconciliationConsistency.EnsureConsistency(ledgerBook))
            {
                recon = ledgerBook.Reconcile(reconciliationDate, currentBankBalances, budgetContext.Model, statement);
            }

            // Create new single use matching rules - if needed to ensure transfers are assigned a bucket easily without user intervention.
            foreach (ToDoTask task in recon.Tasks)
            {
                this.logger.LogInfo(l => l.Format("TASK: {0} SystemGenerated:{1}", task.Description, task.SystemGenerated));
                var transferTask = task as TransferTask;
                if (transferTask != null && transferTask.SystemGenerated && transferTask.Reference.IsSomething())
                {
                    this.logger.LogInfo(
                        l => l.Format("TRANSFER-TASK detected- creating new single use rule. SystemGenerated:{1} Reference:{2}", task.Description, task.SystemGenerated, transferTask.Reference));
                    this.transactionRuleService.CreateNewSingleUseRule(transferTask.BucketCode, null, new[] { transferTask.Reference }, null, null, true);
                }
            }

            stopWatch.Stop();
            this.logger.LogInfo(l => l.Format("Finished Ledger Book reconciliation {0}. It took {1:F0}ms", DateTime.Now, stopWatch.ElapsedMilliseconds));
            return recon;
        }

        /// <summary>
        ///     Performs a funds transfer for the given ledger entry line.
        /// </summary>
        public void TransferFunds(TransferFundsCommand transferDetails, LedgerEntryLine ledgerEntryLine)
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

            PerformBankTransfer(transferDetails, ledgerEntryLine);
        }

        private static void PerformBankTransfer(TransferFundsCommand transferDetails, LedgerEntryLine ledgerEntryLine)
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
                ledgerEntryLine.BalanceAdjustment(-transferDetails.TransferAmount, transferDetails.Narrative, transferDetails.FromLedger.StoredInAccount);
                ledgerEntryLine.BalanceAdjustment(transferDetails.TransferAmount, transferDetails.Narrative, transferDetails.ToLedger.StoredInAccount);
            }

            // No need for a source transaction, but need a Balance Adjustment when Bank Transfer is required.
            if (!(transferDetails.FromLedger.BudgetBucket is SurplusBucket))
            {
                LedgerEntry ledgerEntry = ledgerEntryLine.Entries.Single(e => e.LedgerBucket == transferDetails.FromLedger);
                ledgerEntry.AddTransaction(sourceTransaction);
            }

            // No need for a destination transaction, but need a Balance Adjustment when Bank Transfer is required.
            if (!(transferDetails.ToLedger.BudgetBucket is SurplusBucket))
            {
                LedgerEntry ledgerEntry = ledgerEntryLine.Entries.Single(e => e.LedgerBucket == transferDetails.ToLedger);
                ledgerEntry.AddTransaction(destinationTransaction);
            }
        }

        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private static void ValidateDates(LedgerBook ledgerBook, DateTime startDate, DateTime reconciliationDate, StatementModel statement)
        {
            LedgerEntryLine recentEntry = ledgerBook.Reconciliations.FirstOrDefault();
            if (recentEntry != null)
            {
                if (reconciliationDate <= recentEntry.Date)
                {
                    throw new InvalidOperationException("The start Date entered is before the previous ledger entry.");
                }

                if (recentEntry.Date.AddDays(7 * 4) > reconciliationDate)
                {
                    throw new InvalidOperationException("The start Date entered is not at least 4 weeks after the previous reconciliation. ");
                }

                if (recentEntry.Date.Day != reconciliationDate.Day)
                {
                    throw new ValidationWarningException(
                        "The reconciliation Date chosen, {0}, isn't the same day of the month as the previous entry {1}. Not required, but ideally reconciliations should be evenly spaced.");
                }
            }

            if (!statement.AllTransactions.Any(t => t.Date >= startDate))
            {
                throw new ValidationWarningException("There doesn't appear to be any transactions in the statement for the month up to " + reconciliationDate.ToShortDateString());
            }
        }

        private void PreReconciliationValidation(LedgerBook ledgerBook, DateTime reconciliationDate, StatementModel statement)
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

            DateTime startDate = ReconciliationBuilder.CalculateDateForReconcile(ledgerBook, reconciliationDate);

            ValidateDates(ledgerBook, startDate, reconciliationDate, statement);

            ValidateAgainstUncategorisedTransactions(startDate, reconciliationDate, statement);

            ValidateAgainstOrphanedAutoMatchingTransactions(ledgerBook, statement);
        }

        private void ValidateAgainstOrphanedAutoMatchingTransactions(LedgerBook ledgerBook, StatementModel statement)
        {
            LedgerEntryLine lastLine = ledgerBook.Reconciliations.FirstOrDefault();
            if (lastLine == null)
            {
                return;
            }

            List<LedgerTransaction> unmatchedTxns = lastLine.Entries
                .SelectMany(e => e.Transactions)
                .Where(t => !string.IsNullOrWhiteSpace(t.AutoMatchingReference) && !t.AutoMatchingReference.StartsWith(ReconciliationBuilder.MatchedPrefix, StringComparison.Ordinal))
                .ToList();

            if (unmatchedTxns.None())
            {
                return;
            }

            List<Transaction> statementSubSet = statement.AllTransactions.Where(t => t.Date >= lastLine.Date).ToList();
            foreach (LedgerTransaction ledgerTransaction in unmatchedTxns)
            {
                IEnumerable<Transaction> statementTxns = ReconciliationBuilder.TransactionsToAutoMatch(statementSubSet, ledgerTransaction.AutoMatchingReference);
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
                            ledgerTransaction.Amount));
                }
            }
        }

        private void ValidateAgainstUncategorisedTransactions(DateTime startDate, DateTime reconciliationDate, StatementModel statement)
        {
            if (statement.AllTransactions
                .Where(t => t.Date >= startDate && t.Date < reconciliationDate)
                .Any(t => t.BudgetBucket == null || (t.BudgetBucket != null && string.IsNullOrWhiteSpace(t.BudgetBucket.Code))))
            {
                IEnumerable<Transaction> uncategorised = statement.AllTransactions.Where(t => t.BudgetBucket == null || (t.BudgetBucket != null && string.IsNullOrWhiteSpace(t.BudgetBucket.Code)));
                var count = 0;
                this.logger.LogWarning(_ => "LedgerBook.PreReconciliationValidation: There appears to be transactions in the statement that are not categorised into a budget bucket.");
                foreach (Transaction transaction in uncategorised)
                {
                    count++;
                    Transaction transactionCopy = transaction;
                    this.logger.LogWarning(_ => "LedgerBook.PreReconciliationValidation: Transaction: " + transactionCopy.Id + transactionCopy.BudgetBucket);
                    if (count > 5)
                    {
                        this.logger.LogWarning(_ => "LedgerBook.PreReconciliationValidation: There are more than 5 transactions.");
                    }
                }

                throw new ValidationWarningException("There appears to be transactions in the statement that are not categorised into a budget bucket.");
            }
        }
    }
}