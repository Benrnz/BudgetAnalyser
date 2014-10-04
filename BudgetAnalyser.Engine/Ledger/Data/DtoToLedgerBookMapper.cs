using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Ledger.Data
{
    [AutoRegisterWithIoC(SingleInstance = true, RegisterAs = typeof(BasicMapper<LedgerBookDto, LedgerBook>))]
    public class DtoToLedgerBookMapper : MagicMapper<LedgerBookDto, LedgerBook>
    {
        private readonly IAccountTypeRepository accountRepo;
        private readonly ConcurrentDictionary<string, LedgerColumn> cachedLedgers = new ConcurrentDictionary<string, LedgerColumn>();

        public DtoToLedgerBookMapper([NotNull] IAccountTypeRepository accountRepo)
        {
            if (accountRepo == null)
            {
                throw new ArgumentNullException("accountRepo");
            }

            this.accountRepo = accountRepo;
        }

        public override LedgerBook Map([NotNull] LedgerBookDto source)
        {
            LedgerBook book = base.Map(source);

            this.cachedLedgers.Clear();
            foreach (var ledgerColumn in book.Ledgers)
            {
                if (ledgerColumn.StoredInAccount == null)
                {
                    // Defaults to Cheque Account if unspecified.
                    ledgerColumn.StoredInAccount = this.accountRepo.GetByKey(AccountTypeRepositoryConstants.Cheque);
                }

                // Add the outer ledgers map collection to the cache
                this.cachedLedgers.GetOrAdd(ledgerColumn.BudgetBucket.Code, ledgerColumn);
            }

            bool ledgersMapWasEmpty = !book.Ledgers.Any();

            // Make sure there are no duplicate LedgerColumn instances.
            // Also make sure there are no BudgetBucket duplicates.
            foreach (LedgerEntryLine line in book.DatedEntries)
            {
                foreach (LedgerEntry entry in line.Entries)
                {
                    entry.LedgerColumn = this.cachedLedgers.GetOrAdd(entry.LedgerColumn.BudgetBucket.Code, code => entry.LedgerColumn);
                    if (entry.LedgerColumn.StoredInAccount == null)
                    {
                        // Outer Ledgers map collection was empty - will need to default the ledgers to CHEQUE.
                        entry.LedgerColumn.StoredInAccount = this.accountRepo.GetByKey(AccountTypeRepositoryConstants.Cheque);
                    }
                }
            }

            if (ledgersMapWasEmpty)
            {
                book.Ledgers = this.cachedLedgers.Values.ToList();
            }

            return book;
        }
    }
}