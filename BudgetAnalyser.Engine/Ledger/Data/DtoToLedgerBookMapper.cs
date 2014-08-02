using System.Collections.Concurrent;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Ledger.Data
{
    [AutoRegisterWithIoC(SingleInstance = true, RegisterAs = typeof(BasicMapper<LedgerBookDto, LedgerBook>))]
    public class DtoToLedgerBookMapper : MagicMapper<LedgerBookDto, LedgerBook>
    {
        private static readonly ConcurrentDictionary<string, LedgerColumn> CachedLedgers = new ConcurrentDictionary<string, LedgerColumn>();

        public override LedgerBook Map([NotNull] LedgerBookDto source)
        {
            LedgerBook book = base.Map(source);

            // Make sure all instances of LedgerColumn are using the same instance for the one Bucket Code.
            foreach (LedgerEntryLine line in book.DatedEntries)
            {
                foreach (LedgerEntry entry in line.Entries)
                {
                    entry.LedgerColumn = CachedLedgers.GetOrAdd(entry.LedgerColumn.BudgetBucket.Code, code => entry.LedgerColumn);
                }
            }

            return book;
        }
    }
}