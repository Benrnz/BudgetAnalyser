using System;
using System.Diagnostics;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Ledger
{
    public static class LedgerCalculation
    {
        public static decimal LocateApplicableLedgerBalance(LedgerBook ledgerBook, GlobalFilterCriteria filter, string bucketCode)
        {
            LedgerEntryLine line = LocateApplicableLedgerLine(ledgerBook, filter);
            if (line == null)
            {
                return 0;
            }

            return (from ledgerEntry in line.Entries where ledgerEntry.LedgerColumn.BudgetBucket.Code == bucketCode select ledgerEntry.Balance).FirstOrDefault();
        }

        public static LedgerEntryLine LocateApplicableLedgerLine(LedgerBook ledgerBook, [NotNull] GlobalFilterCriteria filter)
        {
            if (ledgerBook == null)
            {
                return null;
            }

            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }

            if (filter.Cleared)
            {
                return ledgerBook.DatedEntries.FirstOrDefault();
            }

            Debug.Assert(filter.BeginDate != null);
            Debug.Assert(filter.EndDate != null);
            LedgerEntryLine line = ledgerBook.DatedEntries.FirstOrDefault(ledgerEntryLine => ledgerEntryLine.Date >= filter.BeginDate.Value && ledgerEntryLine.Date <= filter.EndDate.Value);
            if (line == null)
            {
                return null;
            }

            return line;
        }
    }
}