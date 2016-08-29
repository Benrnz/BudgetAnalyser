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
        /// Instantiate a new instance of <see cref="SummarisedLedgerMobileData"/>
        /// </summary>
        public SummarisedLedgerMobileData()
        {
            LedgerBuckets = new List<SummarisedLedgerBucket>();
        }
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
        public List<SummarisedLedgerBucket> LedgerBuckets { get; private set; }

        /// <summary>
        ///     The date this month started
        /// </summary>
        public DateTime StartOfMonth { get; set; }

        /// <summary>
        ///     The title of the budget
        /// </summary>
        public string Title { get; set; }
    }
}