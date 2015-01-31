using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Ledger.Data
{
    [AutoRegisterWithIoC(SingleInstance = true, RegisterAs = typeof(BasicMapper<LedgerBookDto, LedgerBook>))]
    public class DtoToLedgerBookMapper : MagicMapper<LedgerBookDto, LedgerBook>
    {
        private readonly IAccountTypeRepository accountRepo;
        private readonly Dictionary<LedgerBucket, LedgerBucket> cachedLedgers = new Dictionary<LedgerBucket, LedgerBucket>();

        public DtoToLedgerBookMapper([NotNull] IAccountTypeRepository accountRepo)
        {
            if (accountRepo == null)
            {
                throw new ArgumentNullException("accountRepo");
            }

            this.accountRepo = accountRepo;
        }

        public override LedgerBook Map(LedgerBookDto source)
        {
            LedgerBook book = base.Map(source);

            this.cachedLedgers.Clear();
            foreach (LedgerBucket ledgerColumn in book.Ledgers)
            {
                if (ledgerColumn.StoredInAccount == null)
                {
                    // Defaults to Cheque Account if unspecified.
                    ledgerColumn.StoredInAccount = this.accountRepo.GetByKey(AccountTypeRepositoryConstants.Cheque);
                }

                GetOrAddFromCache(ledgerColumn);
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