using System.Collections.Generic;

namespace BudgetAnalyser.Engine.Ledger
{
    public class DataLedgerEntry
    {
        public DataLedgerEntry()
        {
            Transactions = new List<DataLedgerTransaction>();
        }

        public decimal Balance { get; set; }

        public string BucketCode { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Required for serialisation")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Required for serialisation")]
        public List<DataLedgerTransaction> Transactions { get; set; }
    }
}