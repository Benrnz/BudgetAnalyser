using System.Collections.Concurrent;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Ledger.Data
{
    [AutoRegisterWithIoC(SingleInstance = true, RegisterAs = typeof(BasicMapper<LedgerBookDto, LedgerBook>))]
    public class DtoToLedgerBookMapper : MagicMapper<LedgerBookDto, LedgerBook>
    {
        private readonly ConcurrentDictionary<string, LedgerColumn> cachedLedgers = new ConcurrentDictionary<string, LedgerColumn>();

        public override LedgerBook Map([NotNull] LedgerBookDto source)
        {
            LedgerBook book = base.Map(source);

            this.cachedLedgers.Clear();
            foreach (var ledgerColumn in book.Ledgers)
            {
                this.cachedLedgers.GetOrAdd(ledgerColumn.BudgetBucket.Code, ledgerColumn);
            }

            // Make sure there are no duplicate LedgerColumn instances.
            // Also make sure there are no BudgetBucket duplicates.
            foreach (LedgerEntryLine line in book.DatedEntries)
            {
                foreach (LedgerEntry entry in line.Entries)
                {
                    entry.LedgerColumn = this.cachedLedgers.GetOrAdd(entry.LedgerColumn.BudgetBucket.Code, code => entry.LedgerColumn);
                }
            }

            return book;
        }
    }
}