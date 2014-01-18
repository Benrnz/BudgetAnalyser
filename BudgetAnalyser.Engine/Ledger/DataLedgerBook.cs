using System;
using System.Collections.Generic;

namespace BudgetAnalyser.Engine.Ledger
{
    public class DataLedgerBook
    {
        public DataLedgerBook()
        {
            DatedEntries = new List<DataLedgerEntryLine>();
        }

        public List<DataLedgerEntryLine> DatedEntries { get; set; }

        public string FileName { get; set; }

        public DateTime Modified { get; set; }
        public string Name { get; set; }

        public double? Checksum { get; set; }
    }
}