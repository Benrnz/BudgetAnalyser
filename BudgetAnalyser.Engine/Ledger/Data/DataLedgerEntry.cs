using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Ledger.Data
{
    public class DataLedgerEntry
    {
        public DataLedgerEntry()
        {
            this.Transactions = new List<DataLedgerTransaction>();
        }

        public decimal Balance { get; set; }

        public string BucketCode { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Required for serialisation")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Required for serialisation")]
        public List<DataLedgerTransaction> Transactions { get; set; }
    }
}