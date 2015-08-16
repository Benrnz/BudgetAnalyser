using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Statement.Data
{
    public class TransactionSetDto
    {
        public long Checksum { get; set; }
        public DateTime LastImport { get; set; }
        public string StorageKey { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Necessary for persistence")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Necessary for persistence")]
        public List<TransactionDto> Transactions { get; set; }

        public string VersionHash { get; set; }
    }
}