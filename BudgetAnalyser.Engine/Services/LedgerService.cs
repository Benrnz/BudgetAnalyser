using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Services
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class LedgerService : ILedgerService
    {
        private readonly IAccountTypeRepository accountTypeRepository;
        private readonly ILedgerBookRepository ledgerRepository;
        private LedgerBook book;

        public LedgerService([NotNull] ILedgerBookRepository ledgerRepository, [NotNull] IAccountTypeRepository accountTypeRepository)
        {
            if (ledgerRepository == null)
            {
                throw new ArgumentNullException("ledgerRepository");
            }

            if (accountTypeRepository == null)
            {
                throw new ArgumentNullException("accountTypeRepository");
            }

            this.ledgerRepository = ledgerRepository;
            this.accountTypeRepository = accountTypeRepository;
        }

        /// <summary>
        ///     Cancels an existing balance adjustment transaction that already exists in the Ledger Entry Line.
        /// </summary>
        public void CancelBalanceAdjustment(LedgerEntryLine entryLine, Guid transactionId)
        {
            if (entryLine == null)
            {
                throw new ArgumentNullException("entryLine");
            }

            if (this.book.Reconciliations.All(l => l != entryLine))
            {
                throw new ArgumentException("Ledger Entry Line provided does not exist in the current Ledger Book.", "entryLine");
            }

            entryLine.CancelBalanceAdjustment(transactionId);
        }

        /// <summary>
        ///     Creates a new balance adjustment transaction for the given entry line.  The entry line must exist in the current
        ///     Ledger Book.
        /// </summary>
        public LedgerTransaction CreateBalanceAdjustment(LedgerEntryLine entryLine, decimal amount, string narrative, AccountType account)
        {
            if (entryLine == null)
            {
                throw new ArgumentNullException("entryLine");
            }

            if (narrative == null)
            {
                throw new ArgumentNullException("narrative");
            }

            if (account == null)
            {
                throw new ArgumentNullException("account");
            }

            if (this.book.Reconciliations.All(l => l != entryLine))
            {
                throw new ArgumentException("Ledger Entry Line provided does not exist in the current Ledger Book.", "entryLine");
            }

            BankBalanceAdjustmentTransaction adjustmentTransaction = entryLine.BalanceAdjustment(amount, narrative).WithAccountType(account);
            adjustmentTransaction.Date = entryLine.Date;
            return adjustmentTransaction;
        }

        /// <summary>
        ///     Creates a new ledger transaction in the given Ledger. The Ledger Entry must exist in the current Ledger Book.
        /// </summary>
        public LedgerTransaction CreateLedgerTransaction(LedgerEntry ledgerEntry, decimal amount, string narrative)
        {
            if (ledgerEntry == null)
            {
                throw new ArgumentNullException("ledgerEntry");
            }

            if (narrative == null)
            {
                throw new ArgumentNullException("narrative");
            }

            if (this.book.Reconciliations.First().Entries.All(e => e != ledgerEntry))
            {
                throw new ArgumentException("Ledger Entry provided does not exist in the current Ledger Book.", "ledgerEntry");
            }

            LedgerTransaction newTransaction = new CreditLedgerTransaction();
            newTransaction.WithAmount(amount).WithNarrative(narrative);
            newTransaction.Date = this.book.Reconciliations.First().Date;
            ledgerEntry.AddTransaction(newTransaction);
            return newTransaction;
        }

        public LedgerBook CreateNew(string storageKey)
        {
            if (storageKey == null)
            {
                throw new ArgumentNullException("storageKey");
            }

            return this.ledgerRepository.CreateNew("New LedgerBook, give me a proper name :-(", storageKey);
        }

        public LedgerBook DisplayLedgerBook(string storageKey)
        {
            if (storageKey == null)
            {
                throw new ArgumentNullException("storageKey");
            }

            if (!this.ledgerRepository.Exists(storageKey))
            {
                throw new FileNotFoundException("The requested file, or the previously loaded file, cannot be located.\n" + storageKey, storageKey);
            }

            this.book = this.ledgerRepository.Load(storageKey);
            return this.book;
        }

        /// <summary>
        ///     Creates a new LedgerEntryLine for the specified <see cref="LedgerBook" /> to begin reconciliation.
        /// </summary>
        /// <param name="reconciliationDate">
        ///     The date for the <see cref="LedgerEntryLine" />. Also used to search for transactions in the
        ///     <see cref="statement" />. This date ideally is your payday of the month, and should be the same date
        ///     every month. Transactions are searched for up to but not including this date.
        /// </param>
        /// <param name="balances">
        ///     The bank balances as at the <see cref="reconciliationDate" /> to include in this new single line of the
        ///     ledger book.
        /// </param>
        /// <param name="budgetContext">The current budget context.</param>
        /// <param name="statement">The currently loaded statement.</param>
        /// <param name="ignoreWarnings">Ignores validation warnings if true, otherwise <see cref="ValidationWarningException" />.</param>
        /// <exception cref="System.ArgumentNullException">
        ///     balances or budgetContext or statement
        /// </exception>
        /// <exception cref="System.InvalidOperationException">Reconciling against an inactive budget is invalid.</exception>
        public LedgerEntryLine MonthEndReconciliation(
            DateTime reconciliationDate,
            IEnumerable<BankBalance> balances,
            IBudgetCurrencyContext budgetContext,
            StatementModel statement,
            bool ignoreWarnings = false)
        {
            if (balances == null)
            {
                throw new ArgumentNullException("balances");
            }
            if (budgetContext == null)
            {
                throw new ArgumentNullException("budgetContext");
            }
            if (statement == null)
            {
                throw new ArgumentNullException("statement");
            }

            if (!budgetContext.BudgetActive)
            {
                throw new InvalidOperationException("Reconciling against an inactive budget is invalid.");
            }

            return this.book.Reconcile(reconciliationDate, balances, budgetContext.Model, statement, ignoreWarnings);
        }

        public void MoveLedgerToAccount(LedgerBook ledgerBook, LedgerBucket ledger, AccountType storedInAccount)
        {
            if (ledgerBook == null)
            {
                throw new ArgumentNullException("ledgerBook");
            }
            if (ledger == null)
            {
                throw new ArgumentNullException("ledger");
            }
            if (storedInAccount == null)
            {
                throw new ArgumentNullException("storedInAccount");
            }

            ledgerBook.SetLedgerAccount(ledger, storedInAccount);
        }

        public LastLedgerBookLoadedV1 PreparePersistentStateData()
        {
            return new LastLedgerBookLoadedV1
            {
                LedgerBookStorageKey = this.book == null ? null : this.book.FileName
            };
        }

        public void RemoveReconciliation(LedgerEntryLine line)
        {
            if (line == null)
            {
                throw new ArgumentNullException("line");
            }

            this.book.RemoveLine(line);
        }

        /// <summary>
        ///     Removes the transaction from the specified Ledger Entry. The Ledger Entry must exist in the current Ledger Book.
        /// </summary>
        public void RemoveTransaction(LedgerEntry ledgerEntry, Guid transactionId)
        {
            if (ledgerEntry == null)
            {
                throw new ArgumentNullException("ledgerEntry");
            }

            if (this.book.Reconciliations.First().Entries.Any(e => e == ledgerEntry))
            {
                throw new ArgumentException("Ledger Entry provided does not exist in the current Ledger Book.", "ledgerEntry");
            }

            ledgerEntry.RemoveTransaction(transactionId);
        }

        public void RenameLedgerBook(LedgerBook ledgerBook, string newName)
        {
            if (ledgerBook == null)
            {
                throw new ArgumentNullException("ledgerBook");
            }
            if (newName == null)
            {
                throw new ArgumentNullException("newName");
            }

            ledgerBook.Name = newName;
        }

        public void Save(LedgerBook ledgerBook, string storageKey = null)
        {
            if (ledgerBook == null)
            {
                throw new ArgumentNullException("ledgerBook");
            }
            if (string.IsNullOrWhiteSpace(storageKey))
            {
                this.ledgerRepository.Save(ledgerBook);
            }
            else
            {
                this.ledgerRepository.Save(ledgerBook, storageKey);
            }
        }

        public LedgerBucket TrackNewBudgetBucket(ExpenseBucket bucket, AccountType storeInThisAccount)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException("bucket");
            }
            if (storeInThisAccount == null)
            {
                throw new ArgumentNullException("storeInThisAccount");
            }

            return this.book.AddLedger(bucket, storeInThisAccount);
        }

        public LedgerEntryLine UnlockCurrentMonth()
        {
            return this.book.UnlockMostRecentLine();
        }

        /// <summary>
        ///     Updates the remarks for the given Ledger Entry Line. The Ledger Entry Line must exist in the current Ledger Book.
        /// </summary>
        public void UpdateRemarks(LedgerEntryLine entryLine, string remarks)
        {
            if (entryLine == null)
            {
                throw new ArgumentNullException("entryLine");
            }

            if (remarks == null)
            {
                throw new ArgumentNullException("remarks");
            }

            if (this.book.Reconciliations.All(l => l != entryLine))
            {
                throw new ArgumentException("Ledger Entry Line provided does not exist in the current Ledger Book.", "entryLine");
            }

            entryLine.UpdateRemarks(remarks);
        }

        /// <summary>
        ///     Returns a list of valid accounts for use with the Ledger Book.
        /// </summary>
        public IEnumerable<AccountType> ValidLedgerAccounts()
        {
            return this.accountTypeRepository.ListCurrentlyUsedAccountTypes();
        }
    }
}