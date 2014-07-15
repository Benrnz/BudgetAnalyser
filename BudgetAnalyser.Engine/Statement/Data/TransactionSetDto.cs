using System;
using System.Collections.Generic;

namespace BudgetAnalyser.Engine.Statement.Data
{
    public class TransactionSetDto
    {
        public long Checksum { get; set; }

        public string FileName { get; set; }

        public DateTime LastImport { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Necessary for persistence")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Necessary for persistence")]
        public List<TransactionDto> Transactions { get; set; }

        public string VersionHash { get; set; }
    }
}