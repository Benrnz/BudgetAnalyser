using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Ledger.Data
{
    /// <summary>
    /// A Dto for <see cref="LedgerBook"/>
    /// </summary>
    public class LedgerBookDto
    {
        public LedgerBookDto()
        {
            DatedEntries = new List<LedgerEntryLineDto>();
            Ledgers = new List<LedgerColumnDto>();
        }

        /// <summary>
        /// A hash value to verify once loaded from a file.
        /// </summary>
        public double Checksum { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Required for serialisation")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Required for serialisation")]
        public List<LedgerEntryLineDto> DatedEntries { get; set; }

        public string FileName { get; set; }

        public List<LedgerColumnDto> Ledgers { get; set; }

        public DateTime Modified { get; set; }
        public string Name { get; set; }
    }
}