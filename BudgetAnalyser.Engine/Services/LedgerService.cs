using System;
using System.Collections.Generic;
using System.IO;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Services
{
    [AutoRegisterWithIoC]
    public class LedgerService : ILedgerService
    {
        private readonly ILedgerBookRepository ledgerRepository;
        private LedgerBook book;

        public LedgerService([NotNull] ILedgerBookRepository ledgerRepository)
        {
            if (ledgerRepository == null)
            {
                throw new ArgumentNullException("ledgerRepository");
            }

            this.ledgerRepository = ledgerRepository;
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
        /// <param name="date">
        ///     The date for the <see cref="LedgerEntryLine" />. Also used to search for transactions in the
        ///     <see cref="statement" />. This date ideally is your payday of the month, and should be the same date
        ///     every month. Transactions are searched for up to but not including this date.
        /// </param>
        /// <param name="balances">
        ///     The bank balances as at the <see cref="date" /> to include in this new single line of the
        ///     ledger book.
        /// </param>
        /// <param name="budgetContext">The current budget context.</param>
        /// <param name="statement">The currently loaded statement.</param>
        /// <param name="ignoreWarnings">Ignores validation warnings if true, otherwise <see cref="ValidationWarningException" />.</param>
        /// <exception cref="System.ArgumentNullException">
        ///     balances or budgetContext or statement
        /// </exception>
        /// <exception cref="System.InvalidOperationException">Reconciling against an inactive budget is invalid.</exception>
        public LedgerEntryLine MonthEndReconciliation(DateTime date, IEnumerable<BankBalance> balances, IBudgetCurrencyContext budgetContext, StatementModel statement, bool ignoreWarnings = false)
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

            return this.book.Reconcile(date, balances, budgetContext.Model, statement, ignoreWarnings);
        }

        public void MoveLedgerToAccount(LedgerBook ledgerBook, LedgerColumn ledger, AccountType storedInAccount)
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

        public void RemoveReconciliation(LedgerEntryLine line)
        {
            if (line == null)
            {
                throw new ArgumentNullException("line");
            }

            this.book.RemoveLine(line);
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

        public LedgerColumn TrackNewBudgetBucket(ExpenseBucket bucket, AccountType storeInThisAccount)
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
    }
}