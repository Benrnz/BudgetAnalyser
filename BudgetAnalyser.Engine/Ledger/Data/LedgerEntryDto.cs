using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Ledger.Data
{
    /// <summary>
    /// A Dto for <see cref="LedgerEntry"/>.
    /// </summary>
    public class LedgerEntryDto
    {
        public LedgerEntryDto()
        {
            Transactions = new List<LedgerTransactionDto>();
        }

        /// <summary>
        /// The Balance of the Ledger as at the date in the parent LedgerLine
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// The Budget Bucket being tracked.  
        /// The LedgerBucketDto type was intentionally not used here, to prevent the same instance being used between ledger lines and the "next reconciliation" mapping at the LedgerBookDto level.
        /// </summary>
        public string BucketCode { get; set; }

        /// <summary>
        /// The account in which the Ledger was stored in at the time of the month end reconciliation for this line.
        /// </summary>
        public string StoredInAccount { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Required for serialisation")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Required for serialisation")]
        public List<LedgerTransactionDto> Transactions { get; set; }
    }
}