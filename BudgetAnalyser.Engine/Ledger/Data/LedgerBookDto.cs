using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Ledger.Data
{
    public class LedgerBookDto
    {
        public LedgerBookDto()
        {
            DatedEntries = new List<LedgerEntryLineDto>();
        }

        public double Checksum { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Required for serialisation")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Required for serialisation")]
        public List<LedgerEntryLineDto> DatedEntries { get; set; }

        public string FileName { get; set; }

        public DateTime Modified { get; set; }
        public string Name { get; set; }
    }
}