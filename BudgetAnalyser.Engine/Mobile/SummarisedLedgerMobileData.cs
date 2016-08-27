using System;
using System.Collections.Generic;

namespace BudgetAnalyser.Engine.Mobile
{
    /// <summary>
    ///     A DTO type to export all data about the state of the ledger at a point in time.
    /// </summary>
    public class SummarisedLedgerMobileData
    {
        /// <summary>
        ///     The date and time this object was exported from Budget Analyser
        /// </summary>
        public DateTime Exported { get; set; }

        /// <summary>
        ///     The date and time transactions were last imported into BudgetAnalyser from the bank.
        /// </summary>
        public DateTime LastTransactionImport { get; set; }

        /// <summary>
        ///     All the ledger buckets in the ledger
        /// </summary>
        public List<SummarisedLedgerBucket> LedgerBuckets { get; set; }

        /// <summary>
        ///     The title of the budget
        /// </summary>
        public string Title { get; set; }
    }
}