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

        public double? Checksum { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Required for serialisation")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Required for serialisation")]
        public List<DataLedgerEntryLine> DatedEntries { get; set; }

        public string FileName { get; set; }

        public DateTime Modified { get; set; }
        public string Name { get; set; }
    }
}