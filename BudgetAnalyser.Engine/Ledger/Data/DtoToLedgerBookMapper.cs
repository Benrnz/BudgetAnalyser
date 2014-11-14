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
        private readonly Dictionary<LedgerColumn, LedgerColumn> cachedLedgers = new Dictionary<LedgerColumn, LedgerColumn>();

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
            foreach (LedgerColumn ledgerColumn in book.Ledgers)
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
            foreach (LedgerEntryLine line in book.DatedEntries)
            {
                foreach (LedgerEntry entry in line.Entries)
                {
                    if (entry.LedgerColumn.StoredInAccount == null)
                    {
                        entry.LedgerColumn.StoredInAccount = this.accountRepo.GetByKey(AccountTypeRepositoryConstants.Cheque);
                    }
                }
            }

            // If ledger column map at the book level was empty, default it to the last used ledger columns in the Dated Entries.
            if (ledgersMapWasEmpty && book.DatedEntries.Any())
            {
                book.Ledgers = book.DatedEntries.First().Entries.Select(e => e.LedgerColumn);
            }

            return book;
        }

        private void GetOrAddFromCache(LedgerColumn key)
        {
            if (this.cachedLedgers.ContainsKey(key))
            {
                return;
            }

            this.cachedLedgers.Add(key, key);
        }
    }
}