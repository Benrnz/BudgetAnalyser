using System;

namespace BudgetAnalyser.Engine.Ledger.Data
{
    /// <summary>
    ///     A Dto for <see cref="LedgerBucket" />.
    /// </summary>
    public class LedgerBucketDto
    {
        /// <summary>
        ///     Instantiate a new instance of the <see cref="LedgerBucketDto" /> class.
        /// </summary>
        public LedgerBucketDto()
        {
            Id = Guid.NewGuid();
        }

        /// <summary>
        ///     Gets or sets the bucket code.
        /// </summary>
        public string BucketCode { get; set; }

        /// <summary>
        ///     A unique token to identify this ledger bucket when using the code is not appropriate.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        ///     Gets or sets the bank account that this ledger bucket is stored in.
        /// </summary>
        public string StoredInAccount { get; set; }
    }
}