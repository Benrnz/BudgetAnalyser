using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Ledger.Data
{
    public class LedgerEntryDto
    {
        public LedgerEntryDto()
        {
            Transactions = new List<LedgerTransactionDto>();
        }

        public decimal Balance { get; set; }

        public string BucketCode { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Required for serialisation")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Required for serialisation")]
        public List<LedgerTransactionDto> Transactions { get; set; }
    }
}