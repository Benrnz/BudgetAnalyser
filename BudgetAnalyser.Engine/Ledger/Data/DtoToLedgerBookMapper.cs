using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Ledger.Data
{
    /// <summary>
    /// An overriden mapper class to allow custom initialisation to be done after the base <see cref="LedgerBook"/> has been created and mapped.
    /// For example: Must make sure that the <see cref="LedgerBook.Ledgers"/> Collection is populated and each one has a default storage Account.
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true, RegisterAs = typeof(BasicMapper<LedgerBookDto, LedgerBook>))]
    public class DtoToLedgerBookMapper : MagicMapper<LedgerBookDto, LedgerBook>
    {
        private readonly IAccountTypeRepository accountRepo;
        private readonly Dictionary<LedgerBucket, LedgerBucket> cachedLedgers = new Dictionary<LedgerBucket, LedgerBucket>();

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

        private void GetOrAddFromCache(LedgerBucket key)
        {
            if (this.cachedLedgers.ContainsKey(key))
            {
                return;
            }

            this.cachedLedgers.Add(key, key);
        }
    }
}