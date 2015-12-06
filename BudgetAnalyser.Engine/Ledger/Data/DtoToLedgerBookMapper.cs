using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.BankAccount;

namespace BudgetAnalyser.Engine.Ledger.Data
{
    /// <summary>
    ///     An overriden mapper class to allow custom initialisation to be done after the base <see cref="LedgerBook" /> has
    ///     been created and mapped.
    ///     For example: Must make sure that the <see cref="LedgerBook.Ledgers" /> Collection is populated and each one has a
    ///     default storage Account.
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true, RegisterAs = typeof(BasicMapper<LedgerBookDto, LedgerBook>))]
    public class DtoToLedgerBookMapper : MagicMapper<LedgerBookDto, LedgerBook>
    {
        private readonly IAccountTypeRepository accountRepo;
        private readonly Dictionary<string, LedgerBucket> cachedLedgers = new Dictionary<string, LedgerBucket>();

        public DtoToLedgerBookMapper([NotNull] IAccountTypeRepository accountRepo)
        {
            if (accountRepo == null)
            {
                throw new ArgumentNullException(nameof(accountRepo));
            }

            this.accountRepo = accountRepo;
        }

        public override LedgerBook Map(LedgerBookDto source)
        {
            LedgerBook book = base.Map(source);

            this.cachedLedgers.Clear();
            foreach (LedgerBucket ledgerBucket in book.Ledgers)
            {
                if (ledgerBucket.StoredInAccount == null)
                {
                    // Defaults to Cheque Account if unspecified.
                    ledgerBucket.StoredInAccount = this.accountRepo.GetByKey(AccountTypeRepositoryConstants.Cheque);
                }

                GetOrAddFromCache(ledgerBucket);
            }

            bool ledgersMapWasEmpty = book.Ledgers.None();

            // Default to CHEQUE when StoredInAccount is null.
            foreach (LedgerEntryLine line in book.Reconciliations)
            {
                foreach (LedgerEntry entry in line.Entries)
                {
                    // Ensure the ledger bucker is the same instance as listed in the book.Legders;
                    entry.LedgerBucket = GetOrAddFromCache(entry.LedgerBucket, true);
                    if (entry.LedgerBucket.StoredInAccount == null)
                    {
                        entry.LedgerBucket.StoredInAccount = this.accountRepo.GetByKey(AccountTypeRepositoryConstants.Cheque);
                    }
                }
            }

            // If ledger column map at the book level was empty, default it to the last used ledger columns in the Dated Entries.
            if (ledgersMapWasEmpty && book.Reconciliations.Any())
            {
                book.Ledgers = book.Reconciliations.First().Entries.Select(e => e.LedgerBucket);
            }

            return book;
        }

        private LedgerBucket GetOrAddFromCache(LedgerBucket ledger, bool throwIfNotFound = false)
        {
            if (this.cachedLedgers.ContainsKey(ledger.BudgetBucket.Code))
            {
                return this.cachedLedgers[ledger.BudgetBucket.Code];
            }

            if (throwIfNotFound)
            {
                throw new IndexOutOfRangeException($"Ledger Bucket {ledger.BudgetBucket.Code} not found in cache.");
            }

            this.cachedLedgers.Add(ledger.BudgetBucket.Code, ledger);
            return ledger;
        }
    }
}