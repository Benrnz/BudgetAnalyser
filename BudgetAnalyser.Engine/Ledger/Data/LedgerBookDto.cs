using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Ledger.Data
{
    /// <summary>
    ///     A Dto for <see cref="LedgerBook" />
    /// </summary>
    public class LedgerBookDto
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LedgerBookDto" /> class.
        /// </summary>
        public LedgerBookDto()
        {
            Reconciliations = new List<LedgerEntryLineDto>();
            Ledgers = new List<LedgerBucketDto>();
        }

        /// <summary>
        ///     A hash value to verify once loaded from a file.
        /// </summary>
        public double Checksum { get; set; }

        /// <summary>
        ///     The ledger to Bucket mapping for when a new reconciliation creates a new instances of LedgerEntry's.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "Required for serialisation")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Scope = "member",
            Target = "BudgetAnalyser.Engine.Ledger.Data.LedgerBookDto.#Ledgers",
            Justification = "Required for serialisation")]
        [UsedImplicitly]
        public List<LedgerBucketDto> Ledgers { get; set; }

        /// <summary>
        ///     The configuration for the remote mobile data storage
        /// </summary>
        public MobileStorageSettingsDto MobileSettings { get; set; }

        /// <summary>
        ///     Gets or sets the last modified date.
        /// </summary>
        public DateTime Modified { get; set; }

        /// <summary>
        ///     Gets or sets the ledger book name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the reconciliations collection.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "Required for serialisation")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists",
            Justification = "Required for serialisation")]
        public List<LedgerEntryLineDto> Reconciliations { get; set; }

        /// <summary>
        ///     Gets or sets the storage key.
        /// </summary>
        public string StorageKey { get; set; }
    }
}